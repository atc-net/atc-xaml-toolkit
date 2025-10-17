namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for AnimationBehaviorView.axaml.
/// </summary>
public partial class AnimationBehaviorView : UserControl
{
    public AnimationBehaviorView()
    {
        InitializeComponent();
        DataContext = new AnimationBehaviorViewModel();
    }
}