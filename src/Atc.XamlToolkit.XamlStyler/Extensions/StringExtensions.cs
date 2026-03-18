namespace Atc.XamlToolkit.XamlStyler.Extensions;

internal static class StringExtensions
{
    private static readonly Dictionary<string, string> EscapedCharacters = new(StringComparer.Ordinal)
    {
        ["&"] = "&amp;",
        ["<"] = "&lt;",
        [">"] = "&gt;",
        ["\""] = "&quot;",
    };

    public static string ToXmlEncodedString(
        this string input,
        ISet<char>? unescapedCharacters = null,
        bool ignoreCarrier = false)
    {
        var buffer = new StringBuilder(input);

        foreach (var escapedCharacter in EscapedCharacters)
        {
            if (unescapedCharacters?.Contains(escapedCharacter.Key[0]) != false)
            {
                continue;
            }

            buffer.Replace(escapedCharacter.Key, escapedCharacter.Value);
        }

        if (!ignoreCarrier)
        {
            buffer.Replace("\n", "&#10;");
        }

        return buffer.ToString();
    }

    public static IEnumerable<string> GetLines(this string source)
    {
        using var reader = new StringReader(source);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            yield return line;
        }
    }

    public static IList<string> ToList(this string source)
        => !string.IsNullOrEmpty(source)
            ? source.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList()
            : new List<string>();

    public static IList<NameSelector> ToNameSelectorList(this string source)
        => !string.IsNullOrEmpty(source)
            ? source.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => new NameSelector(s.Trim()))
                .ToList()
            : new List<NameSelector>();

    /// <summary>
    /// Replaces all occurrences of a string with another string (ordinal comparison).
    /// Works on both netstandard2.0 and net10.0.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="oldValue">The string to find.</param>
    /// <param name="newValue">The replacement string.</param>
    /// <returns>The string with replacements applied.</returns>
    [SuppressMessage("Performance", "CA1307:Specify StringComparison for clarity", Justification = "Cross-target helper; Replace(string, string, StringComparison) not available on netstandard2.0.")]
    internal static string ReplaceOrdinal(
        this string source,
        string oldValue,
        string newValue) =>
        source.Replace(oldValue, newValue);

    /// <summary>
    /// Checks if a string contains the specified character using ordinal comparison.
    /// Works on both netstandard2.0 and net10.0.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The character to find.</param>
    /// <returns>True if the character was found.</returns>
    [SuppressMessage("Performance", "CA1307:Specify StringComparison for clarity", Justification = "Cross-target helper; char overload with StringComparison not available on netstandard2.0.")]
    [SuppressMessage("Globalization", "CA2249:Consider using String.Contains instead of String.IndexOf", Justification = "Cross-target helper; Contains(char, StringComparison) not available on netstandard2.0.")]
    internal static bool ContainsOrdinal(
        this string source,
        char value) =>
        source.IndexOf(value) >= 0;

    /// <summary>
    /// Checks if a string contains the specified substring using ordinal comparison.
    /// Works on both netstandard2.0 and net10.0.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="value">The substring to find.</param>
    /// <returns>True if the substring was found.</returns>
    [SuppressMessage("Globalization", "CA2249:Consider using String.Contains instead of String.IndexOf", Justification = "Cross-target helper; Contains(string, StringComparison) not available on netstandard2.0.")]
    internal static bool ContainsOrdinal(
        this string source,
        string value) =>
        source.IndexOf(value, StringComparison.Ordinal) >= 0;
}