namespace Atc.XamlToolkit.XamlStyler.Services;

internal sealed class XmlEscapingService
{
    private readonly Regex htmlReservedCharRegex = new(@"&(?<entity>[\d\D][^;]{1,7});", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex htmlReservedCharRestoreRegex = new(@"__amp__(?<entity>[\d\D][^;]{1,7})__scln__", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex xmlnsAliasesBypassRegex = new(@"xmlns(?<colonprefix>:(?<prefix>[^=]+))=""(?<ns>[^""]+)""", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));
    private readonly Regex xmlnsAliasesBypassRestoreRegex = new(@"xmlns:(?<prefix>[^=]+)=""\[(\k<prefix>)\](?<ns>[^""]+)""", RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(5));

    public string EscapeDocument(string source)
    {
        source = htmlReservedCharRegex.Replace(source, @"__amp__${entity}__scln__");
        source = xmlnsAliasesBypassRegex.Replace(source, SelectiveXmlReplacer);
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

    private string SelectiveXmlReplacer(Match match)
    {
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