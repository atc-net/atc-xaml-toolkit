namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public static partial class ConsumerFindingsAttach
{
    [Atc.XamlToolkit.Controls.Attributes.AttachedProperty(PropertyName = "Dock")]
    private static System.Windows.Controls.Dock dockValue;

    public static System.Windows.Controls.Dock Opposite(
        System.Windows.Controls.Dock dock)
        => dock == System.Windows.Controls.Dock.Left
            ? System.Windows.Controls.Dock.Right
            : System.Windows.Controls.Dock.Left;
}