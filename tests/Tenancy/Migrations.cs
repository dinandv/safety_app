using BccSafety.Infrastructure.Data;
using Npgsql;

namespace BccSafety.Tests.Tenancy;

/// <summary>
/// Dunne verbinding tussen de isolatietest en TenancyMigrator: schema
/// migreren plus db/001_tenancy_rls.sql toepassen, op een verbinding met
/// eigenaarsrechten. Het wachtwoord van bcc_app komt uit de secret store en
/// staat dus niet in het SQL-script; hier, in testcode tegen een
/// wegwerpcontainer, zetten we het expliciet zodat de test ermee kan inloggen.
/// </summary>
internal static class Migrations
{
    public static async Task ApplyAsync(NpgsqlConnection ownerConnection)
    {
        await TenancyMigrator.ApplyAsync(ownerConnection);

        await using var cmd = new NpgsqlCommand(
            "ALTER ROLE bcc_app WITH PASSWORD 'test'", ownerConnection);
        await cmd.ExecuteNonQueryAsync();
    }
}
