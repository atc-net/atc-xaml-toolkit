namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class FocusBehaviorView : UserControl
{
    public FocusBehaviorView()
    {
        InitializeComponent();
        DataContext = new FocusBehaviorViewModel();
    }
}