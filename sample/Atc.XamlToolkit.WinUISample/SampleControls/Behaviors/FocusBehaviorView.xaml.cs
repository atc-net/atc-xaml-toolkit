namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for FocusBehaviorView.xaml.
/// </summary>
public sealed partial class FocusBehaviorView : UserControl
{
    public FocusBehaviorView()
    {
        InitializeComponent();
        ViewModel = new FocusBehaviorViewModel();
    }

    public FocusBehaviorViewModel ViewModel { get; }
}