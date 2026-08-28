namespace BccSafety.Infrastructure.Entities;

public sealed class ActionToken
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public ActionTokenPurpose Purpose { get; set; }

    /// <summary>SHA-256 of the raw token/code. The raw value is never stored.</summary>
    public required string TokenHash { get; set; }

    /// <summary>E.g. the assignment id a shift-action token is scoped to.</summary>
    public Guid? ScopeId { get; set; }

    public DateTimeOffset ValidUntil { get; set; }
    public DateTimeOffset? UsedAt { get; set; }

    public Person Person { get; set; } = null!;
}
