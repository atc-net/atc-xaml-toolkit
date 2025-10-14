// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.WinUISample.SampleControls.Mvvm.GeneralAttributes.Person;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "OK")]
public partial class PersonViewModel
{
    private CancellationTokenSource? cancellationTokenSource;

    [RelayCommand]
    [SupportedOSPlatform("windows10.0.17763.0")]
    public async Task ShowData()
    {
        var sb = new StringBuilder();
        sb.Append("FirstName: ");
        sb.AppendLine(FirstName);
        sb.Append("LastName: ");
        sb.AppendLine(LastName);
        sb.Append("Age: ");
        sb.AppendLine(Age?.ToString(GlobalizationConstants.EnglishCultureInfo));
        sb.Append("Email: ");
        sb.AppendLine(Email);
        sb.Append("TheProperty: ");
        sb.AppendLine(TheProperty);

        var dialog = new ContentDialog
        {
            Title = "Show-Data",
            Content = sb.ToString(),
            CloseButtonText = "OK",
            XamlRoot = App.GetMainWindowXamlRoot(),
        };

        await dialog.ShowAsync();
    }

    public bool CanSaveHandler()
    {
        // Use validation errors instead of manual checks
        return !HasErrors;
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    [SupportedOSPlatform("windows10.0.17763.0")]
    public async Task SaveHandler()
    {
        var dialog = new ContentDialog
        {
            Title = "Save-Data",
            Content = "Hello from SaveHandler method",
            CloseButtonText = "OK",
            XamlRoot = App.GetMainWindowXamlRoot(),
        };

        await dialog.ShowAsync();
    }

    public bool CanStartLongRunningWork() => cancellationTokenSource is null;

    [RelayCommand(CanExecute = nameof(CanStartLongRunningWork))]
    [SupportedOSPlatform("windows10.0.17763.0")]
    public async Task StartLongRunningWorkAsync()
    {
        if (cancellationTokenSource is not null)
        {
            return; // Already running
        }

        cancellationTokenSource = new CancellationTokenSource();

        // Immediately update command CanExecute states after starting (depends on cancellationTokenSource)
        StartLongRunningWorkAsyncCommand.RaiseCanExecuteChanged();
        CancelLongRunningWorkCommand.RaiseCanExecuteChanged();

        try
        {
            for (var i = 1; i <= 10; i++)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                await Task.Delay(500, cancellationTokenSource.Token).ConfigureAwait(true);
            }

            var dialog = new ContentDialog
            {
                Title = "Information",
                Content = "Long running work completed",
                CloseButtonText = "OK",
                XamlRoot = App.GetMainWindowXamlRoot(),
            };

            await dialog.ShowAsync();
        }
        catch (OperationCanceledException)
        {
            var dialog = new ContentDialog
            {
                Title = "Information",
                Content = "Long running work cancelled",
                CloseButtonText = "OK",
                XamlRoot = App.GetMainWindowXamlRoot(),
            };

            await dialog.ShowAsync();
        }
        finally
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            StartLongRunningWorkAsyncCommand.RaiseCanExecuteChanged();
            CancelLongRunningWorkCommand.RaiseCanExecuteChanged();
        }
    }

    public bool CanCancelLongRunningWork() => cancellationTokenSource is not null;

    [RelayCommand(CanExecute = nameof(CanCancelLongRunningWork))]
    public void CancelLongRunningWork()
    {
        cancellationTokenSource?.Cancel();
    }
}