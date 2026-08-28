namespace BccSafety.Infrastructure.Entiteiten;

public sealed class AgendaBron
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string IcsUrl { get; set; }
    public DateTimeOffset? LaatsteSyncOp { get; set; }
    public string? LaatsteSyncStatus { get; set; }
    public bool Actief { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
