namespace BccSafety.Infrastructure.Entities;

public sealed class Assignment
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid PersonId { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    public Guid? AssignedBy { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? WithdrawnAt { get; set; }
    public string? WithdrawalReason { get; set; }

    /// <summary>Jsonb: soft signals the planner knowingly assigned against. Informational only.</summary>
    public string? WarningsAtAssignment { get; set; }

    public Shift Shift { get; set; } = null!;
    public Person Person { get; set; } = null!;
    public Person? AssignedByPerson { get; set; }
}
