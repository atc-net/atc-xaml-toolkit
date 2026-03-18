namespace Atc.XamlToolkit.XamlStyler.Options;

public sealed class XamlLanguageOptions
{
    public bool IsFormatable { get; set; }

    public ISet<char> UnescapedAttributeCharacters { get; } = new HashSet<char>();
}