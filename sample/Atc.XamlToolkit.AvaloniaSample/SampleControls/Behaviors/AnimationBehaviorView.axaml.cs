namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

public partial class AnimationBehaviorView : UserControl
{
    public AnimationBehaviorView()
    {
        InitializeComponent();
        DataContext = new AnimationBehaviorViewModel();
    }
}