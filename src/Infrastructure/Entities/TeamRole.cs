namespace BccSafety.Infrastructure.Entities;

public sealed class TeamRole
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public TeamRoleKind Kind { get; set; }
    public string? VestColor { get; set; }
    public bool Active { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
