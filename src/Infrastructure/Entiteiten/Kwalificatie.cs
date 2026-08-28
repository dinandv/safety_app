namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Kwalificatie
{
    public Guid Id { get; set; }
    public Guid PersoonId { get; set; }
    public Guid KwalificatieTypeId { get; set; }
    public DateOnly BehaaldOp { get; set; }
    public DateOnly? GeldigTot { get; set; }
    public string? Notitie { get; set; }

    public Persoon Persoon { get; set; } = null!;
    public KwalificatieType KwalificatieType { get; set; } = null!;
}
