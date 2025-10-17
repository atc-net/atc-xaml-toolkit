namespace Atc.XamlToolkit.WinUISample.SampleControls.Behaviors;

/// <summary>
/// ViewModel for the AnimationBehavior sample.
/// </summary>
public partial class AnimationBehaviorViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? fadeInTrigger;

    [ObservableProperty]
    private object? fadeOutTrigger;

    [ObservableProperty]
    private object? slideLeftTrigger;

    [ObservableProperty]
    private object? slideRightTrigger;

    [ObservableProperty]
    private object? slideTrigger;

    [ObservableProperty]
    private object? slideBottomTrigger;

    [ObservableProperty]
    private object? scaleInTrigger;

    [ObservableProperty]
    private object? scaleOutTrigger;

    [RelayCommand]
    private void TriggerFadeIn()
        => FadeInTrigger = new object();

    [RelayCommand]
    private void TriggerFadeOut()
        => FadeOutTrigger = new object();

    [RelayCommand]
    private void TriggerSlideLeft()
        => SlideLeftTrigger = new object();

    [RelayCommand]
    private void TriggerSlideRight()
        => SlideRightTrigger = new object();

    [RelayCommand]
    private void TriggerSlideTop()
        => SlideTrigger = new object();

    [RelayCommand]
    private void TriggerSlideBottom()
        => SlideBottomTrigger = new object();

    [RelayCommand]
    private void TriggerScaleIn()
        => ScaleInTrigger = new object();

    [RelayCommand]
    private void TriggerScaleOut()
        => ScaleOutTrigger = new object();
}