using Npgsql;
using Testcontainers.PostgreSql;

namespace BccSafety.Tests.Tenancy;

/// <summary>
/// Deze testklasse is het enige echte bewijs dat de tenantscheiding werkt.
/// Sloopt iemand de interceptor of vergeet iemand een policy op een nieuwe
/// tabel, dan valt hier iets om. Laat hem draaien in CI, niet alleen lokaal.
///
/// De tests praten bewust rechtstreeks met Npgsql als rol bcc_app, zonder
/// EF ertussen. Global query filters zouden anders de indruk wekken dat de
/// scheiding werkt terwijl het de filters zijn die filteren.
/// </summary>
public sealed class RlsIsolationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid PersoonA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid PersoonB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    private string _appConnectionString = default!;

    public async Task InitializeAsync()
    {
        await _pg.StartAsync();

        // Schema en policies draaien als eigenaar.
        await using var owner = new NpgsqlConnection(_pg.GetConnectionString());
        await owner.OpenAsync();
        await Migrations.ApplyAsync(owner);   // schema + 001_tenancy_rls.sql
        await SeedAsync(owner);

        var builder = new NpgsqlConnectionStringBuilder(_pg.GetConnectionString())
        {
            Username = "bcc_app",
            Password = "test",
            Multiplexing = false   // sessievariabelen overleven multiplexing niet
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

    [Fact]
    public async Task Tenant_ziet_alleen_eigen_personen()
    {
        await using var conn = await OpenAsAsync(TenantA);
        Assert.Equal(1, await ScalarAsync(conn, "SELECT count(*) FROM persoon"));
        Assert.Equal(0, await ScalarAsync(conn,
            "SELECT count(*) FROM persoon WHERE id = @id", ("id", PersoonB)));
    }

    [Fact]
    public async Task Zonder_tenant_zie_je_niets()
    {
        // Faalt-dicht. Als dit ooit een rij teruggeeft, is current_tenant()
        // niet meer NULL-veilig of staat er ergens een policy zonder filter.
        await using var conn = await OpenAsAsync(null);
        Assert.Equal(0, await ScalarAsync(conn, "SELECT count(*) FROM persoon"));
    }

    [Fact]
    public async Task Bijwerken_van_vreemde_rij_raakt_nul_rijen()
    {
        await using var conn = await OpenAsAsync(TenantA);
        await using var cmd = new NpgsqlCommand(
            "UPDATE persoon SET achternaam = 'gehackt' WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", PersoonB);
        Assert.Equal(0, await cmd.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task Invoegen_onder_vreemde_tenant_wordt_geweigerd()
    {
        await using var conn = await OpenAsAsync(TenantA);
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO persoon (id, tenant_id, voornaam, achternaam, email)
            VALUES (gen_random_uuid(), @tenant, 'Mallory', 'Smokkel', 'm@x.nl')
            """, conn);
        cmd.Parameters.AddWithValue("tenant", TenantB);

        var ex = await Assert.ThrowsAsync<PostgresException>(
            () => cmd.ExecuteNonQueryAsync());
        Assert.Equal("42501", ex.SqlState);   // WITH CHECK-schending
    }

    [Fact]
    public async Task Gasttenant_ziet_gedeeld_evenement_maar_kan_het_niet_wijzigen()
    {
        await using var conn = await OpenAsAsync(TenantB);

        // A is eigenaar, B is geaccepteerde gast: lezen mag.
        Assert.Equal(1, await ScalarAsync(conn,
            "SELECT count(*) FROM evenement WHERE titel = 'Landelijke dag'"));

        // Wijzigen mag niet: dat blijft bij de eigenaar-tenant.
        await using var cmd = new NpgsqlCommand(
            "UPDATE evenement SET titel = 'Gekaapt' WHERE titel = 'Landelijke dag'",
            conn);
        Assert.Equal(0, await cmd.ExecuteNonQueryAsync());
    }

    [Fact]
    public async Task Elke_tabel_met_tenant_id_heeft_rls_aan()
    {
        // Vangt de nieuwe tabel die iemand over een half jaar toevoegt en
        // waarbij ENABLE ROW LEVEL SECURITY vergeten wordt.
        await using var owner = new NpgsqlConnection(_pg.GetConnectionString());
        await owner.OpenAsync();
        var zonderRls = await ScalarAsync(owner,
            """
            SELECT count(*) FROM pg_tables t
            JOIN pg_class c ON c.relname = t.tablename
            WHERE t.schemaname = 'public'
              AND (c.relrowsecurity = false OR c.relforcerowsecurity = false)
            """);
        Assert.Equal(0, zonderRls);
    }
}
