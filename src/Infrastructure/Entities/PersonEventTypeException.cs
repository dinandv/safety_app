namespace BccSafety.Infrastructure.Entities;

public sealed class PersonEventTypeException
{
    public Guid PersonId { get; set; }
    public Guid EventTypeId { get; set; }
    public ExceptionVerdict Verdict { get; set; }
    public required string Reason { get; set; }
    public Guid RecordedByPersonId { get; set; }
    public DateTimeOffset RecordedAt { get; set; }

    public Person Person { get; set; } = null!;
    public EventType EventType { get; set; } = null!;
    public Person RecordedByPerson { get; set; } = null!;
}
