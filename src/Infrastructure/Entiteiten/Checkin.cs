namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Checkin
{
    public Guid Id { get; set; }
    public Guid ToewijzingId { get; set; }
    public CheckinMethode Methode { get; set; }
    public Guid DoorPersoonId { get; set; }
    public DateTimeOffset Tijdstip { get; set; }

    public Toewijzing Toewijzing { get; set; } = null!;
    public Persoon DoorPersoon { get; set; } = null!;
}
