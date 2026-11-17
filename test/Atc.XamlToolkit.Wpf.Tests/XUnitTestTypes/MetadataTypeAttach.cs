namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class MetadataTypeAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(
        MetadataType = Atc.XamlToolkit.Controls.Attributes.DependencyPropertyMetadataType.UIPropertyMetadata,
        DefaultValue = 2d,
        TargetType = typeof(System.Windows.Controls.Panel),
        BrowsableForTargetType = true)]
    private static double spacing;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(TargetType = typeof(System.Windows.Controls.Panel))]
    private static double padding;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(AddOwnerOf = typeof(System.Windows.Documents.TextElement))]
    private static double fontSize;
}