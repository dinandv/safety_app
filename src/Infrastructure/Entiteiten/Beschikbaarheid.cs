namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Beschikbaarheid
{
    public Guid Id { get; set; }
    public Guid PersoonId { get; set; }
    public DateTimeOffset Van { get; set; }
    public DateTimeOffset Tot { get; set; }
    public BeschikbaarheidSoort Soort { get; set; }
    public string? Notitie { get; set; }

    public Persoon Persoon { get; set; } = null!;
}
