namespace BccSafety.Infrastructure.Entiteiten;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public required string Naam { get; set; }
    public required string Slug { get; set; }
    public bool Actief { get; set; } = true;
    public DateTimeOffset AangemaaktOp { get; set; }
}
