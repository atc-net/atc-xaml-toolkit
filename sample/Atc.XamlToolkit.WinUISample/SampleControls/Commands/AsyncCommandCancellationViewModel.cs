// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
// ReSharper disable GCSuppressFinalizeForTypeWithoutDestructor
// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.WinUISample.SampleControls.Commands;

[SuppressMessage("Style", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "OK")]
public sealed partial class AsyncCommandCancellationViewModel : ObservableObject, IDisposable
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
            if (Set(ref searchQuery, value))
            {
                // Cancel previous search and start new one
                SearchCommand.Cancel();
                _ = SearchCommand.ExecuteAsync(value);
            }
        }
    }

    public ObservableCollection<string> SearchResults { get; } = [];

    public IRelayCommandAsync StartLongRunningTaskCommand { get; }

    public IRelayCommand CancelTaskCommand { get; }

    public IRelayCommandAsync<string> SearchCommand { get; }

    public IRelayCommandAsync StartDownloadCommand { get; }

    public IRelayCommand CancelDownloadCommand { get; }

    public AsyncCommandCancellationViewModel()
    {
        // Long-running task with cancellation
        StartLongRunningTaskCommand = new RelayCommandAsync(LongRunningTaskAsync);

        CancelTaskCommand = new RelayCommand(
            () => StartLongRunningTaskCommand.Cancel(),
            () => StartLongRunningTaskCommand.IsExecuting);

        // Search with cancellation
        SearchCommand = new RelayCommandAsync<string>(SearchAsync);

        // Download with cancellation
        StartDownloadCommand = new RelayCommandAsync(DownloadFileAsync);

        CancelDownloadCommand = new RelayCommand(
            () => StartDownloadCommand.Cancel(),
            () => StartDownloadCommand.IsExecuting);
    }

    private async Task LongRunningTaskAsync(CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = "Task running...";
            Progress = 0;
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
            CancelTaskCommand.RaiseCanExecuteChanged();
        }
    }

    private async Task SearchAsync(string query, CancellationToken cancellationToken)
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
            for (int i = 1; i <= 10; i++)
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

    private async Task DownloadFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            DownloadStatus = "Download started...";
            DownloadProgress = 0;
            CancelDownloadCommand.RaiseCanExecuteChanged();

            // Simulate download in chunks
            for (int i = 0; i <= 100; i += 5)
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
            CancelDownloadCommand.RaiseCanExecuteChanged();
        }
    }

    public void Dispose()
    {
        (StartLongRunningTaskCommand as IDisposable)?.Dispose();
        (SearchCommand as IDisposable)?.Dispose();
        (StartDownloadCommand as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}