namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class SetterAccessControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private)]
    private int total;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty]
    private int step;

    public void Increase()
        => Total += 1;
}