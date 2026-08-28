namespace BccSafety.Infrastructure.Entities;

public sealed class CalendarMismatch
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public CalendarMismatchKind Kind { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    public Event Event { get; set; } = null!;
}
