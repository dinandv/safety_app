namespace BccSafety.Infrastructure.Entiteiten;

public sealed class PersoonEvenementtypeUitzondering
{
    public Guid PersoonId { get; set; }
    public Guid EvenementtypeId { get; set; }
    public UitzonderingOordeel Oordeel { get; set; }
    public required string Reden { get; set; }
    public Guid VastgelegdDoorPersoonId { get; set; }
    public DateTimeOffset VastgelegdOp { get; set; }

    public Persoon Persoon { get; set; } = null!;
    public Evenementtype Evenementtype { get; set; } = null!;
    public Persoon VastgelegdDoorPersoon { get; set; } = null!;
}
