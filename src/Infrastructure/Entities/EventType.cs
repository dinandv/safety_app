namespace BccSafety.Infrastructure.Entities;

public sealed class EventType
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public string? TargetAudienceDescription { get; set; }
    public int? TargetAgeFrom { get; set; }
    public int? TargetAgeTo { get; set; }
    public int? DeployableAgeFrom { get; set; }
    public int? DeployableAgeTo { get; set; }
    public Guid? RequiredSkillId { get; set; }
    public int? ExpectedVisitorCount { get; set; }
    public bool Active { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public TeamRole? RequiredSkill { get; set; }
}
