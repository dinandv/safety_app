namespace BccSafety.Infrastructure.Entiteiten;

public sealed class EvenementGasttenant
{
    public Guid EvenementId { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>
    /// Denormalisatie van Evenement.TenantId, gezet bij het aanmaken van de
    /// uitnodiging. Nodig om de RLS-policy op deze tabel niet in evenement te
    /// laten kijken — dat zou een cirkelverwijzing geven met evenement_lezen,
    /// die op zijn beurt deze tabel raadpleegt.
    /// </summary>
    public Guid EigenaarTenantId { get; set; }

    public GasttenantStatus Status { get; set; } = GasttenantStatus.uitgenodigd;

    public Evenement Evenement { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
