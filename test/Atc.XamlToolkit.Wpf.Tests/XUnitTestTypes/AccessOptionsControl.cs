namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class AccessOptionsControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(AccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private, DefaultValue = 1d)]
    private double internalZoom;

    public double ZoomIn()
    {
        InternalZoom *= 2;
        return InternalZoom;
    }
}