namespace BccSafety.Infrastructure.Entities;

public sealed class QualificationType
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public Guid? RequiredForTeamRoleId { get; set; }
    public int? DefaultValidityMonths { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TeamRole? RequiredForTeamRole { get; set; }
}
