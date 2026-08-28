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
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_source",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ics_url = table.Column<string>(type: "text", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_sync_status = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_source", x => x.id);
                    table.ForeignKey(
                        name: "fk_calendar_source_tenant_tenant_id",
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
                    name = table.Column<string>(type: "text", nullable: false),
                    function = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: false),
                    is_emergency_number = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
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
                    title = table.Column<string>(type: "text", nullable: false),
                    version_label = table.Column<string>(type: "text", nullable: false),
                    file_ref = table.Column<string>(type: "text", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "location",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    qr_slug = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_location", x => x.id);
                    table.ForeignKey(
                        name: "fk_location_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name_prefix = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "text", nullable: true),
                    chat_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    stopped_on = table.Column<DateOnly>(type: "date", nullable: true),
                    pseudonymized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person", x => x.id);
                    table.ForeignKey(
                        name: "fk_person_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    vest_color = table.Column<string>(type: "text", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_team_role", x => x.id);
                    table.ForeignKey(
                        name: "fk_team_role_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "candidate_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    calendar_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ics_uid = table.Column<string>(type: "text", nullable: false),
                    recurrence_id = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    location_text = table.Column<string>(type: "text", nullable: true),
                    content_hash = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_candidate_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_candidate_event_calendar_source_calendar_source_id",
                        column: x => x.calendar_source_id,
                        principalTable: "calendar_source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "action_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_action_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_action_token_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_log_person_actor_person_id",
                        column: x => x.actor_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_audit_log_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "availability",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_availability", x => x.id);
                    table.ForeignKey(
                        name: "fk_availability_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guideline",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    sanitized_html = table.Column<string>(type: "text", nullable: false),
                    visibility = table.Column<string>(type: "text", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guideline", x => x.id);
                    table.ForeignKey(
                        name: "fk_guideline_person_updated_by",
                        column: x => x.updated_by,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guideline_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    idempotency_key = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notification_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_app_role",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    app_role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_app_role", x => new { x.person_id, x.app_role });
                    table.ForeignKey(
                        name: "fk_person_app_role_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    target_audience_description = table.Column<string>(type: "text", nullable: true),
                    target_age_from = table.Column<int>(type: "integer", nullable: true),
                    target_age_to = table.Column<int>(type: "integer", nullable: true),
                    deployable_age_from = table.Column<int>(type: "integer", nullable: true),
                    deployable_age_to = table.Column<int>(type: "integer", nullable: true),
                    required_skill_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expected_visitor_count = table.Column<int>(type: "integer", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_type", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_type_team_role_required_skill_id",
                        column: x => x.required_skill_id,
                        principalTable: "team_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_event_type_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_team_role",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    self_confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_team_role", x => new { x.person_id, x.team_role_id });
                    table.ForeignKey(
                        name: "fk_person_team_role_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_team_role_team_role_team_role_id",
                        column: x => x.team_role_id,
                        principalTable: "team_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qualification_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    required_for_team_role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_validity_months = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualification_type", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualification_type_team_role_required_for_team_role_id",
                        column: x => x.required_for_team_role_id,
                        principalTable: "team_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_qualification_type_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "advisory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: false),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_advisory", x => x.id);
                    table.ForeignKey(
                        name: "fk_advisory_event_type_event_type_id",
                        column: x => x.event_type_id,
                        principalTable: "event_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_advisory_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    candidate_event_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_candidate_event_candidate_event_id",
                        column: x => x.candidate_event_id,
                        principalTable: "candidate_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_event_event_type_event_type_id",
                        column: x => x.event_type_id,
                        principalTable: "event_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_location_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_event_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_event_type_exception",
                columns: table => new
                {
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verdict = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    recorded_by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_event_type_exception", x => new { x.person_id, x.event_type_id });
                    table.ForeignKey(
                        name: "fk_person_event_type_exception_event_type_event_type_id",
                        column: x => x.event_type_id,
                        principalTable: "event_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_event_type_exception_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_event_type_exception_person_recorded_by_person_id",
                        column: x => x.recorded_by_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shift_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    start_offset_minutes = table.Column<int>(type: "integer", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    deployable_age_from = table.Column<int>(type: "integer", nullable: true),
                    deployable_age_to = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_template", x => x.id);
                    table.ForeignKey(
                        name: "fk_shift_template_event_type_event_type_id",
                        column: x => x.event_type_id,
                        principalTable: "event_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shift_template_team_role_team_role_id",
                        column: x => x.team_role_id,
                        principalTable: "team_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qualification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    qualification_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    obtained_on = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualification", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualification_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_qualification_qualification_type_qualification_type_id",
                        column: x => x.qualification_type_id,
                        principalTable: "qualification_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "calendar_mismatch",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calendar_mismatch", x => x.id);
                    table.ForeignKey(
                        name: "fk_calendar_mismatch_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_guest_tenant",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_guest_tenant", x => new { x.event_id, x.tenant_id });
                    table.ForeignKey(
                        name: "fk_event_guest_tenant_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_guest_tenant_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shift",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    required_count = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift", x => x.id);
                    table.ForeignKey(
                        name: "fk_shift_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shift_team_role_team_role_id",
                        column: x => x.team_role_id,
                        principalTable: "team_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    withdrawn_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    withdrawal_reason = table.Column<string>(type: "text", nullable: true),
                    warnings_at_assignment = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_assignment_person_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assignment_person_person_id",
                        column: x => x.person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_assignment_shift_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shift",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_in",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_check_in", x => x.id);
                    table.ForeignKey(
                        name: "fk_check_in_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_check_in_person_by_person_id",
                        column: x => x.by_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "swap_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_by_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_swap_request", x => x.id);
                    table.ForeignKey(
                        name: "fk_swap_request_assignment_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "assignment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_swap_request_person_requested_by_person_id",
                        column: x => x.requested_by_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_swap_request_person_target_person_id",
                        column: x => x.target_person_id,
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_swap_request_shift_shift_id",
                        column: x => x.shift_id,
                        principalTable: "shift",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_action_token_person_id",
                table: "action_token",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_action_token_token_hash",
                table: "action_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_advisory_event_type_id",
                table: "advisory",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_advisory_tenant_id",
                table: "advisory",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_assigned_by",
                table: "assignment",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_person_id",
                table: "assignment",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_assignment_shift_id",
                table: "assignment",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_actor_person_id",
                table: "audit_log",
                column: "actor_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_tenant_id",
                table: "audit_log",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_availability_person_id",
                table: "availability",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_mismatch_event_id",
                table: "calendar_mismatch",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_calendar_source_tenant_id",
                table: "calendar_source",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_candidate_event_calendar_source_id_ics_uid_recurrence_id",
                table: "candidate_event",
                columns: new[] { "calendar_source_id", "ics_uid", "recurrence_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_check_in_assignment_id",
                table: "check_in",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_check_in_by_person_id",
                table: "check_in",
                column: "by_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_tenant_id",
                table: "contact",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_document_tenant_id",
                table: "document",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_candidate_event_id",
                table: "event",
                column: "candidate_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_event_type_id",
                table: "event",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_location_id",
                table: "event",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_tenant_id",
                table: "event",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_guest_tenant_tenant_id",
                table: "event_guest_tenant",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_type_required_skill_id",
                table: "event_type",
                column: "required_skill_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_type_tenant_id",
                table: "event_type",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_guideline_tenant_id",
                table: "guideline",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_guideline_updated_by",
                table: "guideline",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_location_tenant_id",
                table: "location",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_idempotency_key",
                table: "notification",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_person_id",
                table: "notification",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_tenant_id",
                table: "notification",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_tenant_id",
                table: "person",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_event_type_exception_event_type_id",
                table: "person_event_type_exception",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_event_type_exception_recorded_by_person_id",
                table: "person_event_type_exception",
                column: "recorded_by_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_person_team_role_team_role_id",
                table: "person_team_role",
                column: "team_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualification_person_id",
                table: "qualification",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualification_qualification_type_id",
                table: "qualification",
                column: "qualification_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualification_type_required_for_team_role_id",
                table: "qualification_type",
                column: "required_for_team_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualification_type_tenant_id",
                table: "qualification_type",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_shift_event_id",
                table: "shift",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_shift_team_role_id",
                table: "shift",
                column: "team_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_shift_template_event_type_id",
                table: "shift_template",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_shift_template_team_role_id",
                table: "shift_template",
                column: "team_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_swap_request_assignment_id",
                table: "swap_request",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_swap_request_requested_by_person_id",
                table: "swap_request",
                column: "requested_by_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_swap_request_shift_id",
                table: "swap_request",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_swap_request_target_person_id",
                table: "swap_request",
                column: "target_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_team_role_tenant_id",
                table: "team_role",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_slug",
                table: "tenant",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "action_token");

            migrationBuilder.DropTable(
                name: "advisory");

            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "availability");

            migrationBuilder.DropTable(
                name: "calendar_mismatch");

            migrationBuilder.DropTable(
                name: "check_in");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "event_guest_tenant");

            migrationBuilder.DropTable(
                name: "guideline");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "person_app_role");

            migrationBuilder.DropTable(
                name: "person_event_type_exception");

            migrationBuilder.DropTable(
                name: "person_team_role");

            migrationBuilder.DropTable(
                name: "qualification");

            migrationBuilder.DropTable(
                name: "shift_template");

            migrationBuilder.DropTable(
                name: "swap_request");

            migrationBuilder.DropTable(
                name: "qualification_type");

            migrationBuilder.DropTable(
                name: "assignment");

            migrationBuilder.DropTable(
                name: "person");

            migrationBuilder.DropTable(
                name: "shift");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "candidate_event");

            migrationBuilder.DropTable(
                name: "event_type");

            migrationBuilder.DropTable(
                name: "location");

            migrationBuilder.DropTable(
                name: "calendar_source");

            migrationBuilder.DropTable(
                name: "team_role");

            migrationBuilder.DropTable(
                name: "tenant");
        }
    }
}
