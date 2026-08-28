namespace BccSafety.Infrastructure.Entities;

public sealed class Qualification
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid QualificationTypeId { get; set; }
    public DateOnly ObtainedOn { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public string? Note { get; set; }

    public Person Person { get; set; } = null!;
    public QualificationType QualificationType { get; set; } = null!;
}
