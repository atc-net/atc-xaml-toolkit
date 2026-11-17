namespace Atc.XamlToolkit.XamlStyler.Services;

internal sealed class XmlEscapingService
{
    private readonly Regex htmlReservedCharRegex = new(@"&(?<entity>[\d\D][^;]{1,7});", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex htmlReservedCharRestoreRegex = new(@"__amp__(?<entity>[\d\D][^;]{1,7})__scln__", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex xmlnsAliasesBypassRegex = new(@"xmlns(?<colonprefix>:(?<prefix>[\w.\-]+))\s*=\s*(?<quote>[""'])(?<ns>[^""']+)\k<quote>", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex xmlnsAliasesBypassRestoreRegex = new(@"xmlns:(?<prefix>[\w.\-]+)\s*=\s*""\[(\k<prefix>)\](?<ns>[^""]+)""", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));

    public string EscapeDocument(string source)
    {
        source = htmlReservedCharRegex.Replace(source, @"__amp__${entity}__scln__");
        var original = source;
        source = xmlnsAliasesBypassRegex.Replace(source, match => SelectiveXmlReplacer(original, match));
        return source;
    }

    public string UnescapeDocument(string source)
    {
        source = htmlReservedCharRestoreRegex.Replace(source, @"&${entity};");
        source = xmlnsAliasesBypassRestoreRegex.Replace(source, @"xmlns:${prefix}=""${ns}""");
        return source;
    }

    internal string RestoreXmlnsAliasesBypass(string source) =>
        xmlnsAliasesBypassRestoreRegex.Replace(source, @"xmlns:${prefix}=""${ns}""");

    // True when the position is inside a comment or inside the quotes of an attribute
    // value. Text there that looks like an xmlns declaration is not one.
    private static bool IsInsideCommentOrAttributeValue(
        string source,
        int index)
    {
        // A raw '<' cannot occur inside an attribute value, so the last one starts the tag
        // or comment the position belongs to.
        var start = source.LastIndexOf('<', index);
        if (start < 0)
        {
            return false;
        }

        if (string.CompareOrdinal(source, start, "<!--", 0, 4) == 0)
        {
            return source.IndexOf("-->", start, index - start, StringComparison.Ordinal) < 0;
        }

        var openQuote = '\0';
        for (var i = start; i < index; i++)
        {
            var c = source[i];
            if (openQuote == '\0')
            {
                if (c is '"' or '\'')
                {
                    openQuote = c;
                }
            }
            else if (c == openQuote)
            {
                openQuote = '\0';
            }
        }

        return openQuote != '\0';
    }

    private string SelectiveXmlReplacer(
        string source,
        Match match)
    {
        if (IsInsideCommentOrAttributeValue(source, match.Index))
        {
            return match.Value;
        }

        // Allow partial xmlns definitions in comments to not break anything.
        // See https://github.com/Xavalon/XamlStyler/issues/426
        if (match.Captures.Count == 1
            && match.Captures[0].Value.ContainsOrdinal("-->"))
        {
            return match.Captures[0].Value;
        }

        return xmlnsAliasesBypassRegex.Replace(match.Value, @"xmlns${colonprefix}=""[${prefix}]${ns}""");
    }
}