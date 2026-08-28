namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Evenement
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EvenementtypeId { get; set; }
    public Guid? KandidaatEvenementId { get; set; }
    public Guid LocatieId { get; set; }
    public required string Titel { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset Eind { get; set; }
    public EvenementStatus Status { get; set; } = EvenementStatus.concept;
    public EvenementBron Bron { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Evenementtype Evenementtype { get; set; } = null!;
    public KandidaatEvenement? KandidaatEvenement { get; set; }
    public Locatie Locatie { get; set; } = null!;
}
