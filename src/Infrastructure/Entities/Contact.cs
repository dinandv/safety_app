namespace BccSafety.Infrastructure.Entities;

public sealed class Contact
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public string? Function { get; set; }
    public required string Phone { get; set; }
    public bool IsEmergencyNumber { get; set; }
    public int SortOrder { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
