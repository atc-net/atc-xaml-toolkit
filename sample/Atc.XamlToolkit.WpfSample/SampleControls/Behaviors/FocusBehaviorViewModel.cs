namespace Atc.XamlToolkit.WpfSample.SampleControls.Behaviors;

public partial class FocusBehaviorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string initialFocusText = "I have initial focus!";

    [ObservableProperty]
    private bool isTextBoxFocused;

    [ObservableProperty]
    private bool isCombinedFocused;

    [ObservableProperty]
    private object? focusTrigger;

    [RelayCommand]
    private void SetFocus()
    {
        IsTextBoxFocused = true;
    }

    [RelayCommand]
    private void TriggerFocus()
    {
        // Change the FocusTrigger value to trigger focus
        FocusTrigger = DateTime.Now;
    }
}