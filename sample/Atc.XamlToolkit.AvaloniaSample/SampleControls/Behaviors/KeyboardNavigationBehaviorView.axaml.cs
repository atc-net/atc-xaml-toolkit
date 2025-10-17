namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for KeyboardNavigationBehaviorView.axaml.
/// </summary>
public partial class KeyboardNavigationBehaviorView : UserControl
{
    public KeyboardNavigationBehaviorView()
    {
        InitializeComponent();
        DataContext = new KeyboardNavigationBehaviorViewModel();
    }
}