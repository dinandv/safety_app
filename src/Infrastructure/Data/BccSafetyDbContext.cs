using BccSafety.Infrastructure.Entiteiten;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Infrastructure.Data;

/// <summary>
/// EF Core is hier de ergonomie, niet de beveiliging. De query filters
/// hieronder dupliceren de Postgres row-level security-policies uit
/// db/001_tenancy_rls.sql zodat een vergeten .Where(tenantId) geen gat
/// slaat, maar de policies zijn het echte vangnet als deze filters ooit
/// niet kloppen.
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
    public DbSet<Persoon> Personen => Set<Persoon>();
    public DbSet<PersoonApprol> PersoonApprollen => Set<PersoonApprol>();
    public DbSet<Teamrol> Teamrollen => Set<Teamrol>();
    public DbSet<PersoonTeamrol> PersoonTeamrollen => Set<PersoonTeamrol>();
    public DbSet<KwalificatieType> KwalificatieTypen => Set<KwalificatieType>();
    public DbSet<Kwalificatie> Kwalificaties => Set<Kwalificatie>();
    public DbSet<Beschikbaarheid> Beschikbaarheden => Set<Beschikbaarheid>();
    public DbSet<Evenementtype> Evenementtypen => Set<Evenementtype>();
    public DbSet<PersoonEvenementtypeUitzondering> PersoonEvenementtypeUitzonderingen => Set<PersoonEvenementtypeUitzondering>();
    public DbSet<Dienstsjabloon> Dienstsjablonen => Set<Dienstsjabloon>();
    public DbSet<AgendaBron> AgendaBronnen => Set<AgendaBron>();
    public DbSet<KandidaatEvenement> KandidaatEvenementen => Set<KandidaatEvenement>();
    public DbSet<Locatie> Locaties => Set<Locatie>();
    public DbSet<Evenement> Evenementen => Set<Evenement>();
    public DbSet<AgendaAfwijking> AgendaAfwijkingen => Set<AgendaAfwijking>();
    public DbSet<EvenementGasttenant> EvenementGasttenanten => Set<EvenementGasttenant>();
    public DbSet<Dienst> Diensten => Set<Dienst>();
    public DbSet<Toewijzing> Toewijzingen => Set<Toewijzing>();
    public DbSet<Ruilverzoek> Ruilverzoeken => Set<Ruilverzoek>();
    public DbSet<Checkin> Checkins => Set<Checkin>();
    public DbSet<Richtlijn> Richtlijnen => Set<Richtlijn>();
    public DbSet<Document> Documenten => Set<Document>();
    public DbSet<Aandachtspunt> Aandachtspunten => Set<Aandachtspunt>();
    public DbSet<Contact> Contacten => Set<Contact>();
    public DbSet<Notificatie> Notificaties => Set<Notificatie>();
    public DbSet<Auditlog> Auditlogs => Set<Auditlog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Tenant en identiteit -----------------------------------------

        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenant");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasQueryFilter(x => x.Id == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Persoon>(e =>
        {
            e.ToTable("persoon");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersoonApprol>(e =>
        {
            e.ToTable("persoon_approl");
            e.HasKey(x => new { x.PersoonId, x.Approl });
            e.Property(x => x.Approl).HasConversion<string>();
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId);
            e.HasQueryFilter(x => x.Persoon.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Teamrol>(e =>
        {
            e.ToTable("teamrol");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Soort).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersoonTeamrol>(e =>
        {
            e.ToTable("persoon_teamrol");
            e.HasKey(x => new { x.PersoonId, x.TeamrolId });
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId);
            e.HasOne(x => x.Teamrol).WithMany().HasForeignKey(x => x.TeamrolId);
            e.HasQueryFilter(x => x.Persoon.TenantId == _tenantContext.TenantId);
        });

        // --- Kwalificaties en beschikbaarheid ------------------------------

        modelBuilder.Entity<KwalificatieType>(e =>
        {
            e.ToTable("kwalificatie_type");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.VereistVoorTeamrol).WithMany()
                .HasForeignKey(x => x.VereistVoorTeamrolId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Kwalificatie>(e =>
        {
            e.ToTable("kwalificatie");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId);
            e.HasOne(x => x.KwalificatieType).WithMany().HasForeignKey(x => x.KwalificatieTypeId);
            e.HasQueryFilter(x => x.Persoon.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Beschikbaarheid>(e =>
        {
            e.ToTable("beschikbaarheid");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Soort).HasConversion<string>();
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId);
            e.HasQueryFilter(x => x.Persoon.TenantId == _tenantContext.TenantId);
        });

        // --- Evenementen ----------------------------------------------------

        modelBuilder.Entity<Evenementtype>(e =>
        {
            e.ToTable("evenementtype");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.VereisteBekwaamheid).WithMany()
                .HasForeignKey(x => x.VereisteBekwaamheidId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<PersoonEvenementtypeUitzondering>(e =>
        {
            e.ToTable("persoon_evenementtype_uitzondering");
            e.HasKey(x => new { x.PersoonId, x.EvenementtypeId });
            e.Property(x => x.Oordeel).HasConversion<string>();
            e.HasOne(x => x.Persoon).WithMany()
                .HasForeignKey(x => x.PersoonId);
            e.HasOne(x => x.Evenementtype).WithMany()
                .HasForeignKey(x => x.EvenementtypeId);
            e.HasOne(x => x.VastgelegdDoorPersoon).WithMany()
                .HasForeignKey(x => x.VastgelegdDoorPersoonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.Persoon.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Dienstsjabloon>(e =>
        {
            e.ToTable("dienstsjabloon");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Evenementtype).WithMany().HasForeignKey(x => x.EvenementtypeId);
            e.HasOne(x => x.Teamrol).WithMany().HasForeignKey(x => x.TeamrolId);
            e.HasQueryFilter(x => x.Evenementtype.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<AgendaBron>(e =>
        {
            e.ToTable("agenda_bron");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<KandidaatEvenement>(e =>
        {
            e.ToTable("kandidaat_evenement");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.AgendaBronId, x.IcsUid, x.RecurrenceId }).IsUnique();
            e.HasOne(x => x.AgendaBron).WithMany().HasForeignKey(x => x.AgendaBronId);
            e.HasQueryFilter(x => x.AgendaBron.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Locatie>(e =>
        {
            e.ToTable("locatie");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Evenement>(e =>
        {
            e.ToTable("evenement");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.Bron).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.Evenementtype).WithMany().HasForeignKey(x => x.EvenementtypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KandidaatEvenement).WithMany().HasForeignKey(x => x.KandidaatEvenementId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Locatie).WithMany().HasForeignKey(x => x.LocatieId)
                .OnDelete(DeleteBehavior.Restrict);

            // evenement_lezen/invoegen/wijzigen/verwijderen in het RLS-script vervangen
            // de standaard tenant-policy; hier alleen de eigenaar-tenant, gasttoegang
            // is databasekant (RLS) geregeld en wordt bewust niet in EF gedupliceerd.
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<AgendaAfwijking>(e =>
        {
            e.ToTable("agenda_afwijking");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Soort).HasConversion<string>();
            e.HasOne(x => x.Evenement).WithMany().HasForeignKey(x => x.EvenementId);
            e.HasQueryFilter(x => x.Evenement.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<EvenementGasttenant>(e =>
        {
            e.ToTable("evenement_gasttenant");
            e.HasKey(x => new { x.EvenementId, x.TenantId });
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Evenement).WithMany().HasForeignKey(x => x.EvenementId);
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);

            // Zichtbaarheid (eigenaar én genodigde gast) staat in de RLS-policy;
            // hier geen filter zodat een gast zijn eigen uitnodigingsrij ziet.
        });

        // --- Rooster ----------------------------------------------------------

        modelBuilder.Entity<Dienst>(e =>
        {
            e.ToTable("dienst");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Evenement).WithMany().HasForeignKey(x => x.EvenementId);
            e.HasOne(x => x.Teamrol).WithMany().HasForeignKey(x => x.TeamrolId)
                .OnDelete(DeleteBehavior.Restrict);

            // De RLS-policy laat elke tenant die het evenement ziet ook de dienst
            // lezen (ook gasttenants); dat filteren we hier bewust niet na.
        });

        modelBuilder.Entity<Toewijzing>(e =>
        {
            e.ToTable("toewijzing");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.WaarschuwingenBijToewijzing).HasColumnType("jsonb");
            e.HasOne(x => x.Dienst).WithMany().HasForeignKey(x => x.DienstId);
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToegewezenDoorPersoon).WithMany()
                .HasForeignKey(x => x.ToegewezenDoor)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ruilverzoek>(e =>
        {
            e.ToTable("ruilverzoek");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Soort).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Dienst).WithMany().HasForeignKey(x => x.DienstId);
            e.HasOne(x => x.Toewijzing).WithMany().HasForeignKey(x => x.ToewijzingId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.AangevraagdDoorPersoon).WithMany()
                .HasForeignKey(x => x.AangevraagdDoorPersoonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.DoelPersoon).WithMany()
                .HasForeignKey(x => x.DoelPersoonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Checkin>(e =>
        {
            e.ToTable("checkin");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Methode).HasConversion<string>();
            e.HasOne(x => x.Toewijzing).WithMany().HasForeignKey(x => x.ToewijzingId);
            e.HasOne(x => x.DoorPersoon).WithMany()
                .HasForeignKey(x => x.DoorPersoonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Informatielaag -----------------------------------------------------

        modelBuilder.Entity<Richtlijn>(e =>
        {
            e.ToTable("richtlijn");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Zichtbaarheid).HasConversion<string>();
            e.Property(x => x.Soort).HasConversion<string>();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.BijgewerktDoorPersoon).WithMany()
                .HasForeignKey(x => x.BijgewerktDoor)
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

        modelBuilder.Entity<Aandachtspunt>(e =>
        {
            e.ToTable("aandachtspunt");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.Evenementtype).WithMany()
                .HasForeignKey(x => x.EvenementtypeId)
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

        // --- Techniek -------------------------------------------------------------

        modelBuilder.Entity<Notificatie>(e =>
        {
            e.ToTable("notificatie");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Kanaal).HasConversion<string>();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.Persoon).WithMany().HasForeignKey(x => x.PersoonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });

        modelBuilder.Entity<Auditlog>(e =>
        {
            e.ToTable("auditlog");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.OudeWaarde).HasColumnType("jsonb");
            e.Property(x => x.NieuweWaarde).HasColumnType("jsonb");
            e.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            e.HasOne(x => x.ActorPersoon).WithMany()
                .HasForeignKey(x => x.ActorPersoonId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.TenantId == _tenantContext.TenantId);
        });
    }
}
