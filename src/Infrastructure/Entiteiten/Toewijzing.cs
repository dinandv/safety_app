namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Toewijzing
{
    public Guid Id { get; set; }
    public Guid DienstId { get; set; }
    public Guid PersoonId { get; set; }
    public ToewijzingStatus Status { get; set; } = ToewijzingStatus.ingedeeld;
    public Guid? ToegewezenDoor { get; set; }
    public DateTimeOffset ToegewezenOp { get; set; }
    public DateTimeOffset? AfgemeldOp { get; set; }
    public string? AfmeldReden { get; set; }

    /// <summary>Jsonb: signalen waar de planner bewust tegenin plande. Puur informatief.</summary>
    public string? WaarschuwingenBijToewijzing { get; set; }

    public Dienst Dienst { get; set; } = null!;
    public Persoon Persoon { get; set; } = null!;
    public Persoon? ToegewezenDoorPersoon { get; set; }
}
