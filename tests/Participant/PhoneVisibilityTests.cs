using BccSafety.Api.Contracts;
using BccSafety.Api.Security;
using Xunit;

namespace BccSafety.Tests.Participant;

/// <summary>
/// The day overview shows names and team roles to every participant of
/// the tenant, but phone numbers only to the people who are actually
/// working that event, and only around the time they work it. That rule
/// is the one thing on this screen that is a privacy decision rather
/// than a layout decision, so it is pinned down here.
/// </summary>
public sealed class PhoneVisibilityTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 6, 9, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2026, 9, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Someone_without_a_shift_never_sees_numbers()
    {
        var state = PhoneVisibility.Evaluate(
            callerIsScheduled: false, Start, End, Start.AddMinutes(30));

        Assert.Equal(PhoneVisibilityState.NotScheduled, state);
    }

    [Fact]
    public void Someone_on_the_shift_sees_numbers_while_it_runs()
    {
        var state = PhoneVisibility.Evaluate(
            callerIsScheduled: true, Start, End, Start.AddMinutes(30));

        Assert.Equal(PhoneVisibilityState.Visible, state);
    }

    [Fact]
    public void Numbers_appear_before_the_shift_starts_but_not_days_ahead()
    {
        Assert.Equal(
            PhoneVisibilityState.Visible,
            PhoneVisibility.Evaluate(true, Start, End, Start - PhoneVisibility.Lead));

        Assert.Equal(
            PhoneVisibilityState.OutsideShiftWindow,
            PhoneVisibility.Evaluate(true, Start, End, Start.AddDays(-1)));
    }

    [Fact]
    public void Numbers_survive_the_wrap_up_and_then_disappear()
    {
        Assert.Equal(
            PhoneVisibilityState.Visible,
            PhoneVisibility.Evaluate(true, Start, End, End + PhoneVisibility.Grace));

        Assert.Equal(
            PhoneVisibilityState.OutsideShiftWindow,
            PhoneVisibility.Evaluate(true, Start, End, End + PhoneVisibility.Grace.Add(TimeSpan.FromMinutes(1))));
    }
}
