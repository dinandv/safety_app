namespace BccSafety.Infrastructure.Entities;

public sealed class Guideline
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Title { get; set; }

    /// <summary>Sanitized server-side with an allow-list before this field is populated.</summary>
    public required string SanitizedHtml { get; set; }

    public GuidelineVisibility Visibility { get; set; } = GuidelineVisibility.General;
    public GuidelineKind Kind { get; set; }
    public int SortOrder { get; set; }
    public int Version { get; set; } = 1;
    public DateTimeOffset? PublishedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Person? UpdatedByPerson { get; set; }
}
