namespace BccSafety.Infrastructure.Entities;

public sealed class Availability
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset Until { get; set; }
    public AvailabilityKind Kind { get; set; }
    public string? Note { get; set; }

    public Person Person { get; set; } = null!;
}
