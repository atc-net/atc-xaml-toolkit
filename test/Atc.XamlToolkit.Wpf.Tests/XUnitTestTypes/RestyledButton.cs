namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.DefaultStyleKey(typeof(System.Windows.Controls.Button))]
public partial class RestyledButton : System.Windows.Controls.Primitives.ButtonBase
{
    public object ExposedDefaultStyleKey => DefaultStyleKey;
}