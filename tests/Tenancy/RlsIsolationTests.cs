using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

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
    private static readonly Guid LocatieA = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid EvenementtypeA = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid EvenementLandelijkeDag = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");

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

    /// <summary>
    /// Twee tenants met elk één persoon, en een evenement van tenant A dat
    /// tenant B als geaccepteerde gast heeft — nodig voor de gasttenant-test.
    /// Draait als eigenaar, dus rechtstreeks via SQL, niet via de app-rol.
    /// </summary>
    private static async Task SeedAsync(NpgsqlConnection owner)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO tenant (id, naam, slug, actief, aangemaakt_op) VALUES
                (@tenantA, 'Tenant A', 'tenant-a', true, now()),
                (@tenantB, 'Tenant B', 'tenant-b', true, now());

            INSERT INTO persoon (id, tenant_id, voornaam, achternaam, geboortedatum, email, status) VALUES
                (@persoonA, @tenantA, 'Anna', 'Voorbeeld', '1990-01-01', 'a@voorbeeld.test', 'actief'),
                (@persoonB, @tenantB, 'Bob', 'Voorbeeld', '1990-01-01', 'b@voorbeeld.test', 'actief');

            INSERT INTO locatie (id, tenant_id, naam)
                VALUES (@locatieA, @tenantA, 'Hoofdlocatie');

            INSERT INTO evenementtype (id, tenant_id, naam, actief)
                VALUES (@typeA, @tenantA, 'Testtype', true);

            INSERT INTO evenement
                (id, tenant_id, evenementtype_id, locatie_id, titel, start, eind, status, bron)
                VALUES (@evenement, @tenantA, @typeA, @locatieA, 'Landelijke dag',
                        now(), now() + interval '2 hours', 'gepland', 'handmatig');

            INSERT INTO evenement_gasttenant (evenement_id, tenant_id, eigenaar_tenant_id, status)
                VALUES (@evenement, @tenantB, @tenantA, 'geaccepteerd');
            """, owner);
        cmd.Parameters.AddWithValue("tenantA", TenantA);
        cmd.Parameters.AddWithValue("tenantB", TenantB);
        cmd.Parameters.AddWithValue("persoonA", PersoonA);
        cmd.Parameters.AddWithValue("persoonB", PersoonB);
        cmd.Parameters.AddWithValue("locatieA", LocatieA);
        cmd.Parameters.AddWithValue("typeA", EvenementtypeA);
        cmd.Parameters.AddWithValue("evenement", EvenementLandelijkeDag);
        await cmd.ExecuteNonQueryAsync();
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
            INSERT INTO persoon (id, tenant_id, voornaam, achternaam, geboortedatum, email)
            VALUES (gen_random_uuid(), @tenant, 'Mallory', 'Smokkel', '1990-01-01', 'm@x.nl')
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

    // Tabellen die bewust geen RLS hebben, met reden. Voeg hier nooit een
    // tenant-tabel aan toe om deze test te laten slagen — de hele waarde
    // van de test zit erin dat hij omvalt bij een vergeten policy.
    private static readonly string[] GeenTenantDataUitzonderingen =
    [
        "__EFMigrationsHistory", // EF Core's eigen migratietabel, geen tenant-data
    ];

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
              AND t.tablename <> ALL(@uitgezonderd)
              AND (c.relrowsecurity = false OR c.relforcerowsecurity = false)
            """,
            ("uitgezonderd", GeenTenantDataUitzonderingen));
        Assert.Equal(0, zonderRls);
    }
}
