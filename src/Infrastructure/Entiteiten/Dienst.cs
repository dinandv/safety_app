namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Dienst
{
    public Guid Id { get; set; }
    public Guid EvenementId { get; set; }
    public Guid TeamrolId { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset Eind { get; set; }
    public int BenodigdAantal { get; set; }
    public string? Notitie { get; set; }

    public Evenement Evenement { get; set; } = null!;
    public Teamrol Teamrol { get; set; } = null!;
}
