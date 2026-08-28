namespace BccSafety.Infrastructure.Entities;

public sealed class Event
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EventTypeId { get; set; }
    public Guid? CandidateEventId { get; set; }
    public Guid LocationId { get; set; }
    public required string Title { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public EventSource Source { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public EventType EventType { get; set; } = null!;
    public CandidateEvent? CandidateEvent { get; set; }
    public Location Location { get; set; } = null!;
}
