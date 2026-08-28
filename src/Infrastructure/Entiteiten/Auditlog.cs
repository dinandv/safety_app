namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Auditlog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ActorPersoonId { get; set; }
    public required string Entiteit { get; set; }
    public Guid EntiteitId { get; set; }
    public required string Actie { get; set; }
    public string? OudeWaarde { get; set; }
    public string? NieuweWaarde { get; set; }
    public DateTimeOffset Tijdstip { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Persoon? ActorPersoon { get; set; }
}
