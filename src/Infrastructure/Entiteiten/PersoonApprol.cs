namespace BccSafety.Infrastructure.Entiteiten;

public sealed class PersoonApprol
{
    public Guid PersoonId { get; set; }
    public Approl Approl { get; set; }

    public Persoon Persoon { get; set; } = null!;
}
