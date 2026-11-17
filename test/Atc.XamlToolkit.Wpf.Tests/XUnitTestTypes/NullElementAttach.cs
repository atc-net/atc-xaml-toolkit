namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class NullElementAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty]
    private static double lenient;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(NullElementHandling = Atc.XamlToolkit.NullElementHandling.ThrowArgumentNullException)]
    private static double strict;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(NullElementHandling = Atc.XamlToolkit.NullElementHandling.ThrowArgumentNullException)]
    private static bool strictFlag;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(NullElementHandling = Atc.XamlToolkit.NullElementHandling.None)]
    private static double plain;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(
        ElementParameterName = "obj",
        NullElementHandling = Atc.XamlToolkit.NullElementHandling.ThrowArgumentNullException)]
    private static double named;
}