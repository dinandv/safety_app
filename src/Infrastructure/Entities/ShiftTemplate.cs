namespace BccSafety.Infrastructure.Entities;

public sealed class ShiftTemplate
{
    public Guid Id { get; set; }
    public Guid EventTypeId { get; set; }
    public Guid TeamRoleId { get; set; }
    public int Count { get; set; }
    public int StartOffsetMinutes { get; set; }
    public int DurationMinutes { get; set; }
    public int? DeployableAgeFrom { get; set; }
    public int? DeployableAgeTo { get; set; }

    public EventType EventType { get; set; } = null!;
    public TeamRole TeamRole { get; set; } = null!;
}
