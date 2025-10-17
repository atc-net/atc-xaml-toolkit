namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

/// <summary>
/// Interaction logic for AnimationBehaviorView.xaml.
/// </summary>
public sealed partial class AnimationBehaviorView : UserControl
{
    public AnimationBehaviorView()
    {
        InitializeComponent();
        ViewModel = new AnimationBehaviorViewModel();
    }

    public AnimationBehaviorViewModel ViewModel { get; }
}