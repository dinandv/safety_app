using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// Brengt het schema naar de huidige staat en zet daarna de RLS-policies
/// uit db/001_tenancy_rls.sql erop. Moet draaien op een verbinding met
/// eigenaarsrechten (bcc_owner), nooit met bcc_app — anders staan de
/// policies er straks wel maar heeft de rol die ze aanmaakt zelf
/// BYPASSRLS-achtige rechten via het eigenaarschap.
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
                $"Embedded resource '{resourceName}' niet gevonden. " +
                $"Beschikbaar: {string.Join(", ", assembly.GetManifestResourceNames())}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
