namespace BccSafety.Infrastructure.Entities;

public sealed class CalendarSource
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string IcsUrl { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public bool Active { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
