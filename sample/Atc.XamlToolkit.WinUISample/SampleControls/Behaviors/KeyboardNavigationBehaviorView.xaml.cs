namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

public sealed partial class KeyboardNavigationBehaviorView : UserControl
{
    public KeyboardNavigationBehaviorView()
    {
        InitializeComponent();
        ViewModel = new KeyboardNavigationBehaviorViewModel();
    }

    public KeyboardNavigationBehaviorViewModel ViewModel { get; }
}