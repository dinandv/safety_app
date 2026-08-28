namespace BccSafety.Infrastructure.Entities;

public sealed class Shift
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TeamRoleId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public int RequiredCount { get; set; }
    public string? Note { get; set; }

    public Event Event { get; set; } = null!;
    public TeamRole TeamRole { get; set; } = null!;
}
