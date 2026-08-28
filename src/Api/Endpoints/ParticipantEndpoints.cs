using System.Security.Claims;
using BccSafety.Api.Contracts;
using BccSafety.Api.Formatting;
using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Endpoints;

/// <summary>
/// What a participant reads about themselves: who they are logged in as,
/// and the shifts they are down for.
/// </summary>
public static class ParticipantEndpoints
{
    public static void MapParticipantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapGet("/me", GetMeAsync);
        group.MapGet("/my/shifts", GetMyShiftsAsync);
    }

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal user, BccSafetyDbContext db, ITenantContext tenant, CancellationToken ct)
    {
        var personId = user.ResolveFor(tenant);
        if (personId is null) return Results.Forbid();

        var person = await db.People
            .AsNoTracking()
            .Where(p => p.Id == personId && p.Status == PersonStatus.Active)
            .Select(p => new { p.Id, p.FirstName, p.LastNamePrefix, p.LastName })
            .FirstOrDefaultAsync(ct);
        if (person is null) return Results.Forbid();

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Results.Ok(new CurrentUserResponse(
            person.Id,
            person.FirstName,
            person.LastName,
            PersonName.Display(person.FirstName, person.LastNamePrefix, person.LastName),
            roles));
    }

    private static async Task<IResult> GetMyShiftsAsync(
        ClaimsPrincipal user,
        BccSafetyDbContext db,
        ITenantContext tenant,
        TimeProvider time,
        CancellationToken ct)
    {
        var personId = user.ResolveFor(tenant);
        if (personId is null) return Results.Forbid();

        var now = time.GetUtcNow();

        var shifts = await db.Assignments
            .AsNoTracking()
            .Where(a => a.PersonId == personId
                && a.Status != AssignmentStatus.Withdrawn
                && a.Shift.End >= now
                && a.Shift.Event.Status == EventStatus.Scheduled)
            .OrderBy(a => a.Shift.Start)
            .Select(a => new MyShift(
                a.ShiftId,
                a.Id,
                a.Shift.EventId,
                a.Shift.Event.Title,
                a.Shift.TeamRole.Name,
                a.Shift.TeamRole.VestColor,
                a.Shift.Start,
                a.Shift.End,
                a.Shift.Event.Location.Name,
                a.Shift.RequiredCount,
                db.Assignments.Count(other => other.ShiftId == a.ShiftId
                    && other.Status != AssignmentStatus.Withdrawn)))
            .ToListAsync(ct);

        return Results.Ok(shifts);
    }
}
