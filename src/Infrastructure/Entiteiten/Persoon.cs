namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Persoon
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Voornaam { get; set; }
    public string? Tussenvoegsel { get; set; }
    public required string Achternaam { get; set; }
    public DateOnly Geboortedatum { get; set; }
    public required string Email { get; set; }
    public string? Telefoon { get; set; }
    public string? ChatId { get; set; }
    public PersoonStatus Status { get; set; } = PersoonStatus.actief;
    public DateOnly? GestoptOp { get; set; }
    public DateTimeOffset? GepseudonimiseerdOp { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
