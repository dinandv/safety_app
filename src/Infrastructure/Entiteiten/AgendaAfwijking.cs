namespace BccSafety.Infrastructure.Entiteiten;

public sealed class AgendaAfwijking
{
    public Guid Id { get; set; }
    public Guid EvenementId { get; set; }
    public AgendaAfwijkingSoort Soort { get; set; }
    public DateTimeOffset GedetecteerdOp { get; set; }
    public DateTimeOffset? AfgehandeldOp { get; set; }

    public Evenement Evenement { get; set; } = null!;
}
