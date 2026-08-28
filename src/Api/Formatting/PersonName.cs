namespace BccSafety.Api.Formatting;

/// <summary>
/// Dutch names carry a separate prefix ("de", "van der", "el"), which
/// sorts and abbreviates differently from the rest of the surname. The
/// avatar in the day overview shows "NA" for Nadia el Amrani, not "NE".
/// </summary>
public static class PersonName
{
    public static string Display(string firstName, string? prefix, string lastName)
        => string.IsNullOrWhiteSpace(prefix)
            ? $"{firstName} {lastName}"
            : $"{firstName} {prefix} {lastName}";

    public static string Initials(string firstName, string lastName)
    {
        var first = firstName.Trim();
        var last = lastName.Trim();
        var initials = string.Concat(
            first.Length > 0 ? char.ToUpperInvariant(first[0]).ToString() : string.Empty,
            last.Length > 0 ? char.ToUpperInvariant(last[0]).ToString() : string.Empty);
        return initials.Length > 0 ? initials : "?";
    }
}
