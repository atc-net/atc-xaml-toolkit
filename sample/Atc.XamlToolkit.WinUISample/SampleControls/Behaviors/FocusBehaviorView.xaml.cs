namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

public sealed partial class FocusBehaviorView : UserControl
{
    public FocusBehaviorView()
    {
        InitializeComponent();
        ViewModel = new FocusBehaviorViewModel();
    }

    public FocusBehaviorViewModel ViewModel { get; }
}