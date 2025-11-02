namespace Atc.XamlToolkit.WpfSample.SampleControls.Commands;

[SuppressMessage("", "S1172", Justification = "OK")]
public sealed partial class AsyncCommandCancellationAndIsBusyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string statusMessage = "Ready to start...";

    private static bool CanDoStuff() => true;

    [RelayCommand(
        CanExecute = nameof(CanDoStuff),
        AutoSetIsBusy = true,
        SupportsCancellation = true)]
    private async Task DoStuff(
        CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = "Starting first delay (5 seconds)...";

            await Task.Delay(5_000, cancellationToken).ConfigureAwait(true);

            StatusMessage = "First delay completed! Starting second delay (10 seconds)...";

            await Task.Delay(10_000, cancellationToken).ConfigureAwait(true);

            StatusMessage = "Both delays completed successfully!";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Task was cancelled!";
        }
    }
}