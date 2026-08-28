namespace BccSafety.Infrastructure.Entities;

public sealed class Location
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? QrSlug { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
