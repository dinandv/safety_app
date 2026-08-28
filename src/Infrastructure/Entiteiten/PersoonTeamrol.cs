namespace BccSafety.Infrastructure.Entiteiten;

public sealed class PersoonTeamrol
{
    public Guid PersoonId { get; set; }
    public Guid TeamrolId { get; set; }
    public DateTimeOffset? BevestigdOp { get; set; }
    public bool BevestigdDoorPersoonZelf { get; set; }

    public Persoon Persoon { get; set; } = null!;
    public Teamrol Teamrol { get; set; } = null!;
}
