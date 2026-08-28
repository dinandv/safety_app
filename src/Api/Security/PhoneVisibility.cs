using BccSafety.Api.Contracts;

namespace BccSafety.Api.Security;

/// <summary>
/// Who gets to see a colleague's phone number on the day overview.
///
/// The rule, from docs/ontwerp.md: a participant sees name, team role and
/// phone number of the people working the same duty — nothing wider.
/// "The same duty" is the event, not the single shift: the whole point of
/// the screen is that a steward can reach today's first-aider, and those
/// are different shifts of the same event.
///
/// Two conditions, both required. You are scheduled on the event
/// yourself, and the event is close enough in time to be the one you are
/// actually working. Someone who opens the app a week ahead sees who is
/// coming, not everyone's number.
/// </summary>
public static class PhoneVisibility
{
    /// <summary>Numbers appear this long before the event starts.</summary>
    public static readonly TimeSpan Lead = TimeSpan.FromHours(4);

    /// <summary>And stay this long after it ends, for the wrap-up.</summary>
    public static readonly TimeSpan Grace = TimeSpan.FromHours(2);

    public static PhoneVisibilityState Evaluate(
        bool callerIsScheduled,
        DateTimeOffset eventStart,
        DateTimeOffset eventEnd,
        DateTimeOffset now)
    {
        if (!callerIsScheduled) return PhoneVisibilityState.NotScheduled;
        var withinWindow = now >= eventStart - Lead && now <= eventEnd + Grace;
        return withinWindow ? PhoneVisibilityState.Visible : PhoneVisibilityState.OutsideShiftWindow;
    }
}
