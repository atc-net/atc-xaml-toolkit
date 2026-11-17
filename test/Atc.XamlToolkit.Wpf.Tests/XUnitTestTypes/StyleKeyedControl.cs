namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.DefaultStyleKey]
public partial class StyleKeyedControl : System.Windows.Controls.Control
{
    public object ExposedDefaultStyleKey => DefaultStyleKey;
}