using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace BccSafety.Tests.Participant;

/// <summary>
/// The real API against a real Postgres, connected as bcc_app with the
/// policies in place — the only configuration in which what these tests
/// prove says anything about production.
///
/// The seed is fictional throughout, and stays that way. No real
/// participant data goes near a database, test or otherwise.
/// </summary>
public sealed class ParticipantApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithUsername("bcc_owner")
        .WithDatabase("bccsafety")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public static readonly Guid Tenant = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string TenantHost = "demo.example.test";

    /// <summary>Scheduled on the event today, so this one sees numbers.</summary>
    public static readonly Guid Responder = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public const string ResponderEmail = "responder@example.test";

    /// <summary>A participant with no shift today, who must not see numbers.</summary>
    public static readonly Guid Bystander = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    public const string BystanderEmail = "bystander@example.test";

    public static readonly Guid FirstAidShift = Guid.Parse("cccccccc-0000-0000-0000-000000000002");

    /// <summary>
    /// An unfilled spot on a later day. The claim tests use this one so
    /// they cannot disturb what the day-overview tests assert about
    /// today.
    /// </summary>
    public static readonly Guid TomorrowShift = Guid.Parse("cccccccc-0000-0000-0000-000000000003");

    /// <summary>
    /// A spot in a role that requires a certificate nobody in the seed
    /// holds. The one hard exclusion in this application, so it gets a
    /// shift of its own to be tested against.
    /// </summary>
    public static readonly Guid CertifiedShift = Guid.Parse("cccccccc-0000-0000-0000-000000000004");

    /// <summary>Captures what the API would have mailed, so a test can log in.</summary>
    public CapturingEmailSender Email { get; } = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using (var owner = new NpgsqlConnection(_postgres.GetConnectionString()))
        {
            await owner.OpenAsync();
            await Tenancy.Migrations.ApplyAsync(owner);
            await SeedAsync(owner);
        }

        var appConnectionString = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Username = "bcc_app",
            Password = "test",
            Multiplexing = false,
        }.ToString();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Development so a failing request answers with the exception
            // rather than a bare 500; a test that only knows "something
            // broke" costs an hour to act on.
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Default", appConnectionString);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(Email);
            });
        });
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// A client that looks like a browser on this tenant's subdomain.
    /// The base address is https so the session cookie, which is marked
    /// Secure, is actually kept and sent back.
    /// </summary>
    public HttpClient CreateClient()
    {
        var client = _factory!.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/"),
            HandleCookies = true,
        });
        client.DefaultRequestHeaders.Add("X-Forwarded-Host", TenantHost);
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        return client;
    }

    private readonly Dictionary<string, HttpClient> _signedIn = new();

    /// <summary>
    /// A client with a live session for this address, signed in through
    /// the real magic-code flow rather than a forged cookie — so the
    /// login endpoints and the tenant check are covered too.
    ///
    /// One session per person, reused. Signing in afresh for every test
    /// runs into the login rate limiter, which is the limiter working
    /// rather than a test problem to configure away. It also matches how
    /// the app behaves: a volunteer signs in once and stays in for
    /// ninety days.
    /// </summary>
    public async Task<HttpClient> SignedInClientAsync(string email)
    {
        if (_signedIn.TryGetValue(email, out var existing)) return existing;

        var client = CreateClient();
        (await client.PostAsJsonAsync("/auth/login/request", new { email }))
            .EnsureSuccessStatusCode();

        var body = Email.BodyFor(email)
            ?? throw new InvalidOperationException($"No login mail was sent to {email}.");
        var code = Regex.Match(body, @"\b\d{6}\b").Value;
        if (code.Length == 0)
            throw new InvalidOperationException($"No six-digit code in the mail to {email}.");

        var confirmed = await client.PostAsJsonAsync("/auth/login/confirm", new { email, code });
        if (!confirmed.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Signing in as {email} failed: {(int)confirmed.StatusCode} " +
                await confirmed.Content.ReadAsStringAsync());

        _signedIn[email] = client;
        return client;
    }

    /// <summary>
    /// One event today with four roles, mirroring the shape the day
    /// overview was designed against: a lead responder, a filled
    /// supervision shift, and a first-aid spot that opened this morning
    /// when someone withdrew.
    /// </summary>
    private static async Task SeedAsync(NpgsqlConnection owner)
    {
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO tenant (id, name, slug, active, created_at)
                VALUES (@tenant, 'Demo', 'demo', true, now());

            INSERT INTO person (id, tenant_id, first_name, last_name_prefix, last_name,
                                date_of_birth, email, phone, status) VALUES
                (@responder, @tenant, 'Marit', NULL, 'Vermeer',
                 '1990-04-02', @responderEmail, '0612340001', 'Active'),
                (@bystander, @tenant, 'Nadia', 'el', 'Amrani',
                 '1988-11-19', @bystanderEmail, '0612340002', 'Active'),
                (@withdrawn, @tenant, 'Bas', NULL, 'Terpstra',
                 '1979-06-30', 'bas@example.test', '0612340003', 'Active');

            INSERT INTO person_app_role (person_id, app_role) VALUES
                (@responder, 'Participant'),
                (@bystander, 'Participant'),
                (@withdrawn, 'Participant');

            INSERT INTO team_role (id, tenant_id, name, kind, vest_color, active) VALUES
                (@leadRole, @tenant, 'Hoofd-BHV', 'ShiftRole', 'red', true),
                (@aidRole, @tenant, 'EHBO', 'ShiftRole', 'blue', true),
                (@guardRole, @tenant, 'Toezicht', 'ShiftRole', 'yellow', true);

            -- Required for Toezicht and held by nobody, so everyone is
            -- excluded from that role however willing they are.
            INSERT INTO qualification_type (id, tenant_id, name, required_for_team_role_id)
                VALUES (@certificate, @tenant, 'Toezichtcertificaat', @guardRole);

            INSERT INTO person_team_role (person_id, team_role_id, self_confirmed) VALUES
                (@responder, @leadRole, false),
                (@responder, @aidRole, false),
                (@bystander, @aidRole, false),
                (@bystander, @guardRole, false);

            INSERT INTO location (id, tenant_id, name)
                VALUES (@location, @tenant, 'Hoofdgebouw');

            INSERT INTO event_type (id, tenant_id, name, active)
                VALUES (@eventType, @tenant, 'Zondagdienst', true);

            INSERT INTO event (id, tenant_id, event_type_id, location_id, title,
                               start, "end", status, source)
                VALUES (@event, @tenant, @eventType, @location, 'Zondagdienst',
                        now() + interval '1 hour', now() + interval '4 hours',
                        'Scheduled', 'Manual');

            INSERT INTO shift (id, event_id, team_role_id, start, "end", required_count) VALUES
                (@leadShift, @event, @leadRole,
                 now() + interval '1 hour', now() + interval '4 hours', 1),
                (@aidShift, @event, @aidRole,
                 now() + interval '1 hour', now() + interval '4 hours', 1);

            INSERT INTO assignment (id, shift_id, person_id, status, assigned_at) VALUES
                (@leadAssignment, @leadShift, @responder, 'Assigned', now());

            INSERT INTO assignment (id, shift_id, person_id, status, assigned_at,
                                    withdrawn_at, withdrawal_reason)
                VALUES (@aidAssignment, @aidShift, @withdrawn, 'Withdrawn', now(),
                        now(), 'Ziek');

            INSERT INTO swap_request (id, shift_id, assignment_id, requested_by_person_id,
                                      kind, status, expires_at)
                VALUES (@swap, @aidShift, @aidAssignment, @withdrawn,
                        'OpenCall', 'Open', now() + interval '4 hours');

            INSERT INTO event (id, tenant_id, event_type_id, location_id, title,
                               start, "end", status, source)
                VALUES (@laterEvent, @tenant, @eventType, @location, 'Vrijdagavond',
                        now() + interval '2 days', now() + interval '2 days 3 hours',
                        'Scheduled', 'Manual');

            INSERT INTO shift (id, event_id, team_role_id, start, "end", required_count) VALUES
                (@tomorrowShift, @laterEvent, @aidRole,
                 now() + interval '2 days', now() + interval '2 days 3 hours', 1),
                (@certifiedShift, @laterEvent, @guardRole,
                 now() + interval '2 days', now() + interval '2 days 3 hours', 1);

            INSERT INTO advisory (id, tenant_id, title, text, valid_from, valid_until, priority)
                VALUES (@advisory, @tenant, 'Aandachtspunt vandaag',
                        'De lift is buiten dienst.',
                        current_date - 1, current_date + 1, 10);

            INSERT INTO contact (id, tenant_id, name, function, phone,
                                 is_emergency_number, sort_order) VALUES
                (@contact, @tenant, 'Meldkamer', 'Alarmnummer', '112', true, 0);
            """, owner);

        cmd.Parameters.AddWithValue("tenant", Tenant);
        cmd.Parameters.AddWithValue("responder", Responder);
        cmd.Parameters.AddWithValue("responderEmail", ResponderEmail);
        cmd.Parameters.AddWithValue("bystander", Bystander);
        cmd.Parameters.AddWithValue("bystanderEmail", BystanderEmail);
        cmd.Parameters.AddWithValue("withdrawn", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"));
        cmd.Parameters.AddWithValue("leadRole", Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"));
        cmd.Parameters.AddWithValue("aidRole", Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"));
        cmd.Parameters.AddWithValue("location", Guid.Parse("dddddddd-0000-0000-0000-000000000001"));
        cmd.Parameters.AddWithValue("eventType", Guid.Parse("dddddddd-0000-0000-0000-000000000002"));
        cmd.Parameters.AddWithValue("event", Guid.Parse("eeeeeeee-0000-0000-0000-000000000001"));
        cmd.Parameters.AddWithValue("leadShift", Guid.Parse("cccccccc-0000-0000-0000-000000000001"));
        cmd.Parameters.AddWithValue("aidShift", FirstAidShift);
        cmd.Parameters.AddWithValue("laterEvent", Guid.Parse("eeeeeeee-0000-0000-0000-000000000002"));
        cmd.Parameters.AddWithValue("tomorrowShift", TomorrowShift);
        cmd.Parameters.AddWithValue("certifiedShift", CertifiedShift);
        cmd.Parameters.AddWithValue("guardRole", Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"));
        cmd.Parameters.AddWithValue("certificate", Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"));
        cmd.Parameters.AddWithValue("leadAssignment", Guid.Parse("ffffffff-0000-0000-0000-000000000001"));
        cmd.Parameters.AddWithValue("aidAssignment", Guid.Parse("ffffffff-0000-0000-0000-000000000002"));
        cmd.Parameters.AddWithValue("swap", Guid.Parse("ffffffff-0000-0000-0000-000000000003"));
        cmd.Parameters.AddWithValue("advisory", Guid.Parse("ffffffff-0000-0000-0000-000000000004"));
        cmd.Parameters.AddWithValue("contact", Guid.Parse("ffffffff-0000-0000-0000-000000000005"));

        await cmd.ExecuteNonQueryAsync();
    }
}

/// <summary>Keeps the last message per address so a test can read the code out of it.</summary>
public sealed class CapturingEmailSender : IEmailSender
{
    private readonly Dictionary<string, string> _sent = new();

    public Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        lock (_sent) _sent[to] = body;
        return Task.CompletedTask;
    }

    public string? BodyFor(string address)
    {
        lock (_sent) return _sent.TryGetValue(address, out var body) ? body : null;
    }
}

[CollectionDefinition(nameof(ParticipantApiCollection))]
public sealed class ParticipantApiCollection : ICollectionFixture<ParticipantApiFixture>;
