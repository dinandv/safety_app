using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// Alleen voor `dotnet ef migrations add`. Er wordt geen verbinding
/// gemaakt, dus deze connectiestring hoeft nergens echt te bestaan.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BccSafetyDbContext>
{
    public BccSafetyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BccSafetyDbContext>()
            .UseNpgsql("Host=localhost;Database=bccsafety;Username=bcc_owner")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new BccSafetyDbContext(options, new TenantContext());
    }
}
