using System.Security.Claims;
using BccSafety.Api.Contracts;
using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Endpoints;

/// <summary>
/// The information tab: the contact card and the generally visible
/// guideline cards. Both are cached by the service worker, because the
/// place where you need a phone number is usually the place with no
/// signal.
///
/// Restricted guidelines — anything about the physical security of a
/// location — are not returned here at all, and not cached. Not returned
/// with a lock on them either: a visible lock is an invitation.
/// </summary>
public static class InfoEndpoints
{
    public static void MapInfoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/info").RequireAuthorization();

        group.MapGet("/contacts", GetContactsAsync);
        group.MapGet("/guidelines", GetGuidelinesAsync);
    }

    private static async Task<IResult> GetContactsAsync(
        ClaimsPrincipal user, BccSafetyDbContext db, ITenantContext tenant, CancellationToken ct)
    {
        if (user.ResolveFor(tenant) is null) return Results.Forbid();

        var contacts = await db.Contacts
            .AsNoTracking()
            .OrderByDescending(c => c.IsEmergencyNumber)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new ContactCardEntry(c.Id, c.Name, c.Function, c.Phone, c.IsEmergencyNumber))
            .ToListAsync(ct);

        return Results.Ok(contacts);
    }

    private static async Task<IResult> GetGuidelinesAsync(
        ClaimsPrincipal user, BccSafetyDbContext db, ITenantContext tenant, CancellationToken ct)
    {
        if (user.ResolveFor(tenant) is null) return Results.Forbid();

        var guidelines = await db.Guidelines
            .AsNoTracking()
            .Where(g => g.Visibility == GuidelineVisibility.General
                && g.Kind == GuidelineKind.Card
                && g.PublishedAt != null)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Title)
            .Select(g => new GuidelineCard(g.Id, g.Title, g.SanitizedHtml, g.Version))
            .ToListAsync(ct);

        return Results.Ok(guidelines);
    }
}
