namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Notificatie
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PersoonId { get; set; }
    public NotificatieKanaal Kanaal { get; set; }
    public required string Sjabloon { get; set; }
    public Guid ContextId { get; set; }
    public DateTimeOffset GeplandOp { get; set; }
    public DateTimeOffset? VerzondenOp { get; set; }
    public required string Status { get; set; }

    /// <summary>Voorkomt dubbele verzending na een herstart van de scheduler.</summary>
    public required string IdempotencyKey { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Persoon Persoon { get; set; } = null!;
}
