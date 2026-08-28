namespace BccSafety.Infrastructure.Entities;

public enum PersonStatus
{
    Active,
    Inactive,
}

public enum AppRole
{
    PlatformAdmin,
    TenantAdmin,
    Planner,
    Participant,
}

public enum TeamRoleKind
{
    ShiftRole,
    Skill,
}

public enum AvailabilityKind
{
    Unavailable,
    Preferred,
}

public enum ExceptionVerdict
{
    AlwaysDeploy,
    NeverDeploy,
}

public enum CandidateEventStatus
{
    New,
    Linked,
    Ignored,
    Changed,
    RemovedFromSource,
}

public enum EventStatus
{
    Draft,
    Scheduled,
    Cancelled,
}

public enum EventSource
{
    Calendar,
    Manual,
}

public enum CalendarMismatchKind
{
    SourceRemoved,
    TimeChanged,
    NoSourceLeft,
}

public enum GuestTenantStatus
{
    Invited,
    Accepted,
    Declined,
}

public enum AssignmentStatus
{
    Assigned,
    Withdrawn,
    CheckedIn,
    NoShow,
}

public enum SwapRequestKind
{
    Swap,
    OpenCall,
}

public enum SwapRequestStatus
{
    Open,
    Accepted,
    Rejected,
    Expired,
}

public enum CheckInMethod
{
    Qr,
    Self,
    Supervisor,
}

public enum GuidelineVisibility
{
    General,
    Restricted,
}

public enum GuidelineKind
{
    Card,
    Document,
}

public enum NotificationChannel
{
    Email,
    Chat,
    WebPush,
}

public enum ActionTokenPurpose
{
    Login,
    ShiftAction,
    ChatLink,
}
