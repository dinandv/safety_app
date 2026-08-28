namespace BccSafety.Api.Contracts;

/// <summary>
/// What the participant PWA reads. Deliberately flat and screen-shaped:
/// the day overview is opened on a phone, often on a bad connection, and
/// the client should not have to stitch four calls together to draw one
/// screen.
///
/// No Dutch strings cross this boundary. The API returns facts (a reason
/// kind, a count, a colour); the interface turns them into the sentence
/// the volunteer reads.
/// </summary>
public sealed record TodayResponse(
    DateOnly Date,
    DateTimeOffset GeneratedAt,
    TodayEvent? Event,
    UpcomingEvent? NextEvent);

public sealed record TodayEvent(
    Guid Id,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    int FilledCount,
    int RequiredCount,
    PhoneVisibilityState PhoneNumbers,
    IReadOnlyList<AdvisoryNote> Advisories,
    OwnShift? OwnShift,
    IReadOnlyList<RoleGroup> RoleGroups);

public sealed record UpcomingEvent(
    Guid Id,
    string Title,
    DateTimeOffset Start,
    DateTimeOffset End);

/// <summary>
/// Why phone numbers are, or are not, on the screen. The interface needs
/// the reason and not just the verdict: "you are not on this duty" and
/// "the duty has not started yet" are different sentences, and telling
/// someone the wrong one makes the rule look arbitrary.
/// </summary>
public enum PhoneVisibilityState
{
    Visible,

    /// <summary>The caller has no shift on today's event.</summary>
    NotScheduled,

    /// <summary>The caller is scheduled, but the event is not close enough yet.</summary>
    OutsideShiftWindow,
}

public sealed record AdvisoryNote(Guid Id, string Title, string Text);

/// <summary>The caller's own shift on today's event, if they have one.</summary>
public sealed record OwnShift(
    Guid ShiftId,
    Guid AssignmentId,
    string TeamRoleName,
    string? VestColor,
    DateTimeOffset Start,
    DateTimeOffset End,
    string PersonName,
    string? Note);

public sealed record RoleGroup(
    Guid ShiftId,
    string TeamRoleName,
    string? VestColor,
    DateTimeOffset Start,
    DateTimeOffset End,
    int RequiredCount,
    IReadOnlyList<TeamMember> People,
    OpenSpots? OpenSpots);

/// <summary>
/// A colleague on the same event today. <see cref="Phone"/> is null
/// unless the caller is scheduled themselves and the event is close
/// enough — see <c>PhoneVisibility</c>. Never a date of birth, never
/// anyone who is not on this event.
/// </summary>
public sealed record TeamMember(
    Guid PersonId,
    string Name,
    string Initials,
    string? Phone,
    bool IsSelf);

/// <summary>
/// An unfilled spot is a row of its own, not a missing row: the screen
/// exists to make gaps impossible to miss.
/// </summary>
public sealed record OpenSpots(
    int Count,
    OpenSpotReason Reason,
    string? WithdrawnByFirstName,
    Guid? OpenCallId);

public enum OpenSpotReason
{
    /// <summary>Never filled while planning.</summary>
    NeverFilled,

    /// <summary>Someone withdrew, which opened a call automatically.</summary>
    Withdrawn,
}

public sealed record MyShift(
    Guid ShiftId,
    Guid AssignmentId,
    Guid EventId,
    string EventTitle,
    string TeamRoleName,
    string? VestColor,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    int RequiredCount,
    int FilledCount);

public sealed record OpenCall(
    Guid Id,
    Guid ShiftId,
    Guid EventId,
    string EventTitle,
    string TeamRoleName,
    string? VestColor,
    DateTimeOffset Start,
    DateTimeOffset End,
    string LocationName,
    OpenSpotReason Reason,
    string? WithdrawnByFirstName,
    bool AlreadyOnThisShift);

public sealed record ContactCardEntry(
    Guid Id,
    string Name,
    string? Function,
    string Phone,
    bool IsEmergencyNumber);

public sealed record GuidelineCard(
    Guid Id,
    string Title,
    string SanitizedHtml,
    int Version);

public sealed record CurrentUserResponse(
    Guid PersonId,
    string FirstName,
    string LastName,
    string DisplayName,
    IReadOnlyList<string> Roles);
