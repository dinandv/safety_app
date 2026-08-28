namespace BccSafety.Infrastructure.Entities;

public sealed class CandidateEvent
{
    public Guid Id { get; set; }
    public Guid CalendarSourceId { get; set; }
    public required string IcsUid { get; set; }
    public string? RecurrenceId { get; set; }
    public required string Title { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string? LocationText { get; set; }
    public required string ContentHash { get; set; }
    public CandidateEventStatus Status { get; set; } = CandidateEventStatus.New;

    public CalendarSource CalendarSource { get; set; } = null!;
}
