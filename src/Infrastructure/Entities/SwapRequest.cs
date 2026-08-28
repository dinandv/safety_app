namespace BccSafety.Infrastructure.Entities;

public sealed class SwapRequest
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid? AssignmentId { get; set; }
    public Guid RequestedByPersonId { get; set; }
    public Guid? TargetPersonId { get; set; }
    public SwapRequestKind Kind { get; set; }
    public SwapRequestStatus Status { get; set; } = SwapRequestStatus.Open;
    public DateTimeOffset ExpiresAt { get; set; }

    public Shift Shift { get; set; } = null!;
    public Assignment? Assignment { get; set; }
    public Person RequestedByPerson { get; set; } = null!;
    public Person? TargetPerson { get; set; }
}
