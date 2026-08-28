namespace BccSafety.Infrastructure.Entiteiten;

public sealed class KandidaatEvenement
{
    public Guid Id { get; set; }
    public Guid AgendaBronId { get; set; }
    public required string IcsUid { get; set; }
    public string? RecurrenceId { get; set; }
    public required string Titel { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset Eind { get; set; }
    public string? LocatieTekst { get; set; }
    public required string InhoudHash { get; set; }
    public KandidaatEvenementStatus Status { get; set; } = KandidaatEvenementStatus.nieuw;

    public AgendaBron AgendaBron { get; set; } = null!;
}
