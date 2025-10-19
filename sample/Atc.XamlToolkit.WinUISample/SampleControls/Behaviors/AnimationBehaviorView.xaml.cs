namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

public sealed partial class AnimationBehaviorView : UserControl
{
    public AnimationBehaviorView()
    {
        InitializeComponent();
        ViewModel = new AnimationBehaviorViewModel();
    }

    public AnimationBehaviorViewModel ViewModel { get; }
}