namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Richtlijn
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Titel { get; set; }

    /// <summary>Server-side gesaniteerd met een allow-list voordat dit veld gevuld wordt.</summary>
    public required string HtmlGesaniteerd { get; set; }

    public RichtlijnZichtbaarheid Zichtbaarheid { get; set; } = RichtlijnZichtbaarheid.algemeen;
    public RichtlijnSoort Soort { get; set; }
    public int Volgorde { get; set; }
    public int Versie { get; set; } = 1;
    public DateTimeOffset? GepubliceerdOp { get; set; }
    public Guid? BijgewerktDoor { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Persoon? BijgewerktDoorPersoon { get; set; }
}
