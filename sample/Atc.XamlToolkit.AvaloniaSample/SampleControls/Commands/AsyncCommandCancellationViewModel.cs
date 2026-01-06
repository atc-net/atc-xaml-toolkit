// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Commands;

[SuppressMessage("Style", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "OK")]
public sealed partial class AsyncCommandCancellationViewModel : ViewModelBase
{
    [ObservableProperty]
    private int progress;

    [ObservableProperty]
    private string statusMessage = "Ready";

    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string searchStatus = string.Empty;

    [ObservableProperty]
    private int downloadProgress;

    [ObservableProperty]
    private string downloadStatus = "Ready";

    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (!Set(ref searchQuery, value))
            {
                return;
            }

            // Cancel previous search and start new one
            if (SearchAsyncCommand.IsExecuting)
            {
                CancelSearchAsync();
            }

            _ = SearchAsyncCommand.ExecuteAsync(value);
        }
    }

    public ObservableCollection<string> SearchResults { get; } = [];

    // Cancel commands for XAML binding
    [RelayCommand(CanExecute = nameof(CanCancelTask))]
    private void CancelTask()
        => CancelStartLongRunningTaskAsync();

    [RelayCommand(CanExecute = nameof(CanCancelDownload))]
    private void CancelDownload()
        => CancelStartDownloadAsync();

    private bool CanCancelTask()
        => StartLongRunningTaskAsyncCommand.IsExecuting;

    private bool CanCancelDownload()
        => StartDownloadAsyncCommand.IsExecuting;

    [RelayCommand(SupportsCancellation = true)]
    private async Task StartLongRunningTaskAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = "Task running...";
            Progress = 0;

            // Notify cancel command that execution state changed
            CancelTaskCommand.RaiseCanExecuteChanged();

            for (var i = 0; i <= 100; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Simulate work
                await Task.Delay(50, cancellationToken).ConfigureAwait(true);

                Progress = i;
                StatusMessage = $"Processing... {i}%";
            }

            StatusMessage = "Task completed successfully!";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Task was cancelled.";
            Progress = 0;
        }
        finally
        {
            // Notify cancel command that execution state changed
            CancelTaskCommand.RaiseCanExecuteChanged();
        }
    }

    [RelayCommand(SupportsCancellation = true)]
    private async Task SearchAsync(
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            SearchResults.Clear();
            SearchStatus = string.Empty;
            return;
        }

        try
        {
            SearchStatus = $"Searching for '{query}'...";
            SearchResults.Clear();

            // Simulate API call delay
            await Task.Delay(1000, cancellationToken).ConfigureAwait(true);

            // Check again after delay
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate search results
            for (var i = 1; i <= 10; i++)
            {
                SearchResults.Add($"Result {i} for '{query}'");
            }

            SearchStatus = $"Found {SearchResults.Count} results for '{query}'";
        }
        catch (OperationCanceledException)
        {
            SearchStatus = $"Search for '{query}' was cancelled.";
            SearchResults.Clear();
        }
    }

    [RelayCommand(SupportsCancellation = true)]
    private async Task StartDownloadAsync(CancellationToken cancellationToken)
    {
        try
        {
            DownloadStatus = "Download started...";
            DownloadProgress = 0;

            // Notify cancel command that execution state changed
            CancelDownloadCommand.RaiseCanExecuteChanged();

            // Simulate download in chunks
            for (var i = 0; i <= 100; i += 5)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(100, cancellationToken).ConfigureAwait(true);

                DownloadProgress = i;
                DownloadStatus = $"Downloading... {i}%";
            }

            DownloadStatus = "Download completed!";
            DownloadProgress = 100;
        }
        catch (OperationCanceledException)
        {
            DownloadStatus = "Download was cancelled.";
            DownloadProgress = 0;
        }
        finally
        {
            // Notify cancel command that execution state changed
            CancelDownloadCommand.RaiseCanExecuteChanged();
        }
    }
}