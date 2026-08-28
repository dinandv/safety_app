namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Aandachtspunt
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Titel { get; set; }
    public required string Tekst { get; set; }
    public DateOnly GeldigVan { get; set; }
    public DateOnly GeldigTot { get; set; }
    public Guid? EvenementtypeId { get; set; }
    public int Prioriteit { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Evenementtype? Evenementtype { get; set; }
}
