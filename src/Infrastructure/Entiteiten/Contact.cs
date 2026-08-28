namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Contact
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Naam { get; set; }
    public string? Functie { get; set; }
    public required string Telefoon { get; set; }
    public bool IsNoodnummer { get; set; }
    public int Volgorde { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
