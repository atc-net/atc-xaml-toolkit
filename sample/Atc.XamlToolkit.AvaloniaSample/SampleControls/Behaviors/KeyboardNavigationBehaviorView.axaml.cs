namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class KeyboardNavigationBehaviorView : UserControl
{
    public KeyboardNavigationBehaviorView()
    {
        InitializeComponent();
        DataContext = new KeyboardNavigationBehaviorViewModel();
    }
}