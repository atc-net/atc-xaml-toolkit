namespace Atc.XamlToolkit.XamlStyler.Model;

[Flags]
internal enum ContentTypes
{
    None = 0,
    SingleLineTextOnly = 1,
    MultiLineTextOnly = 2,
    Mixed = 4,
}