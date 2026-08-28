using BccSafety.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Infrastructure.Scheduling;

/// <summary>
/// The one hard exclusion in this application (docs/datamodel.md,
/// "Geschiktheidsberekening"): without the shift role, or without a valid
/// certificate for that role on the date of the shift, someone cannot be
/// scheduled in it. Everything else — age range, availability, skills —
/// sorts and warns but never blocks, which is why there is no override
/// mechanism anywhere.
///
/// Validity is judged on the date of the shift, never on today: a
/// certificate that expires between planning and the event is invalid for
/// that event.
/// </summary>
public static class Qualifications
{
    public static async Task<bool> IsQualifiedAsync(
        this BccSafetyDbContext db,
        Guid personId,
        Guid teamRoleId,
        DateOnly onDate,
        CancellationToken ct = default)
    {
        var hasRole = await db.PersonTeamRoles
            .AnyAsync(pt => pt.PersonId == personId && pt.TeamRoleId == teamRoleId, ct);
        if (!hasRole) return false;

        // Every certificate type that this role requires must be held and
        // still valid. A type with no expiry date never expires.
        var missing = await db.QualificationTypes
            .Where(qt => qt.RequiredForTeamRoleId == teamRoleId)
            .Where(qt => !db.Qualifications.Any(q =>
                q.PersonId == personId
                && q.QualificationTypeId == qt.Id
                && q.ObtainedOn <= onDate
                && (q.ValidUntil == null || q.ValidUntil >= onDate)))
            .AnyAsync(ct);

        return !missing;
    }
}
