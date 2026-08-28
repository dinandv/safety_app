namespace BccSafety.Infrastructure.Entities;

public sealed class Advisory
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public required string Text { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly ValidUntil { get; set; }
    public Guid? EventTypeId { get; set; }
    public int Priority { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public EventType? EventType { get; set; }
}
