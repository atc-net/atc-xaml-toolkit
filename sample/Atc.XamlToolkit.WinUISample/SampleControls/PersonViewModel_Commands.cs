// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.WinUISample.SampleControls;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "OK")]
public partial class PersonViewModel
{
    private CancellationTokenSource? cancellationTokenSource;

    [RelayCommand]
    public async Task ShowData()
    {
        var sb = new StringBuilder();
        sb.Append("FirstName: ");
        sb.AppendLine(FirstName);
        sb.Append("LastName: ");
        sb.AppendLine(LastName);
        sb.Append("Age: ");
        sb.AppendLine(Age.ToString(GlobalizationConstants.EnglishCultureInfo));
        sb.Append("Email: ");
        sb.AppendLine(Email);
        sb.Append("TheProperty: ");
        sb.AppendLine(TheProperty);

        var dialog = new ContentDialog
        {
            Title = "Show-Data",
            Content = sb.ToString(),
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow?.Content.XamlRoot,
        };

        await dialog.ShowAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    public async Task SaveHandler()
    {
        var dialog = new ContentDialog
        {
            Title = "Save-Data",
            Content = "Hello from SaveHandler method",
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow?.Content.XamlRoot,
        };

        await dialog.ShowAsync();
    }

    public bool CanSaveHandler()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            return false;
        }

        if (Age is < 18 or > 120)
        {
            return false;
        }

        if (Email is not null && !Email.IsEmailAddress())
        {
            return false;
        }

        return true;
    }

    public bool CanStartLongRunningWork() => cancellationTokenSource is null;

    [RelayCommand(CanExecute = nameof(CanStartLongRunningWork))]
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

            var completedDialog = new ContentDialog
            {
                Title = "Information",
                Content = "Long running work completed",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };
            await completedDialog.ShowAsync();
        }
        catch (OperationCanceledException)
        {
            var cancelledDialog = new ContentDialog
            {
                Title = "Information",
                Content = "Long running work cancelled",
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };
            await cancelledDialog.ShowAsync();
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