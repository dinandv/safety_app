namespace BccSafety.Infrastructure.Entiteiten;

public sealed class KwalificatieType
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Naam { get; set; }
    public Guid? VereistVoorTeamrolId { get; set; }
    public int? StandaardGeldigheidMaanden { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Teamrol? VereistVoorTeamrol { get; set; }
}
