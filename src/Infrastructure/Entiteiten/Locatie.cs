namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Locatie
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Naam { get; set; }
    public string? Adres { get; set; }
    public string? QrSlug { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
