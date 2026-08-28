namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Document
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Titel { get; set; }
    public required string VersieLabel { get; set; }
    public required string BestandRef { get; set; }
    public bool IsActueel { get; set; }
    public DateTimeOffset GepubliceerdOp { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
