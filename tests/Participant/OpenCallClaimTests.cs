using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BccSafety.Api.Contracts;
using Xunit;

namespace BccSafety.Tests.Participant;

/// <summary>
/// Taking an open spot. Two rules are worth holding still.
///
/// "Whoever responds first gets it" has to survive two people tapping at
/// the same moment, or the day overview starts disagreeing with reality —
/// and an overview that disagrees with reality is worse than none.
///
/// A missing or expired certificate is the one thing in this application
/// that blocks outright. Everything else warns and sorts. There is no
/// override anywhere, on purpose, so this check has to hold on its own.
/// </summary>
[Collection(nameof(ParticipantApiCollection))]
public sealed class OpenCallClaimTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ParticipantApiFixture _api;

    public OpenCallClaimTests(ParticipantApiFixture api) => _api = api;

    [Fact]
    public async Task Claiming_a_spot_puts_you_on_it_and_a_second_claim_is_refused()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.BystanderEmail);
        var shift = ParticipantApiFixture.TomorrowShift;

        var claimed = await client.PostAsync($"/api/shifts/{shift}/claim", null);
        Assert.Equal(HttpStatusCode.OK, claimed.StatusCode);

        var mine = await client.GetFromJsonAsync<List<MyShift>>("/api/my/shifts", Json);
        Assert.Contains(mine!, s => s.ShiftId == shift);

        var again = await client.PostAsync($"/api/shifts/{shift}/claim", null);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("already_assigned", await ReasonAsync(again));
    }

    [Fact]
    public async Task Without_the_certificate_the_role_is_closed_and_not_even_offered()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.BystanderEmail);
        var shift = ParticipantApiFixture.CertifiedShift;

        // Not offered: a shift that would be refused on submit is worse
        // than one that was never shown.
        var open = await client.GetFromJsonAsync<List<OpenCall>>("/api/open-calls", Json);
        Assert.DoesNotContain(open!, call => call.ShiftId == shift);

        // And refused even when asked for directly.
        var claimed = await client.PostAsync($"/api/shifts/{shift}/claim", null);
        Assert.Equal(HttpStatusCode.Conflict, claimed.StatusCode);
        Assert.Equal("not_qualified", await ReasonAsync(claimed));
    }

    [Fact]
    public async Task An_open_call_carries_the_reason_it_opened()
    {
        var client = await _api.SignedInClientAsync(ParticipantApiFixture.BystanderEmail);

        var open = await client.GetFromJsonAsync<List<OpenCall>>("/api/open-calls", Json);

        var today = Assert.Single(
            open!, call => call.ShiftId == ParticipantApiFixture.FirstAidShift);
        Assert.Equal(OpenSpotReason.Withdrawn, today.Reason);
        Assert.Equal("Bas", today.WithdrawnByFirstName);
    }

    private static async Task<string?> ReasonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.TryGetProperty("reason", out var reason) ? reason.GetString() : null;
    }
}
