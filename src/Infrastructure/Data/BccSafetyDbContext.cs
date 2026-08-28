using BccSafety.Infrastructure.Entities;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// EF Core is the ergonomics here, not the security. The query filters
/// below duplicate the Postgres row-level security policies in
/// db/001_tenancy_rls.sql so a forgotten .Where(tenantId) doesn't open a
/// gap, but the policies are the real safety net if these filters are
/// ever wrong.
/// </summary>
public sealed class BccSafetyDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public BccSafetyDbContext(DbContextOptions<BccSafetyDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonAppRole> PersonAppRoles => Set<PersonAppRole>();
    public DbSet<TeamRole> TeamRoles => Set<TeamRole>();
    public DbSet<PersonTeamRole> PersonTeamRoles => Set<PersonTeamRole>();
    public DbSet<ActionToken> ActionTokens => Set<ActionToken>();
    public DbSet<QualificationType> QualificationTypes => Set<QualificationType>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<PersonEventTypeException> PersonEventTypeExceptions => Set<PersonEventTypeException>();
    public DbSet<ShiftTemplate> ShiftTemplates => Set<ShiftTemplate>();
    public DbSet<CalendarSource> CalendarSources => Set<CalendarSource>();
    public DbSet<CandidateEvent> CandidateEvents => Set<CandidateEvent>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<CalendarMismatch> CalendarMismatches => Set<CalendarMismatch>();
    public DbSet<EventGuestTenant> EventGuestTenants => Set<EventGuestTenant>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<Guideline> Guidelines => Set<Guideline>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Advisory> Advisories => Set<Advisory>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Tenant and identity -----------------------------------------

        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenant");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasQueryFilter(x => x.Id == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Person>(e =>
        {
            e.ToTable("person");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersonAppRole>(e =>
        {
            e.ToTable("person_app_role");
            e.HasKey(x => new { x.PersonId, x.AppRole });
            e.Property(x => x.AppRole).HasConversion<string>();
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<TeamRole>(e =>
        {
            e.ToTable("team_role");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Kind).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersonTeamRole>(e =>
        {
            e.ToTable("person_team_role");
            e.HasKey(x => new { x.PersonId, x.TeamRoleId });
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            e.HasOne(x => x.TeamRole).WithMany().HasForeignKey(x => x.TeamRoleId);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<ActionToken>(e =>
        {
            e.ToTable("action_token");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Purpose).HasConversion<string>();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        // --- Qualifications and availability ------------------------------

        modelBuilder.Entity<QualificationType>(e =>
        {
            e.ToTable("qualification_type");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.RequiredForTeamRole).WithMany()
                .HasForeignKey(x => x.RequiredForTeamRoleId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Qualification>(e =>
        {
            e.ToTable("qualification");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            e.HasOne(x => x.QualificationType).WithMany().HasForeignKey(x => x.QualificationTypeId);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Availability>(e =>
        {
            e.ToTable("availability");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Kind).HasConversion<string>();
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        // --- Events ----------------------------------------------------------

        modelBuilder.Entity<EventType>(e =>
        {
            e.ToTable("event_type");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.RequiredSkill).WithMany()
                .HasForeignKey(x => x.RequiredSkillId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersonEventTypeException>(e =>
        {
            e.ToTable("person_event_type_exception");
            e.HasKey(x => new { x.PersonId, x.EventTypeId });
            e.Property(x => x.Verdict).HasConversion<string>();
            e.HasOne(x => x.Person).WithMany()
                .HasForeignKey(x => x.PersonId);
            e.HasOne(x => x.EventType).WithMany()
                .HasForeignKey(x => x.EventTypeId);
            e.HasOne(x => x.RecordedByPerson).WithMany()
                .HasForeignKey(x => x.RecordedByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.Person.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<ShiftTemplate>(e =>
        {
            e.ToTable("shift_template");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.EventType).WithMany().HasForeignKey(x => x.EventTypeId);
            e.HasOne(x => x.TeamRole).WithMany().HasForeignKey(x => x.TeamRoleId);
            e.HasQueryFilter(x => x.EventType.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<CalendarSource>(e =>
        {
            e.ToTable("calendar_source");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<CandidateEvent>(e =>
        {
            e.ToTable("candidate_event");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.CalendarSourceId, x.IcsUid, x.RecurrenceId }).IsUnique();
            e.HasOne(x => x.CalendarSource).WithMany().HasForeignKey(x => x.CalendarSourceId);
            e.HasQueryFilter(x => x.CalendarSource.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("location");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable("event");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Source).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.EventType).WithMany().HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CandidateEvent).WithMany().HasForeignKey(x => x.CandidateEventId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // evenement_lezen/invoegen/wijzigen/verwijderen in the RLS script replace
            // the standard tenant policy; only the owner tenant here, guest access
            // is handled database-side (RLS) and deliberately not duplicated in EF.
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<CalendarMismatch>(e =>
        {
            e.ToTable("calendar_mismatch");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Kind).HasConversion<string>();
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
            e.HasQueryFilter(x => x.Event.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<EventGuestTenant>(e =>
        {
            e.ToTable("event_guest_tenant");
            e.HasKey(x => new { x.EventId, x.TenantId });
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);

            // Visibility (owner and invited guest) lives in the RLS policy;
            // no filter here so a guest sees their own invite row.
        });

        // --- Roster ----------------------------------------------------------

        modelBuilder.Entity<Shift>(e =>
        {
            e.ToTable("shift");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
            e.HasOne(x => x.TeamRole).WithMany().HasForeignKey(x => x.TeamRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // The RLS policy lets any tenant that can see the event also read
            // the shift (guest tenants included); deliberately not re-filtered here.
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.ToTable("assignment");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.WarningsAtAssignment).HasColumnType("jsonb");
            e.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId);
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedByPerson).WithMany()
                .HasForeignKey(x => x.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SwapRequest>(e =>
        {
            e.ToTable("swap_request");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Kind).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Shift).WithMany().HasForeignKey(x => x.ShiftId);
            e.HasOne(x => x.Assignment).WithMany().HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RequestedByPerson).WithMany()
                .HasForeignKey(x => x.RequestedByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TargetPerson).WithMany()
                .HasForeignKey(x => x.TargetPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CheckIn>(e =>
        {
            e.ToTable("check_in");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Method).HasConversion<string>();
            e.HasOne(x => x.Assignment).WithMany().HasForeignKey(x => x.AssignmentId);
            e.HasOne(x => x.ByPerson).WithMany()
                .HasForeignKey(x => x.ByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Information layer -----------------------------------------------------

        modelBuilder.Entity<Guideline>(e =>
        {
            e.ToTable("guideline");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Visibility).HasConversion<string>();
            e.Property(x => x.Kind).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.UpdatedByPerson).WithMany()
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.ToTable("document");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Advisory>(e =>
        {
            e.ToTable("advisory");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.EventType).WithMany()
                .HasForeignKey(x => x.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Contact>(e =>
        {
            e.ToTable("contact");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        // --- Technical -------------------------------------------------------------

        modelBuilder.Entity<Notification>(e =>
        {
            e.ToTable("notification");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Channel).HasConversion<string>();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_log");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.OldValue).HasColumnType("jsonb");
            e.Property(x => x.NewValue).HasColumnType("jsonb");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.ActorPerson).WithMany()
                .HasForeignKey(x => x.ActorPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });
    }
}
