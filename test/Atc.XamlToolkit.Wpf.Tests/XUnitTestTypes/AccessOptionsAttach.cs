namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class AccessOptionsAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(TargetType = typeof(System.Windows.UIElement))]
    private static System.Windows.CornerRadius checkCornerRadius;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(AccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Internal)]
    private static bool isFiltered;

    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(
        AccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private,
        GenerateClrAccessors = false)]
    private static string? state;

    public static string? ReadState(System.Windows.DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (string?)element.GetValue(StateProperty);
    }

    public static void WriteState(
        System.Windows.DependencyObject element,
        string? value)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(StateProperty, value);
    }
}