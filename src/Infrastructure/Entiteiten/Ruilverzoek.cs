namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Ruilverzoek
{
    public Guid Id { get; set; }
    public Guid DienstId { get; set; }
    public Guid? ToewijzingId { get; set; }
    public Guid AangevraagdDoorPersoonId { get; set; }
    public Guid? DoelPersoonId { get; set; }
    public RuilverzoekSoort Soort { get; set; }
    public RuilverzoekStatus Status { get; set; } = RuilverzoekStatus.open;
    public DateTimeOffset VerlooptOp { get; set; }

    public Dienst Dienst { get; set; } = null!;
    public Toewijzing? Toewijzing { get; set; }
    public Persoon AangevraagdDoorPersoon { get; set; } = null!;
    public Persoon? DoelPersoon { get; set; }
}
