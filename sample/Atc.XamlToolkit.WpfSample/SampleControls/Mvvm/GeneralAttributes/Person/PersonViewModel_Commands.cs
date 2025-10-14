// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.WpfSample.SampleControls.Mvvm.GeneralAttributes.Person;

[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "OK")]
public partial class PersonViewModel
{
    private CancellationTokenSource? cancellationTokenSource;

    [RelayCommand]
    public void ShowData()
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

        MessageBox.Show(sb.ToString(), "Show-Data");
    }

    public bool CanSaveHandler()
    {
        // Use validation errors instead of manual checks
        return !HasErrors;
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    public void SaveHandler()
    {
        MessageBox.Show("Hello from SaveHandler method", "Save-Data");
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

            MessageBox.Show("Long running work completed", "Information");
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("Long running work cancelled", "Information");
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