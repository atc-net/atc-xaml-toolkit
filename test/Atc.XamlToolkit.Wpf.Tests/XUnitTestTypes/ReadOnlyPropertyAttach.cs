namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class ReadOnlyPropertyAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(IsReadOnly = true)]
    private static bool isFiltered;

    public static void MarkAsFiltered(System.Windows.DependencyObject element)
        => SetIsFiltered(element, value: true);
}