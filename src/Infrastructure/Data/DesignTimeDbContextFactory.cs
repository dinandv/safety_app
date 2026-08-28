using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// Only for `dotnet ef migrations add`. No connection is ever made, so
/// this connection string doesn't need to point anywhere real.
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
