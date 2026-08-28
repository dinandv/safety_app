using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// Brings the schema up to date and then applies the RLS policies from
/// db/001_tenancy_rls.sql. Must run on a connection with owner rights
/// (bcc_owner), never with bcc_app — otherwise the policies would exist,
/// but the role creating them would itself have BYPASSRLS-like rights
/// through ownership.
/// </summary>
public static class TenancyMigrator
{
    public static async Task ApplyAsync(NpgsqlConnection ownerConnection, CancellationToken ct = default)
    {
        var options = new DbContextOptionsBuilder<BccSafetyDbContext>()
            .UseNpgsql(ownerConnection)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var context = new BccSafetyDbContext(options, new TenantContext()))
        {
            await context.Database.MigrateAsync(ct);
        }

        await using var cmd = ownerConnection.CreateCommand();
        cmd.CommandText = ReadRlsScript();
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static string ReadRlsScript()
    {
        var assembly = typeof(TenancyMigrator).Assembly;
        const string resourceName = "BccSafety.Infrastructure.Sql.001_tenancy_rls.sql";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. " +
                $"Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
