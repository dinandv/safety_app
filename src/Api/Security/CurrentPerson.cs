using System.Security.Claims;
using BccSafety.Infrastructure.Tenancy;

namespace BccSafety.Api.Security;

/// <summary>
/// The person behind the session cookie, checked against the tenant the
/// hostname resolved to.
///
/// Cookies are host-only, so a session from one subdomain does not travel
/// to another by itself. This check is the belt to that braces: if it
/// ever does, the request is refused rather than silently served with the
/// wrong tenant's row-level security context.
/// </summary>
public static class CurrentPerson
{
    public static Guid? PersonId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static Guid? TenantId(this ClaimsPrincipal user)
        => Guid.TryParse(user.FindFirstValue("tenant_id"), out var id) ? id : null;

    /// <summary>
    /// Returns the caller's person id, or null when there is no usable
    /// session for this tenant. Endpoints turn that into a 403.
    /// </summary>
    public static Guid? ResolveFor(this ClaimsPrincipal user, ITenantContext tenant)
    {
        var personId = user.PersonId();
        if (personId is null) return null;
        return user.TenantId() == tenant.TenantId ? personId : null;
    }
}
