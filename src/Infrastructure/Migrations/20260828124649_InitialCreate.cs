using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BccSafety.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    naam = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    actief = table.Column<bool>(type: "boolean", nullable: false),
                    aangemaakt_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agenda_bron",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ics_url = table.Column<string>(type: "text", nullable: false),
                    laatste_sync_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    laatste_sync_status = table.Column<string>(type: "text", nullable: true),
                    actief = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agenda_bron", x => x.id);
                    table.ForeignKey(
                        name: "fk_agenda_bron_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    naam = table.Column<string>(type: "text", nullable: false),
                    functie = table.Column<string>(type: "text", nullable: true),
                    telefoon = table.Column<string>(type: "text", nullable: false),
                    is_noodnummer = table.Column<bool>(type: "boolean", nullable: false),
                    volgorde = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "text", nullable: false),
                    versie_label = table.Column<string>(type: "text", nullable: false),
                    bestand_ref = table.Column<string>(type: "text", nullable: false),
                    is_actueel = table.Column<bool>(type: "boolean", nullable: false),
                    gepubliceerd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document", x => x.id);
                    table.ForeignKey(
                        name: "fk_document_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locatie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    naam = table.Column<string>(type: "text", nullable: false),
                    adres = table.Column<string>(type: "text", nullable: true),
                    qr_slug = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locatie", x => x.id);
                    table.ForeignKey(
                        name: "fk_locatie_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persoon",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voornaam = table.Column<string>(type: "text", nullable: false),
                    tussenvoegsel = table.Column<string>(type: "text", nullable: true),
                    achternaam = table.Column<string>(type: "text", nullable: false),
                    geboortedatum = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefoon = table.Column<string>(type: "text", nullable: true),
                    chat_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    gestopt_op = table.Column<DateOnly>(type: "date", nullable: true),
                    gepseudonimiseerd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persoon", x => x.id);
                    table.ForeignKey(
                        name: "fk_persoon_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teamrol",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    naam = table.Column<string>(type: "text", nullable: false),
                    soort = table.Column<string>(type: "text", nullable: false),
                    hesje_kleur = table.Column<string>(type: "text", nullable: true),
                    actief = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teamrol", x => x.id);
                    table.ForeignKey(
                        name: "fk_teamrol_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kandidaat_evenement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    agenda_bron_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ics_uid = table.Column<string>(type: "text", nullable: false),
                    recurrence_id = table.Column<string>(type: "text", nullable: true),
                    titel = table.Column<string>(type: "text", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eind = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    locatie_tekst = table.Column<string>(type: "text", nullable: true),
                    inhoud_hash = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kandidaat_evenement", x => x.id);
                    table.ForeignKey(
                        name: "fk_kandidaat_evenement_agenda_bron_agenda_bron_id",
                        column: x => x.agenda_bron_id,
                        principalTable: "agenda_bron",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditlog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_persoon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entiteit = table.Column<string>(type: "text", nullable: false),
                    entiteit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actie = table.Column<string>(type: "text", nullable: false),
                    oude_waarde = table.Column<string>(type: "jsonb", nullable: true),
                    nieuwe_waarde = table.Column<string>(type: "jsonb", nullable: true),
                    tijdstip = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditlog", x => x.id);
                    table.ForeignKey(
                        name: "fk_auditlog_persoon_actor_persoon_id",
                        column: x => x.actor_persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditlog_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "beschikbaarheid",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    van = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tot = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    soort = table.Column<string>(type: "text", nullable: false),
                    notitie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beschikbaarheid", x => x.id);
                    table.ForeignKey(
                        name: "fk_beschikbaarheid_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notificatie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kanaal = table.Column<string>(type: "text", nullable: false),
                    sjabloon = table.Column<string>(type: "text", nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gepland_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verzonden_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificatie", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificatie_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificatie_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persoon_approl",
                columns: table => new
                {
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persoon_approl", x => new { x.persoon_id, x.approl });
                    table.ForeignKey(
                        name: "fk_persoon_approl_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "richtlijn",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "text", nullable: false),
                    html_gesaniteerd = table.Column<string>(type: "text", nullable: false),
                    zichtbaarheid = table.Column<string>(type: "text", nullable: false),
                    soort = table.Column<string>(type: "text", nullable: false),
                    volgorde = table.Column<int>(type: "integer", nullable: false),
                    versie = table.Column<int>(type: "integer", nullable: false),
                    gepubliceerd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bijgewerkt_door = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_richtlijn", x => x.id);
                    table.ForeignKey(
                        name: "fk_richtlijn_persoon_bijgewerkt_door",
                        column: x => x.bijgewerkt_door,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_richtlijn_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evenementtype",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    naam = table.Column<string>(type: "text", nullable: false),
                    doelgroep_omschrijving = table.Column<string>(type: "text", nullable: true),
                    doelgroep_leeftijd_van = table.Column<int>(type: "integer", nullable: true),
                    doelgroep_leeftijd_tot = table.Column<int>(type: "integer", nullable: true),
                    inzetbaar_leeftijd_van = table.Column<int>(type: "integer", nullable: true),
                    inzetbaar_leeftijd_tot = table.Column<int>(type: "integer", nullable: true),
                    vereiste_bekwaamheid_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verwacht_aantal_bezoekers = table.Column<int>(type: "integer", nullable: true),
                    actief = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evenementtype", x => x.id);
                    table.ForeignKey(
                        name: "fk_evenementtype_teamrol_vereiste_bekwaamheid_id",
                        column: x => x.vereiste_bekwaamheid_id,
                        principalTable: "teamrol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evenementtype_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kwalificatie_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    naam = table.Column<string>(type: "text", nullable: false),
                    vereist_voor_teamrol_id = table.Column<Guid>(type: "uuid", nullable: true),
                    standaard_geldigheid_maanden = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kwalificatie_type", x => x.id);
                    table.ForeignKey(
                        name: "fk_kwalificatie_type_teamrol_vereist_voor_teamrol_id",
                        column: x => x.vereist_voor_teamrol_id,
                        principalTable: "teamrol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_kwalificatie_type_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persoon_teamrol",
                columns: table => new
                {
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teamrol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bevestigd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bevestigd_door_persoon_zelf = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persoon_teamrol", x => new { x.persoon_id, x.teamrol_id });
                    table.ForeignKey(
                        name: "fk_persoon_teamrol_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_persoon_teamrol_teamrol_teamrol_id",
                        column: x => x.teamrol_id,
                        principalTable: "teamrol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aandachtspunt",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "text", nullable: false),
                    tekst = table.Column<string>(type: "text", nullable: false),
                    geldig_van = table.Column<DateOnly>(type: "date", nullable: false),
                    geldig_tot = table.Column<DateOnly>(type: "date", nullable: false),
                    evenementtype_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prioriteit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aandachtspunt", x => x.id);
                    table.ForeignKey(
                        name: "fk_aandachtspunt_evenementtype_evenementtype_id",
                        column: x => x.evenementtype_id,
                        principalTable: "evenementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_aandachtspunt_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dienstsjabloon",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    evenementtype_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teamrol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aantal = table.Column<int>(type: "integer", nullable: false),
                    start_offset_minuten = table.Column<int>(type: "integer", nullable: false),
                    duur_minuten = table.Column<int>(type: "integer", nullable: false),
                    inzetbaar_leeftijd_van = table.Column<int>(type: "integer", nullable: true),
                    inzetbaar_leeftijd_tot = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dienstsjabloon", x => x.id);
                    table.ForeignKey(
                        name: "fk_dienstsjabloon_evenementtype_evenementtype_id",
                        column: x => x.evenementtype_id,
                        principalTable: "evenementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dienstsjabloon_teamrol_teamrol_id",
                        column: x => x.teamrol_id,
                        principalTable: "teamrol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "evenement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evenementtype_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kandidaat_evenement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    locatie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titel = table.Column<string>(type: "text", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eind = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    bron = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evenement", x => x.id);
                    table.ForeignKey(
                        name: "fk_evenement_evenementtype_evenementtype_id",
                        column: x => x.evenementtype_id,
                        principalTable: "evenementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evenement_kandidaat_evenement_kandidaat_evenement_id",
                        column: x => x.kandidaat_evenement_id,
                        principalTable: "kandidaat_evenement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_evenement_locatie_locatie_id",
                        column: x => x.locatie_id,
                        principalTable: "locatie",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evenement_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persoon_evenementtype_uitzondering",
                columns: table => new
                {
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evenementtype_id = table.Column<Guid>(type: "uuid", nullable: false),
                    oordeel = table.Column<string>(type: "text", nullable: false),
                    reden = table.Column<string>(type: "text", nullable: false),
                    vastgelegd_door_persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vastgelegd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persoon_evenementtype_uitzondering", x => new { x.persoon_id, x.evenementtype_id });
                    table.ForeignKey(
                        name: "fk_persoon_evenementtype_uitzondering_evenementtype_evenementt",
                        column: x => x.evenementtype_id,
                        principalTable: "evenementtype",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_persoon_evenementtype_uitzondering_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_persoon_evenementtype_uitzondering_persoon_vastgelegd_door_",
                        column: x => x.vastgelegd_door_persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "kwalificatie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kwalificatie_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    behaald_op = table.Column<DateOnly>(type: "date", nullable: false),
                    geldig_tot = table.Column<DateOnly>(type: "date", nullable: true),
                    notitie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kwalificatie", x => x.id);
                    table.ForeignKey(
                        name: "fk_kwalificatie_kwalificatie_type_kwalificatie_type_id",
                        column: x => x.kwalificatie_type_id,
                        principalTable: "kwalificatie_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kwalificatie_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agenda_afwijking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    evenement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    soort = table.Column<string>(type: "text", nullable: false),
                    gedetecteerd_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    afgehandeld_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agenda_afwijking", x => x.id);
                    table.ForeignKey(
                        name: "fk_agenda_afwijking_evenement_evenement_id",
                        column: x => x.evenement_id,
                        principalTable: "evenement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dienst",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    evenement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teamrol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    eind = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    benodigd_aantal = table.Column<int>(type: "integer", nullable: false),
                    notitie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dienst", x => x.id);
                    table.ForeignKey(
                        name: "fk_dienst_evenement_evenement_id",
                        column: x => x.evenement_id,
                        principalTable: "evenement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dienst_teamrol_teamrol_id",
                        column: x => x.teamrol_id,
                        principalTable: "teamrol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evenement_gasttenant",
                columns: table => new
                {
                    evenement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    eigenaar_tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evenement_gasttenant", x => new { x.evenement_id, x.tenant_id });
                    table.ForeignKey(
                        name: "fk_evenement_gasttenant_evenement_evenement_id",
                        column: x => x.evenement_id,
                        principalTable: "evenement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_evenement_gasttenant_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "toewijzing",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    dienst_id = table.Column<Guid>(type: "uuid", nullable: false),
                    persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    toegewezen_door = table.Column<Guid>(type: "uuid", nullable: true),
                    toegewezen_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    afgemeld_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    afmeld_reden = table.Column<string>(type: "text", nullable: true),
                    waarschuwingen_bij_toewijzing = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_toewijzing", x => x.id);
                    table.ForeignKey(
                        name: "fk_toewijzing_dienst_dienst_id",
                        column: x => x.dienst_id,
                        principalTable: "dienst",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_toewijzing_persoon_persoon_id",
                        column: x => x.persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_toewijzing_persoon_toegewezen_door",
                        column: x => x.toegewezen_door,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "checkin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    toewijzing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    methode = table.Column<string>(type: "text", nullable: false),
                    door_persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tijdstip = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkin", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkin_persoon_door_persoon_id",
                        column: x => x.door_persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_checkin_toewijzing_toewijzing_id",
                        column: x => x.toewijzing_id,
                        principalTable: "toewijzing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ruilverzoek",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    dienst_id = table.Column<Guid>(type: "uuid", nullable: false),
                    toewijzing_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aangevraagd_door_persoon_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doel_persoon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    soort = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    verloopt_op = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ruilverzoek", x => x.id);
                    table.ForeignKey(
                        name: "fk_ruilverzoek_dienst_dienst_id",
                        column: x => x.dienst_id,
                        principalTable: "dienst",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ruilverzoek_persoon_aangevraagd_door_persoon_id",
                        column: x => x.aangevraagd_door_persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ruilverzoek_persoon_doel_persoon_id",
                        column: x => x.doel_persoon_id,
                        principalTable: "persoon",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ruilverzoek_toewijzing_toewijzing_id",
                        column: x => x.toewijzing_id,
                        principalTable: "toewijzing",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aandachtspunt_evenementtype_id",
                table: "aandachtspunt",
                column: "evenementtype_id");

            migrationBuilder.CreateIndex(
                name: "ix_aandachtspunt_tenant_id",
                table: "aandachtspunt",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenda_afwijking_evenement_id",
                table: "agenda_afwijking",
                column: "evenement_id");

            migrationBuilder.CreateIndex(
                name: "ix_agenda_bron_tenant_id",
                table: "agenda_bron",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditlog_actor_persoon_id",
                table: "auditlog",
                column: "actor_persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_auditlog_tenant_id",
                table: "auditlog",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_beschikbaarheid_persoon_id",
                table: "beschikbaarheid",
                column: "persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkin_door_persoon_id",
                table: "checkin",
                column: "door_persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkin_toewijzing_id",
                table: "checkin",
                column: "toewijzing_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_tenant_id",
                table: "contact",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dienst_evenement_id",
                table: "dienst",
                column: "evenement_id");

            migrationBuilder.CreateIndex(
                name: "ix_dienst_teamrol_id",
                table: "dienst",
                column: "teamrol_id");

            migrationBuilder.CreateIndex(
                name: "ix_dienstsjabloon_evenementtype_id",
                table: "dienstsjabloon",
                column: "evenementtype_id");

            migrationBuilder.CreateIndex(
                name: "ix_dienstsjabloon_teamrol_id",
                table: "dienstsjabloon",
                column: "teamrol_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_tenant_id",
                table: "document",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenement_evenementtype_id",
                table: "evenement",
                column: "evenementtype_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenement_kandidaat_evenement_id",
                table: "evenement",
                column: "kandidaat_evenement_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenement_locatie_id",
                table: "evenement",
                column: "locatie_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenement_tenant_id",
                table: "evenement",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenement_gasttenant_tenant_id",
                table: "evenement_gasttenant",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenementtype_tenant_id",
                table: "evenementtype",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_evenementtype_vereiste_bekwaamheid_id",
                table: "evenementtype",
                column: "vereiste_bekwaamheid_id");

            migrationBuilder.CreateIndex(
                name: "ix_kandidaat_evenement_agenda_bron_id_ics_uid_recurrence_id",
                table: "kandidaat_evenement",
                columns: new[] { "agenda_bron_id", "ics_uid", "recurrence_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kwalificatie_kwalificatie_type_id",
                table: "kwalificatie",
                column: "kwalificatie_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_kwalificatie_persoon_id",
                table: "kwalificatie",
                column: "persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_kwalificatie_type_tenant_id",
                table: "kwalificatie_type",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_kwalificatie_type_vereist_voor_teamrol_id",
                table: "kwalificatie_type",
                column: "vereist_voor_teamrol_id");

            migrationBuilder.CreateIndex(
                name: "ix_locatie_tenant_id",
                table: "locatie",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificatie_idempotency_key",
                table: "notificatie",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notificatie_persoon_id",
                table: "notificatie",
                column: "persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificatie_tenant_id",
                table: "notificatie",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_persoon_tenant_id",
                table: "persoon",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_persoon_evenementtype_uitzondering_evenementtype_id",
                table: "persoon_evenementtype_uitzondering",
                column: "evenementtype_id");

            migrationBuilder.CreateIndex(
                name: "ix_persoon_evenementtype_uitzondering_vastgelegd_door_persoon_",
                table: "persoon_evenementtype_uitzondering",
                column: "vastgelegd_door_persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_persoon_teamrol_teamrol_id",
                table: "persoon_teamrol",
                column: "teamrol_id");

            migrationBuilder.CreateIndex(
                name: "ix_richtlijn_bijgewerkt_door",
                table: "richtlijn",
                column: "bijgewerkt_door");

            migrationBuilder.CreateIndex(
                name: "ix_richtlijn_tenant_id",
                table: "richtlijn",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_ruilverzoek_aangevraagd_door_persoon_id",
                table: "ruilverzoek",
                column: "aangevraagd_door_persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_ruilverzoek_dienst_id",
                table: "ruilverzoek",
                column: "dienst_id");

            migrationBuilder.CreateIndex(
                name: "ix_ruilverzoek_doel_persoon_id",
                table: "ruilverzoek",
                column: "doel_persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_ruilverzoek_toewijzing_id",
                table: "ruilverzoek",
                column: "toewijzing_id");

            migrationBuilder.CreateIndex(
                name: "ix_teamrol_tenant_id",
                table: "teamrol",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_slug",
                table: "tenant",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_toewijzing_dienst_id",
                table: "toewijzing",
                column: "dienst_id");

            migrationBuilder.CreateIndex(
                name: "ix_toewijzing_persoon_id",
                table: "toewijzing",
                column: "persoon_id");

            migrationBuilder.CreateIndex(
                name: "ix_toewijzing_toegewezen_door",
                table: "toewijzing",
                column: "toegewezen_door");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aandachtspunt");

            migrationBuilder.DropTable(
                name: "agenda_afwijking");

            migrationBuilder.DropTable(
                name: "auditlog");

            migrationBuilder.DropTable(
                name: "beschikbaarheid");

            migrationBuilder.DropTable(
                name: "checkin");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "dienstsjabloon");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "evenement_gasttenant");

            migrationBuilder.DropTable(
                name: "kwalificatie");

            migrationBuilder.DropTable(
                name: "notificatie");

            migrationBuilder.DropTable(
                name: "persoon_approl");

            migrationBuilder.DropTable(
                name: "persoon_evenementtype_uitzondering");

            migrationBuilder.DropTable(
                name: "persoon_teamrol");

            migrationBuilder.DropTable(
                name: "richtlijn");

            migrationBuilder.DropTable(
                name: "ruilverzoek");

            migrationBuilder.DropTable(
                name: "kwalificatie_type");

            migrationBuilder.DropTable(
                name: "toewijzing");

            migrationBuilder.DropTable(
                name: "dienst");

            migrationBuilder.DropTable(
                name: "persoon");

            migrationBuilder.DropTable(
                name: "evenement");

            migrationBuilder.DropTable(
                name: "evenementtype");

            migrationBuilder.DropTable(
                name: "kandidaat_evenement");

            migrationBuilder.DropTable(
                name: "locatie");

            migrationBuilder.DropTable(
                name: "teamrol");

            migrationBuilder.DropTable(
                name: "agenda_bron");

            migrationBuilder.DropTable(
                name: "tenant");
        }
    }
}
