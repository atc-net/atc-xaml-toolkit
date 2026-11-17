namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class CoerceHookAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(GenerateCoerceHook = true)]
    private static double zoomFactor;

    private static partial double CoerceZoomFactor(
        System.Windows.DependencyObject d,
        double value)
        => d is System.Windows.Controls.Button && value > 4 ? 4 : value;
}