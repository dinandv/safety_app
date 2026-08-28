namespace BccSafety.Infrastructure.Entities;

public sealed class PersonAppRole
{
    public Guid PersonId { get; set; }
    public AppRole AppRole { get; set; }

    public Person Person { get; set; } = null!;
}
