// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.GeneralAttributes.Person;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "OK")]
public partial class PersonViewModel
{
    private CancellationTokenSource? cancellationTokenSource;

    [RelayCommand]
    public Task ShowData()
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

        var box = MessageBoxManager.GetMessageBoxStandard(
            "Show-Data",
            sb.ToString());

        return box.ShowAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    public Task SaveHandler()
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Save-Data",
            "Hello from SaveHandler method");

        return box.ShowAsync();
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

            var box = MessageBoxManager.GetMessageBoxStandard(
                "Information",
                "Long running work completed");
            await box.ShowAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Information",
                "Long running work cancelled");
            await box.ShowAsync().ConfigureAwait(true);
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