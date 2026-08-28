namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Dienstsjabloon
{
    public Guid Id { get; set; }
    public Guid EvenementtypeId { get; set; }
    public Guid TeamrolId { get; set; }
    public int Aantal { get; set; }
    public int StartOffsetMinuten { get; set; }
    public int DuurMinuten { get; set; }
    public int? InzetbaarLeeftijdVan { get; set; }
    public int? InzetbaarLeeftijdTot { get; set; }

    public Evenementtype Evenementtype { get; set; } = null!;
    public Teamrol Teamrol { get; set; } = null!;
}
