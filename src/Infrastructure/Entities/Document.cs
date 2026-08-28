namespace BccSafety.Infrastructure.Entities;

public sealed class Document
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public required string VersionLabel { get; set; }
    public required string FileRef { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
