namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class EventToCommandBehaviorView : UserControl
{
    public EventToCommandBehaviorView()
    {
        InitializeComponent();
        DataContext = new EventToCommandBehaviorViewModel();
    }
}