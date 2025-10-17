namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for FocusBehaviorView.axaml.
/// </summary>
public partial class FocusBehaviorView : UserControl
{
    public FocusBehaviorView()
    {
        InitializeComponent();
        DataContext = new FocusBehaviorViewModel();
    }
}