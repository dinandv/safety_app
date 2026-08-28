namespace BccSafety.Infrastructure.Entities;

public sealed class Person
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string FirstName { get; set; }
    public string? LastNamePrefix { get; set; }
    public required string LastName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public string? ChatId { get; set; }
    public PersonStatus Status { get; set; } = PersonStatus.Active;
    public DateOnly? StoppedOn { get; set; }
    public DateTimeOffset? PseudonymizedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
