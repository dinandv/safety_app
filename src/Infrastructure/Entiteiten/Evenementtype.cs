namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Evenementtype
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Naam { get; set; }
    public string? DoelgroepOmschrijving { get; set; }
    public int? DoelgroepLeeftijdVan { get; set; }
    public int? DoelgroepLeeftijdTot { get; set; }
    public int? InzetbaarLeeftijdVan { get; set; }
    public int? InzetbaarLeeftijdTot { get; set; }
    public Guid? VereisteBekwaamheidId { get; set; }
    public int? VerwachtAantalBezoekers { get; set; }
    public bool Actief { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public Teamrol? VereisteBekwaamheid { get; set; }
}
