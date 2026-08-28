using BccSafety.Api.Formatting;
using Xunit;

namespace BccSafety.Tests.Participant;

/// <summary>
/// Dutch surnames carry a separate prefix, which abbreviates differently
/// from the rest of the name. The avatar in the day overview is two
/// letters and people recognise each other by them.
/// </summary>
public sealed class PersonNameTests
{
    [Theory]
    [InlineData("Marit", null, "Vermeer", "Marit Vermeer")]
    [InlineData("Erik", "de", "Groot", "Erik de Groot")]
    [InlineData("Nadia", "el", "Amrani", "Nadia el Amrani")]
    public void Display_puts_the_prefix_between_the_names(
        string first, string? prefix, string last, string expected)
    {
        Assert.Equal(expected, PersonName.Display(first, prefix, last));
    }

    [Theory]
    [InlineData("Marit", "Vermeer", "MV")]
    [InlineData("Erik", "Groot", "EG")]
    [InlineData("Nadia", "Amrani", "NA")]
    public void Initials_skip_the_prefix(string first, string last, string expected)
    {
        Assert.Equal(expected, PersonName.Initials(first, last));
    }

    [Fact]
    public void Initials_never_come_back_empty()
    {
        Assert.Equal("?", PersonName.Initials(" ", " "));
    }
}
