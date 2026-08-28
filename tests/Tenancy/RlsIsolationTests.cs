using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace BccSafety.Tests.Tenancy;

/// <summary>
/// This test class is the only real proof that tenant separation works.
/// If someone breaks the interceptor or forgets a policy on a new table,
/// something here falls over. Run it in CI, not just locally.
///
/// The tests deliberately talk straight to Npgsql as role bcc_app,
/// without EF in between. Global query filters would otherwise give the
/// impression that separation works when it's actually the filters doing
/// the filtering.
/// </summary>
public sealed class RlsIsolationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PersonA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid PersonB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid LocationA = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid EventTypeA = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid NationalDayEvent = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");

    private string _appConnectionString = default!;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        // Schema and policies run as owner.
        await using var owner = new NpgsqlConnection(_pg.GetConnectionString());
        await owner.OpenAsync();
        await Migrations.ApplyAsync(owner);   // schema + 001_tenancy_rls.sql
        await SeedAsync(owner);

        var builder = new NpgsqlConnectionStringBuilder(_pg.GetConnectionString())
        {
            Username = "bcc_app",
            Password = "test",
            Multiplexing = false   // session variables don't survive multiplexing
        };
        _appConnectionString = builder.ToString();
    }

    public Task DisposeAsync() => _pg.DisposeAsync().AsTask();

    private async Task<NpgsqlConnection> OpenAsAsync(Guid? tenantId)
    {
        var conn = new NpgsqlConnection(_appConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT set_config('app.tenant_id', @t, false)", conn);
        cmd.Parameters.AddWithValue("t", tenantId?.ToString() ?? string.Empty);
        await cmd.ExecuteNonQueryAsync();
        return conn;
    }

    private static async Task<long> ScalarAsync(
        NpgsqlConnection conn, string sql, params (string, object)[] args)
    {
        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (name, value) in args) cmd.Parameters.AddWithValue(name, value);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync());
    }

    /// <summary>
    /// Two tenants with one person each, and an event owned by tenant A
    /// that has tenant B as an accepted guest — needed for the guest
    /// tenant test. Runs as owner, so straight SQL, not through the app role.
    /// </summary>
    private static async Task SeedAsync(NpgsqlConnection owner)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO tenant (id, name, slug, active, created_at) VALUES
                (@tenantA, 'Tenant A', 'tenant-a', true, now()),
                (@tenantB, 'Tenant B', 'tenant-b', true, now());

            INSERT INTO person (id, tenant_id, first_name, last_name, date_of_birth, email, status) VALUES
                (@personA, @tenantA, 'Anna', 'Example', '1990-01-01', 'a@example.test', 'Active'),
                (@personB, @tenantB, 'Bob', 'Example', '1990-01-01', 'b@example.test', 'Active');

            INSERT INTO location (id, tenant_id, name)
                VALUES (@locationA, @tenantA, 'Main location');

            INSERT INTO event_type (id, tenant_id, name, active)
                VALUES (@typeA, @tenantA, 'Test type', true);

            INSERT INTO event
                (id, tenant_id, event_type_id, location_id, title, start, "end", status, source)
                VALUES (@event, @tenantA, @typeA, @locationA, 'National day',
                        now(), now() + interval '2 hours', 'Scheduled', 'Manual');

            INSERT INTO event_guest_tenant (event_id, tenant_id, owner_tenant_id, status)
                VALUES (@event, @tenantB, @tenantA, 'Accepted');
            """, owner);
        cmd.Parameters.AddWithValue("tenantA", TenantA);
        cmd.Parameters.AddWithValue("tenantB", TenantB);
        cmd.Parameters.AddWithValue("personA", PersonA);
        cmd.Parameters.AddWithValue("personB", PersonB);
        cmd.Parameters.AddWithValue("locationA", LocationA);
        cmd.Parameters.AddWithValue("typeA", EventTypeA);
        cmd.Parameters.AddWithValue("event", NationalDayEvent);
        await cmd.ExecuteNonQueryAsync();
    }

    [Fact]
    public async Task Tenant_sees_only_its_own_people()
    {
        await using var conn = await OpenAsAsync(TenantA);
        Assert.Equal(1, await ScalarAsync(conn, "SELECT count(*) FROM person"));
        Assert.Equal(0, await ScalarAsync(conn,
            "SELECT count(*) FROM person WHERE id = @id", ("id", PersonB)));
    }

    [Fact]
    public async Task Without_a_tenant_you_see_nothing()
    {
        // Fails closed. If this ever returns a row, current_tenant() is no
        // longer NULL-safe, or some policy is missing its filter.
        await using var conn = await OpenAsAsync(null);
        Assert.Equal(0, await ScalarAsync(conn, "SELECT count(*) FROM person"));
    }

    [Fact]
    public async Task Updating_a_foreign_row_affects_zero_rows()
    {
        await using var conn = await OpenAsAsync(TenantA);
        await using var cmd = new NpgsqlCommand(
            "UPDATE person SET last_name = 'hacked' WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", PersonB);
        Assert.Equal(0, await cmd.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task Inserting_under_a_foreign_tenant_is_rejected()
    {
        await using var conn = await OpenAsAsync(TenantA);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO person (id, tenant_id, first_name, last_name, date_of_birth, email)
            VALUES (gen_random_uuid(), @tenant, 'Mallory', 'Smuggle', '1990-01-01', 'm@x.nl')
            """, conn);
        cmd.Parameters.AddWithValue("tenant", TenantB);

        var ex = await Assert.ThrowsAsync<PostgresException>(
            () => cmd.ExecuteNonQueryAsync());
        Assert.Equal("42501", ex.SqlState);   // WITH CHECK violation
    }

    [Fact]
    public async Task Guest_tenant_sees_shared_event_but_cannot_change_it()
    {
        await using var conn = await OpenAsAsync(TenantB);

        // A is owner, B is an accepted guest: reading is allowed.
        Assert.Equal(1, await ScalarAsync(conn,
            "SELECT count(*) FROM event WHERE title = 'National day'"));

        // Changing is not allowed: that stays with the owner tenant.
        await using var cmd = new NpgsqlCommand(
            "UPDATE event SET title = 'Hijacked' WHERE title = 'National day'",
            conn);
        Assert.Equal(0, await cmd.ExecuteNonQueryAsync());
    }

    // Tables that deliberately have no RLS, with a reason. Never add a
    // tenant table here just to make this test pass — the whole point of
    // the test is that it falls over on a forgotten policy.
    private static readonly string[] NoTenantDataExceptions =
    [
        "__EFMigrationsHistory", // EF Core's own migration table, not tenant data
    ];

    [Fact]
    public async Task Every_table_with_tenant_id_has_rls_on()
    {
        // Catches the new table someone adds six months from now and
        // forgets to ENABLE ROW LEVEL SECURITY on.
        await using var owner = new NpgsqlConnection(_pg.GetConnectionString());
        await owner.OpenAsync();
        var withoutRls = await ScalarAsync(owner,
            """
            SELECT count(*) FROM pg_tables t
            JOIN pg_class c ON c.relname = t.tablename
            WHERE t.schemaname = 'public'
              AND t.tablename <> ALL(@excluded)
              AND (c.relrowsecurity = false OR c.relforcerowsecurity = false)
            """,
            ("excluded", NoTenantDataExceptions));
        Assert.Equal(0, withoutRls);
    }
}
