namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class CoerceHookControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GenerateCoerceHook = true)]
    private double maximum;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GenerateCoerceHook = true)]
    private string? title;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(GenerateCoerceHook = true, CoerceValueCallback = nameof(DoubleIt))]
    private int steps;

    private static object DoubleIt(
        System.Windows.DependencyObject d,
        object baseValue)
        => d is CoerceHookControl ? (int)baseValue * 2 : baseValue;

    private partial double CoerceMaximum(double value)
        => value < 0 ? 0 : value;

    private partial string? CoerceTitle(string? value)
        => value?.Trim();

    private partial int CoerceSteps(int value)
        => value > 10 ? 10 : value;
}