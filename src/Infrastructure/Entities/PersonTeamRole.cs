namespace BccSafety.Infrastructure.Entities;

public sealed class PersonTeamRole
{
    public Guid PersonId { get; set; }
    public Guid TeamRoleId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public bool SelfConfirmed { get; set; }

    public Person Person { get; set; } = null!;
    public TeamRole TeamRole { get; set; } = null!;
}
