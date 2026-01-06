namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

public sealed partial class EventToCommandBehaviorView
{
    public EventToCommandBehaviorView()
    {
        InitializeComponent();
    }

    public EventToCommandBehaviorViewModel ViewModel
        => (EventToCommandBehaviorViewModel)DataContext;
}