using System.Security.Claims;
using BccSafety.Api.Contracts;
using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using BccSafety.Infrastructure.Scheduling;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Endpoints;

/// <summary>
/// The open call: a spot goes to the pool and whoever claims it first
/// gets it. Both the list and the claim live here.
///
/// Claiming works on a shift rather than on a swap request, because the
/// day overview offers the same button for two different gaps: a spot
/// that was never filled while planning, where no swap request exists,
/// and a spot that opened when someone withdrew, where one does. To the
/// volunteer looking at the screen those are the same gap, so they are
/// the same action.
///
/// Handing a shift to one chosen colleague — the swap — is a different
/// flow with a candidate list and lives in its own issue.
/// </summary>
public static class OpenCallEndpoints
{
    public static void MapOpenCallEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapGet("/open-calls", ListAsync);
        group.MapPost("/shifts/{shiftId:guid}/claim", ClaimAsync);
    }

    private static async Task<IResult> ListAsync(
        ClaimsPrincipal user,
        BccSafetyDbContext db,
        ITenantContext tenant,
        TimeProvider time,
        TimeZoneInfo timeZone,
        CancellationToken ct)
    {
        var personId = user.ResolveFor(tenant);
        if (personId is null) return Results.Forbid();

        var now = time.GetUtcNow();

        // A shift is short of people either because a swap request is
        // open on it, or because it was simply never filled. Both belong
        // in this list; a volunteer does not care which it is.
        var candidates = await db.Shifts
            .AsNoTracking()
            .Where(s => s.Start > now
                && s.Event.Status == EventStatus.Scheduled
                && db.Assignments.Count(a => a.ShiftId == s.Id
                    && a.Status != AssignmentStatus.Withdrawn) < s.RequiredCount
                && db.PersonTeamRoles.Any(pt => pt.PersonId == personId && pt.TeamRoleId == s.TeamRoleId))
            .OrderBy(s => s.Start)
            .Select(s => new
            {
                s.Id,
                s.TeamRoleId,
                s.Start,
                s.End,
                s.EventId,
                EventTitle = s.Event.Title,
                TeamRoleName = s.TeamRole.Name,
                s.TeamRole.VestColor,
                LocationName = s.Event.Location.Name,
                AlreadyMine = db.Assignments.Any(a => a.ShiftId == s.Id
                    && a.PersonId == personId && a.Status != AssignmentStatus.Withdrawn),
                WithdrawnByFirstName = db.SwapRequests
                    .Where(r => r.ShiftId == s.Id
                        && r.Kind == SwapRequestKind.OpenCall
                        && r.Status == SwapRequestStatus.Open
                        && r.Assignment != null)
                    .Select(r => r.Assignment!.Person.FirstName)
                    .FirstOrDefault(),
                CallId = db.SwapRequests
                    .Where(r => r.ShiftId == s.Id
                        && r.Kind == SwapRequestKind.OpenCall
                        && r.Status == SwapRequestStatus.Open)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        // The certificate check is the one hard exclusion, and it depends
        // on the date of each shift. The list is short, so checking it
        // per row keeps a single authoritative implementation instead of
        // a second copy of the rule written in SQL.
        var result = new List<OpenCall>();
        foreach (var candidate in candidates)
        {
            var shiftDate = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(candidate.Start, timeZone).DateTime);
            if (!await db.IsQualifiedAsync(personId.Value, candidate.TeamRoleId, shiftDate, ct))
                continue;

            result.Add(new OpenCall(
                candidate.CallId ?? candidate.Id,
                candidate.Id,
                candidate.EventId,
                candidate.EventTitle,
                candidate.TeamRoleName,
                candidate.VestColor,
                candidate.Start,
                candidate.End,
                candidate.LocationName,
                candidate.WithdrawnByFirstName is not null
                    ? OpenSpotReason.Withdrawn
                    : OpenSpotReason.NeverFilled,
                candidate.WithdrawnByFirstName,
                candidate.AlreadyMine));
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> ClaimAsync(
        Guid shiftId,
        ClaimsPrincipal user,
        BccSafetyDbContext db,
        ITenantContext tenant,
        TimeProvider time,
        TimeZoneInfo timeZone,
        CancellationToken ct)
    {
        var personId = user.ResolveFor(tenant);
        if (personId is null) return Results.Forbid();

        var now = time.GetUtcNow();

        // "Whoever responds first gets it" only holds if two people
        // claiming at the same moment cannot both win. The row lock on
        // the shift is what makes that true; without it both transactions
        // read the same count and both insert.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var locked = await db.Shifts
            .FromSql($"SELECT * FROM shift WHERE id = {shiftId} FOR UPDATE")
            .FirstOrDefaultAsync(ct);
        if (locked is null) return Results.NotFound();

        var shift = await db.Shifts
            .Where(s => s.Id == shiftId)
            .Select(s => new { s.TeamRoleId, s.Start, s.RequiredCount, EventStatus = s.Event.Status })
            .FirstAsync(ct);

        if (shift.EventStatus != EventStatus.Scheduled)
            return Results.Conflict(new { reason = "event_not_scheduled" });

        if (shift.Start <= now)
            return Results.Conflict(new { reason = "shift_started" });

        var alreadyMine = await db.Assignments.AnyAsync(
            a => a.ShiftId == shiftId && a.PersonId == personId
                && a.Status != AssignmentStatus.Withdrawn, ct);
        if (alreadyMine) return Results.Conflict(new { reason = "already_assigned" });

        var filled = await db.Assignments.CountAsync(
            a => a.ShiftId == shiftId && a.Status != AssignmentStatus.Withdrawn, ct);
        if (filled >= shift.RequiredCount) return Results.Conflict(new { reason = "already_taken" });

        var shiftDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(shift.Start, timeZone).DateTime);
        if (!await db.IsQualifiedAsync(personId.Value, shift.TeamRoleId, shiftDate, ct))
            return Results.Conflict(new { reason = "not_qualified" });

        var assignment = new Assignment
        {
            ShiftId = shiftId,
            PersonId = personId.Value,
            Status = AssignmentStatus.Assigned,
            AssignedBy = personId,
            AssignedAt = now,
        };
        db.Assignments.Add(assignment);

        // Closing the swap request keeps the open-call list honest. A
        // shift can be short more than one person, so it only closes once
        // the last spot is gone.
        if (filled + 1 >= shift.RequiredCount)
        {
            var call = await db.SwapRequests.FirstOrDefaultAsync(
                r => r.ShiftId == shiftId
                    && r.Kind == SwapRequestKind.OpenCall
                    && r.Status == SwapRequestStatus.Open, ct);
            if (call is not null) call.Status = SwapRequestStatus.Accepted;
        }

        await db.SaveChangesAsync(ct);

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenant.TenantId!.Value,
            ActorPersonId = personId,
            Entity = "assignment",
            EntityId = assignment.Id,
            Action = "claimed",
            Timestamp = now,
        });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Results.Ok(new { assignment.Id });
    }
}
