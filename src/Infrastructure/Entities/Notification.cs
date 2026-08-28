namespace BccSafety.Infrastructure.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersonId { get; set; }
    public NotificationChannel Channel { get; set; }
    public required string Template { get; set; }
    public Guid ContextId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public required string Status { get; set; }

    /// <summary>Prevents duplicate sends after a scheduler restart.</summary>
    public required string IdempotencyKey { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Person Person { get; set; } = null!;
}
