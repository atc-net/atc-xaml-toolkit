namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class ObjectDefaultValueAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(DefaultValue = "#")]
    private static object? buttonContent;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(DefaultValue = 3)]
    private static object? count;
}