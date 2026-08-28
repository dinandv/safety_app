namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Teamrol
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Naam { get; set; }
    public TeamrolSoort Soort { get; set; }
    public string? HesjeKleur { get; set; }
    public bool Actief { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
