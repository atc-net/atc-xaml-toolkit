namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class ConsumerFindingsControl : System.Windows.Controls.UserControl
{
    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(
        GenerateRaiseMethod = true,
        RaiseMethodAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Internal)]
    private static readonly System.Windows.RoutedEvent opened;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        IsReadOnly = true,
        SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Public)]
    private bool isDragging;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(PropertyName = "State")]
    private int stateValue;

    public int NextState()
    {
        var state = State + 1;
        State = state;
        return state;
    }
}