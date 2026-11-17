namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class PartialHooksAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(GeneratePartialHooks = true)]
    private static bool isHighlighted;

    public static System.Collections.ObjectModel.Collection<string> Calls { get; } = [];

    static partial void OnIsHighlightedChanged(
        System.Windows.DependencyObject d,
        bool oldValue,
        bool newValue)
        => Calls.Add($"{d.GetType().Name} {oldValue}->{newValue}");
}