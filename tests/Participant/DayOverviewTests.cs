using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BccSafety.Api.Contracts;
using Xunit;

namespace BccSafety.Tests.Participant;

/// <summary>
/// The day overview over HTTP, against a real Postgres, as bcc_app with
/// the policies on. The queries are the part that only breaks at runtime:
/// EF translates them or it does not, and row-level security either lets
/// this tenant see its own rows or quietly returns nothing.
///
/// The rule these tests exist for is the one about phone numbers. Getting
/// it wrong does not break a screen — it hands every participant a list
/// of everyone's mobile number, and nothing about the page would look
/// out of place.
/// </summary>
[Collection(nameof(ParticipantApiCollection))]
public sealed class DayOverviewTests
{
    /// <summary>
    /// Matches how the API writes enums: by name. Reading them as
    /// numbers here would make the tests pass on a payload no client can
    /// actually use.
    /// </summary>
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ParticipantApiFixture _api;

    public DayOverviewTests(ParticipantApiFixture api) => _api = api;

    [Fact]
    public async Task Without_a_session_there_is_no_day_overview()
    {
        using var client = _api.CreateClient();

        var response = await client.GetAsync("/api/today");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task An_unknown_hostname_is_not_a_tenant()
    {
        using var client = _api.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Forwarded-Host");
        client.DefaultRequestHeaders.Add("X-Forwarded-Host", "nobody.example.test");

        var response = await client.GetAsync("/api/today");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Someone_on_duty_sees_the_roster_with_numbers_and_the_gap_that_opened()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.ResponderEmail);

        var today = await ReadAsync<TodayResponse>(client, "/api/today");

        Assert.NotNull(today);
        Assert.NotNull(today!.Event);
        Assert.Equal("Zondagdienst", today.Event!.Title);
        Assert.Equal("Hoofdgebouw", today.Event.LocationName);
        Assert.Equal(PhoneVisibilityState.Visible, today.Event.PhoneNumbers);

        // Their own shift, marked, and the advisory of the day.
        Assert.NotNull(today.Event.OwnShift);
        Assert.Equal("Hoofd-BHV", today.Event.OwnShift!.TeamRoleName);
        Assert.Contains(today.Event.Advisories, a => a.Title == "Aandachtspunt vandaag");

        var lead = today.Event.RoleGroups.Single(g => g.TeamRoleName == "Hoofd-BHV");
        var self = Assert.Single(lead.People);
        Assert.Equal("Marit Vermeer", self.Name);
        Assert.Equal("MV", self.Initials);
        Assert.True(self.IsSelf);
        Assert.Equal("0612340001", self.Phone);
        Assert.Null(lead.OpenSpots);

        // The first-aid spot is a row of its own, with the reason on it.
        var firstAid = today.Event.RoleGroups.Single(g => g.TeamRoleName == "EHBO");
        Assert.Empty(firstAid.People);
        Assert.NotNull(firstAid.OpenSpots);
        Assert.Equal(1, firstAid.OpenSpots!.Count);
        Assert.Equal(OpenSpotReason.Withdrawn, firstAid.OpenSpots.Reason);
        Assert.Equal("Bas", firstAid.OpenSpots.WithdrawnByFirstName);

        Assert.Equal(1, today.Event.FilledCount);
        Assert.Equal(2, today.Event.RequiredCount);
    }

    [Fact]
    public async Task Someone_not_on_duty_sees_who_is_on_but_no_numbers_at_all()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.BystanderEmail);

        var today = await ReadAsync<TodayResponse>(client, "/api/today");

        Assert.NotNull(today!.Event);
        Assert.Equal(PhoneVisibilityState.NotScheduled, today.Event!.PhoneNumbers);
        Assert.Null(today.Event.OwnShift);

        // Names and roles yes, so they can find today's lead responder.
        var lead = today.Event.RoleGroups.Single(g => g.TeamRoleName == "Hoofd-BHV");
        Assert.Equal("Marit Vermeer", lead.People.Single().Name);

        // Numbers no. Every one of them, not just the first.
        Assert.All(
            today.Event.RoleGroups.SelectMany(g => g.People),
            person => Assert.Null(person.Phone));
    }

    [Fact]
    public async Task The_contact_card_is_there_for_everyone()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.BystanderEmail);

        var contacts = await ReadAsync<List<ContactCardEntry>>(client, "/api/info/contacts");

        var emergency = Assert.Single(contacts!);
        Assert.Equal("Meldkamer", emergency.Name);
        Assert.True(emergency.IsEmergencyNumber);
    }

    /// <summary>
    /// Reads a response, and on failure puts the body in the assertion
    /// message. A test that only reports "500" costs an hour to act on.
    /// </summary>
    private static async Task<T?> ReadAsync<T>(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.IsSuccessStatusCode,
            $"GET {path} -> {(int)response.StatusCode}{Environment.NewLine}{body}");
        return await response.Content.ReadFromJsonAsync<T>(Json);
    }
}
