namespace BccSafety.Infrastructure.Entities;

public sealed class CheckIn
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public CheckInMethod Method { get; set; }
    public Guid ByPersonId { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public Assignment Assignment { get; set; } = null!;
    public Person ByPerson { get; set; } = null!;
}
