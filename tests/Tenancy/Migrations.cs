using BccSafety.Infrastructure.Data;
using Npgsql;

namespace BccSafety.Tests.Tenancy;

/// <summary>
/// Thin link between the isolation test and TenancyMigrator: migrate the
/// schema and apply db/001_tenancy_rls.sql, on a connection with owner
/// rights. bcc_app's password comes from the secret store and so isn't in
/// the SQL script; here, in test code against a throwaway container, we
/// set it explicitly so the test can log in with it.
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
