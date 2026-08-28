namespace BccSafety.Infrastructure.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ActorPersonId { get; set; }
    public required string Entity { get; set; }
    public Guid EntityId { get; set; }
    public required string Action { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Person? ActorPerson { get; set; }
}
