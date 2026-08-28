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
/// The day overview: who is on duty today, per team role.
///
/// The loudest complaint from practice is that nobody knows who is on
/// today, and the person asking usually has no shift themselves — they
/// are looking for a first-aider, or for whoever is lead responder
/// tonight. So this endpoint answers for every participant of the tenant,
/// not only for those scheduled. What differs is the phone numbers; see
/// <see cref="PhoneVisibility"/>.
/// </summary>
public static class TodayEndpoints
{
    public static void MapTodayEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/today", GetAsync).RequireAuthorization();
    }

    private static async Task<IResult> GetAsync(
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
        var localNow = TimeZoneInfo.ConvertTime(now, timeZone);
        var today = DateOnly.FromDateTime(localNow.DateTime);
        var dayStart = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), timeZone.GetUtcOffset(localNow));
        var dayEnd = dayStart.AddDays(1);

        // An event that runs over midnight still counts as today's, hence
        // the overlap test rather than a comparison on the start date.
        var todaysEvent = await db.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Scheduled && e.Start < dayEnd && e.End > dayStart)
            .OrderBy(e => e.End < now)
            .ThenBy(e => e.Start)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Start,
                e.End,
                e.EventTypeId,
                LocationName = e.Location.Name,
            })
            .FirstOrDefaultAsync(ct);

        if (todaysEvent is null)
        {
            var next = await db.Events
                .AsNoTracking()
                .Where(e => e.Status == EventStatus.Scheduled && e.Start >= dayEnd)
                .OrderBy(e => e.Start)
                .Select(e => new UpcomingEvent(e.Id, e.Title, e.Start, e.End))
                .FirstOrDefaultAsync(ct);

            return Results.Ok(new TodayResponse(today, now, null, next));
        }

        var shifts = await db.Shifts
            .AsNoTracking()
            .Where(s => s.EventId == todaysEvent.Id)
            .OrderBy(s => s.Start)
            .ThenBy(s => s.TeamRole.Name)
            .Select(s => new
            {
                s.Id,
                s.Start,
                s.End,
                s.RequiredCount,
                s.Note,
                TeamRoleName = s.TeamRole.Name,
                s.TeamRole.VestColor,
            })
            .ToListAsync(ct);

        var shiftIds = shifts.Select(s => s.Id).ToList();

        var assignments = await db.Assignments
            .AsNoTracking()
            .Where(a => shiftIds.Contains(a.ShiftId)
                && (a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.CheckedIn))
            .OrderBy(a => a.Person.FirstName)
            .Select(a => new
            {
                a.Id,
                a.ShiftId,
                a.PersonId,
                a.Person.FirstName,
                a.Person.LastNamePrefix,
                a.Person.LastName,
                a.Person.Phone,
            })
            .ToListAsync(ct);

        // An unfilled spot needs a reason, and the reason is worth more
        // than the gap itself: "someone withdrew this morning" is a
        // different problem from "never filled while planning".
        var openCalls = await db.SwapRequests
            .AsNoTracking()
            .Where(r => shiftIds.Contains(r.ShiftId)
                && r.Kind == SwapRequestKind.OpenCall
                && r.Status == SwapRequestStatus.Open)
            .Select(r => new
            {
                r.Id,
                r.ShiftId,
                WithdrawnByFirstName = r.Assignment != null ? r.Assignment.Person.FirstName : null,
            })
            .ToListAsync(ct);

        var advisories = await db.Advisories
            .AsNoTracking()
            .Where(a => a.ValidFrom <= today && a.ValidUntil >= today
                && (a.EventTypeId == null || a.EventTypeId == todaysEvent.EventTypeId))
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.Title)
            .Select(a => new AdvisoryNote(a.Id, a.Title, a.Text))
            .ToListAsync(ct);

        var own = assignments.FirstOrDefault(a => a.PersonId == personId);
        var phones = PhoneVisibility.Evaluate(
            own is not null, todaysEvent.Start, todaysEvent.End, now);
        var showPhones = phones == PhoneVisibilityState.Visible;

        var groups = shifts.Select(shift =>
        {
            var people = assignments
                .Where(a => a.ShiftId == shift.Id)
                .Select(a => new TeamMember(
                    a.PersonId,
                    PersonName.Display(a.FirstName, a.LastNamePrefix, a.LastName),
                    PersonName.Initials(a.FirstName, a.LastName),
                    showPhones ? a.Phone : null,
                    a.PersonId == personId))
                .ToList();

            var missing = shift.RequiredCount - people.Count;
            OpenSpots? openSpots = null;
            if (missing > 0)
            {
                var call = openCalls.FirstOrDefault(c => c.ShiftId == shift.Id);
                openSpots = new OpenSpots(
                    missing,
                    call?.WithdrawnByFirstName is not null ? OpenSpotReason.Withdrawn : OpenSpotReason.NeverFilled,
                    call?.WithdrawnByFirstName,
                    call?.Id);
            }

            return new RoleGroup(
                shift.Id,
                shift.TeamRoleName,
                shift.VestColor,
                shift.Start,
                shift.End,
                shift.RequiredCount,
                people,
                openSpots);
        }).ToList();

        OwnShift? ownShift = null;
        if (own is not null)
        {
            var shift = shifts.First(s => s.Id == own.ShiftId);
            ownShift = new OwnShift(
                shift.Id,
                own.Id,
                shift.TeamRoleName,
                shift.VestColor,
                shift.Start,
                shift.End,
                PersonName.Display(own.FirstName, own.LastNamePrefix, own.LastName),
                shift.Note);
        }

        var response = new TodayResponse(
            today,
            now,
            new TodayEvent(
                todaysEvent.Id,
                todaysEvent.Title,
                todaysEvent.Start,
                todaysEvent.End,
                todaysEvent.LocationName,
                groups.Sum(g => g.People.Count),
                groups.Sum(g => g.RequiredCount),
                phones,
                advisories,
                ownShift,
                groups),
            null);

        return Results.Ok(response);
    }
}
