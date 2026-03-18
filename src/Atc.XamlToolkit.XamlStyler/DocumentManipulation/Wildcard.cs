namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

/// <summary>
/// Represents a DOS-style wildcard pattern, compiled to a regex.
/// </summary>
internal sealed class Wildcard : Regex
{
    public Wildcard(string pattern)
        : base(WildcardToRegex(pattern), RegexOptions.Compiled)
    {
    }

    private static string WildcardToRegex(string pattern) =>
        "^" + Regex.Escape(pattern).ReplaceOrdinal("\\*", ".*").ReplaceOrdinal("\\?", ".") + "$";
}