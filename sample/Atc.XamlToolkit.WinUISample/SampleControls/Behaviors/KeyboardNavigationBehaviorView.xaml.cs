namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for KeyboardNavigationBehaviorView.xaml.
/// </summary>
public sealed partial class KeyboardNavigationBehaviorView : UserControl
{
    public KeyboardNavigationBehaviorView()
    {
        InitializeComponent();
        ViewModel = new KeyboardNavigationBehaviorViewModel();
    }

    public KeyboardNavigationBehaviorViewModel ViewModel { get; }
}