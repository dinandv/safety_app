namespace BccSafety.Infrastructure.Entities;

public sealed class EventGuestTenant
{
    public Guid EventId { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>
    /// Denormalized copy of Event.TenantId, set when the invite is created.
    /// Needed so the RLS policy on this table doesn't have to look at Event
    /// — that would create a cycle with the Event read policy, which in
    /// turn consults this table.
    /// </summary>
    public Guid OwnerTenantId { get; set; }

    public GuestTenantStatus Status { get; set; } = GuestTenantStatus.Invited;

    public Event Event { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
