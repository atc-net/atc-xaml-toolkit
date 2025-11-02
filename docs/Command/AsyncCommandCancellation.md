# Async Command Cancellation Support

The `RelayCommandAsync` and `RelayCommandAsync<T>` classes now support cancellation tokens, allowing you to cancel long-running asynchronous operations.

## Features

- **Cancellation Token Support**: Pass a `CancellationToken` to your async command handlers
- **Built-in Cancellation Management**: Commands automatically manage `CancellationTokenSource` lifecycle
- **IDisposable Implementation**: Async command interfaces inherit from `IDisposable` for proper resource cleanup
- **Source Generator Support**: Automatic generation of `DisposeCommands()` helper method for ViewModels with cancellable commands

## Basic Usage

### RelayCommandAsync with Cancellation

```csharp
public class MyViewModel : ObservableObject
{
    public IRelayCommandAsync LoadDataCommand { get; }

    public MyViewModel()
    {
        // Create command with cancellation token support
        LoadDataCommand = new RelayCommandAsync(LoadDataAsync);
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 100; i++)
        {
            // Check if cancellation was requested
            cancellationToken.ThrowIfCancellationRequested();

            // Simulate work
            await Task.Delay(100, cancellationToken);

            // Update progress or data
            Progress = i;
        }
    }

    public void CancelOperation()
    {
        // Cancel the ongoing command execution
        LoadDataCommand.Cancel();
    }
}
```

### RelayCommandAsync<T> with Cancellation

```csharp
public class MyViewModel : ObservableObject
{
    public IRelayCommandAsync<string> SearchCommand { get; }

    public MyViewModel()
    {
        SearchCommand = new RelayCommandAsync<string>(SearchAsync);
    }

    private async Task SearchAsync(string query, CancellationToken cancellationToken)
    {
        // Perform search with cancellation support
        var results = await searchService.SearchAsync(query, cancellationToken);

        SearchResults = results;
    }

    public void CancelSearch()
    {
        SearchCommand.Cancel();
    }
}
```

## Advanced Scenarios

### With CanExecute and Error Handling

```csharp
public class DownloadViewModel : ObservableObject, IDisposable
{
    private readonly IErrorHandler errorHandler;
    public IRelayCommandAsync DownloadCommand { get; }

    public DownloadViewModel(IErrorHandler errorHandler)
    {
        this.errorHandler = errorHandler;

        DownloadCommand = new RelayCommandAsync(
            execute: DownloadFileAsync,
            canExecute: () => !DownloadCommand.IsExecuting,
            errorHandler: this.errorHandler);
    }

    private async Task DownloadFileAsync(CancellationToken cancellationToken)
    {
        using var client = new HttpClient();

        var response = await client.GetAsync(
            "https://example.com/largefile.zip",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = File.Create("largefile.zip");

        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public void Dispose()
    {
        DownloadCommand.Dispose();
    }
}
```

### XAML Binding

```xml
<Window>
    <StackPanel>
        <Button Content="Start Download"
                Command="{Binding DownloadCommand}" />

        <Button Content="Cancel Download"
                Command="{Binding CancelDownloadCommand}" />

        <ProgressBar Value="{Binding Progress}"
                     Maximum="100" />
    </StackPanel>
</Window>
```

```csharp
public class DownloadViewModel : ObservableObject
{
    public IRelayCommandAsync DownloadCommand { get; }
    public IRelayCommand CancelDownloadCommand { get; }

    private int progress;
    public int Progress
    {
        get => progress;
        set => SetProperty(ref progress, value);
    }

    public DownloadViewModel()
    {
        DownloadCommand = new RelayCommandAsync(DownloadAsync);
        CancelDownloadCommand = new RelayCommand(
            () => DownloadCommand.Cancel(),
            () => DownloadCommand.IsExecuting);
    }

    private async Task DownloadAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(50, cancellationToken);
            Progress = i;
        }
    }
}
```

## Migration from Non-Cancellable Commands

Existing commands without cancellation support continue to work as before. The new overload is opt-in:

```csharp
// Old way (still works)
var command1 = new RelayCommandAsync(async () =>
{
    await Task.Delay(1000);
});

// New way with cancellation support
var command2 = new RelayCommandAsync(async (cancellationToken) =>
{
    await Task.Delay(1000, cancellationToken);
});
```

## Important Notes

1. **IDisposable**: `IRelayCommandAsync` and `IRelayCommandAsync<T>` now inherit from `IDisposable`. Dispose commands when the ViewModel is disposed to properly clean up `CancellationTokenSource` resources. The source generator automatically creates a `DisposeCommands()` helper method for ViewModels with cancellable commands.

2. **Cancellation is Cooperative**: The async method must check the `CancellationToken` and respond to cancellation requests. Simply calling `Cancel()` doesn't forcibly stop the operation.

3. **OperationCanceledException**: When a cancellation token is triggered, operations typically throw `OperationCanceledException`. Use an `IErrorHandler` to handle these gracefully.

4. **IsExecuting**: The `IsExecuting` property remains true during cancellation until the async method completes or throws.

## Migration Guide

### Migrating Existing Async Commands

If you have existing async commands without cancellation support, here's how to migrate them:

#### Step 1: Update Method Signature

Add a `CancellationToken` parameter to your async method:

```csharp
// Before
private async Task LoadDataAsync()
{
    await Task.Delay(1000);
    // ... rest of implementation
}

// After
private async Task LoadDataAsync(CancellationToken cancellationToken)
{
    await Task.Delay(1000, cancellationToken);
    // ... rest of implementation
}
```

#### Step 2: Pass CancellationToken to Async APIs

Most .NET async APIs accept a `CancellationToken`:

```csharp
// Before
var response = await httpClient.GetAsync(url);
var data = await stream.ReadAsync(buffer, 0, buffer.Length);

// After
var response = await httpClient.GetAsync(url, cancellationToken);
var data = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
```

#### Step 3: Check for Cancellation in Loops

For long-running loops, periodically check for cancellation:

```csharp
private async Task ProcessItemsAsync(CancellationToken cancellationToken)
{
    foreach (var item in items)
    {
        // Check if cancellation was requested
        cancellationToken.ThrowIfCancellationRequested();

        await ProcessItemAsync(item, cancellationToken);
    }
}
```

#### Step 4: Update Command Initialization

The command initialization stays the same - the framework automatically detects the `CancellationToken` parameter:

```csharp
// Before and After - same code!
LoadDataCommand = new RelayCommandAsync(LoadDataAsync);
```

### Breaking Changes

**None!** This is a fully backward-compatible addition. Existing commands without `CancellationToken` continue to work unchanged.

### Considerations

- **Performance**: Commands with cancellation support use a `CancellationTokenSource`, which has minimal overhead
- **Memory**: Each command instance holds a `CancellationTokenSource` that is created/disposed per execution
- **Threading**: Cancellation is thread-safe and can be called from any thread

## Best Practices

### 1. Always Pass CancellationToken to Async APIs

When calling async methods, always pass the cancellation token:

```csharp
// ✅ Good
await Task.Delay(1000, cancellationToken);
await httpClient.GetAsync(url, cancellationToken);

// ❌ Bad - ignores cancellation
await Task.Delay(1000);
await httpClient.GetAsync(url);
```

### 2. Check Cancellation in CPU-Bound Operations

For CPU-intensive work, periodically check the token:

```csharp
private async Task ProcessDataAsync(CancellationToken cancellationToken)
{
    for (int i = 0; i < largeDataSet.Count; i++)
    {
        // Check every 100 iterations
        if (i % 100 == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        ProcessItem(largeDataSet[i]);
    }
}
```

### 3. Dispose Commands When Appropriate

If you create commands in a ViewModel that gets disposed:

```csharp
public class MyViewModel : ObservableObject, IDisposable
{
    public IRelayCommandAsync LoadDataCommand { get; }

    public MyViewModel()
    {
        LoadDataCommand = new RelayCommandAsync(LoadDataAsync);
    }

    public void Dispose()
    {
        LoadDataCommand.Dispose();
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

**Note:** When using the source generator with `SupportsCancellation = true`, a `DisposeCommands()` helper method is automatically generated:

```csharp
public partial class MyViewModel : ViewModelBase, IDisposable
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        // Implementation
    }

    public void Dispose()
    {
        DisposeCommands(); // Generated helper method
    }
}
```

The generated `DisposeCommands()` method automatically disposes all async commands with cancellation support in your ViewModel.

### 4. Handle OperationCanceledException Gracefully

Use an error handler to handle cancellations gracefully:

```csharp
public class MyErrorHandler : IErrorHandler
{
    public void HandleError(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            // User cancelled - this is expected, don't show error
            Debug.WriteLine("Operation was cancelled by user");
            return;
        }

        // Show error dialog for other exceptions
        MessageBox.Show($"Error: {exception.Message}");
    }
}
```

### 5. Design for Cancellation from the Start

When designing new features with async operations, consider cancellation from the beginning:

```csharp
// Good design - cancellation-aware from the start
public interface IDataService
{
    Task<List<Item>> LoadItemsAsync(CancellationToken cancellationToken);
}
```

### 6. Update UI During Long Operations

Provide feedback during cancellable operations:

```csharp
private async Task ProcessLargeFileAsync(CancellationToken cancellationToken)
{
    StatusMessage = "Processing file...";

    try
    {
        for (int i = 0; i < 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ProcessChunkAsync(i, cancellationToken);
            Progress = i + 1;
            StatusMessage = $"Processing... {i + 1}%";
        }

        StatusMessage = "Completed successfully";
    }
    catch (OperationCanceledException)
    {
        StatusMessage = "Cancelled by user";
        throw;
    }
}
```

### 7. Multiple Async Commands with Independent Cancellation

Each command has its own cancellation token, allowing independent cancellation:

```csharp
public class DashboardViewModel : ObservableObject
{
    public IRelayCommandAsync LoadUsersCommand { get; }
    public IRelayCommandAsync LoadOrdersCommand { get; }
    public IRelayCommandAsync LoadReportsCommand { get; }

    public DashboardViewModel()
    {
        // Each command can be cancelled independently
        LoadUsersCommand = new RelayCommandAsync(LoadUsersAsync);
        LoadOrdersCommand = new RelayCommandAsync(LoadOrdersAsync);
        LoadReportsCommand = new RelayCommandAsync(LoadReportsAsync);
    }

    public void CancelAllOperations()
    {
        LoadUsersCommand.Cancel();
        LoadOrdersCommand.Cancel();
        LoadReportsCommand.Cancel();
    }
}
```

## Frequently Asked Questions (FAQ)

### Q: Do I need to change existing commands?

**A:** No! Existing commands without `CancellationToken` continue to work exactly as before. The cancellation support is opt-in through method signatures.

### Q: What happens if I call Cancel() when the command isn't executing?

**A:** Nothing. The `Cancel()` method checks if the command is executing and does nothing if it's not. This is safe to call at any time.

### Q: Can I reuse a command after cancellation?

**A:** Yes! After cancellation completes (the async method throws `OperationCanceledException` or completes normally), the command can be executed again. A new `CancellationTokenSource` is created for each execution.

### Q: Does cancellation work across all platforms (WPF, WinUI, Avalonia)?

**A:** Yes! The cancellation support is implemented in the base classes, so it works identically across all XAML platforms.

### Q: How do I bind the cancel button to enable only when the command is executing?

**A:** Use the `IsExecuting` property:

```csharp
CancelCommand = new RelayCommand(
    () => LoadDataCommand.Cancel(),
    () => LoadDataCommand.IsExecuting);
```

In XAML:

```xml
<Button Content="Cancel" Command="{Binding CancelCommand}" />
```

### Q: What if my async method doesn't support CancellationToken?

**A:** You have two options:

1. **Wrap in a cancellable task** (for quick operations):
```csharp
private async Task LoadDataAsync(CancellationToken cancellationToken)
{
    await Task.Run(async () =>
    {
        await legacyService.LoadDataAsync(); // No cancellation support
    }, cancellationToken);
}
```

2. **Add cancellation support** to the service method (preferred for long operations)

### Q: Will cancellation forcibly stop my async method?

**A:** No! Cancellation in .NET is **cooperative**. Your async method must:
1. Accept a `CancellationToken` parameter
2. Pass it to async APIs
3. Check it periodically with `ThrowIfCancellationRequested()`

The token won't forcibly abort your code; you must design your method to respect cancellation.

### Q: Should I catch OperationCanceledException in my async method?

**A:** Usually not. Let it propagate to the command's error handler. Only catch it if you need cleanup logic:

```csharp
private async Task LoadDataAsync(CancellationToken cancellationToken)
{
    try
    {
        await service.LoadDataAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        // Cleanup if needed
        CleanupPartialData();
        throw; // Re-throw to signal cancellation
    }
}
```

### Q: How do I test commands with cancellation support?

**A:** Use xUnit or your preferred test framework:

```csharp
[Fact]
public async Task Command_CanBeCancelled()
{
    // Arrange
    var taskCancelled = false;
    using var command = new RelayCommandAsync(async cancellationToken =>
    {
        try
        {
            await Task.Delay(5000, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            taskCancelled = true;
            throw;
        }
    });

    // Act
    var executeTask = command.ExecuteAsync(null);
    await Task.Delay(10);
    command.Cancel();

    try
    {
        await executeTask;
    }
    catch (OperationCanceledException)
    {
        // Expected
    }

    // Assert
    Assert.True(taskCancelled);
}
```

### Q: What's the performance impact of cancellation support?

**A:** Minimal. A `CancellationTokenSource` is created per execution and disposed afterward. The memory overhead is approximately 100-200 bytes per execution, which is negligible for typical UI operations.

### Q: Can I provide my own CancellationToken?

**A:** No, the command manages its own `CancellationTokenSource` internally. This ensures proper lifecycle management and prevents issues with shared tokens. Use the `Cancel()` method to trigger cancellation.

### Q: Does the source generator support CancellationToken?

**A:** Yes! The source generator supports cancellation in two ways:

#### 1. Automatic Detection (Method with CancellationToken parameter)

If your method signature includes a `CancellationToken` parameter, the generator automatically creates the command with cancellation support:

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    // Generator creates:
    // - public IRelayCommandAsync LoadDataCommand { get; }
    // - Cancel method available: LoadDataCommand.Cancel()
}
```

#### 2. Explicit Declaration (SupportsCancellation property)

Use `SupportsCancellation = true` to generate a `Cancel{CommandName}()` method even when your async method doesn't have a `CancellationToken` parameter:

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task ProcessAsync()
    {
        // No CancellationToken parameter
        await Task.Delay(1000);
    }

    // Generator creates:
    // - public IRelayCommandAsync ProcessCommand { get; }
    // - public void CancelProcess() => ProcessCommand.Cancel();
}
```

This is useful for:

- Convenience methods to cancel commands from code or XAML
- Consistent cancellation API across your ViewModels
- Commands that call services with their own cancellation handling

**See [ViewModel Source Generator Documentation](../SourceGenerators/ViewModel.md#-commands-with-cancellation-support) for more details and advanced scenarios.**

## Using Source Generator for Cancellation

The `[RelayCommand]` attribute makes working with cancellable async commands much simpler than manual command creation.

### Basic Example with Auto-Detection

```csharp
public partial class DownloadViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task DownloadAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i <= 100; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(50, cancellationToken);
            Progress = i;
        }
    }

    // Use the generated command:
    // <Button Content="Download" Command="{Binding DownloadCommand}" />
    // <Button Content="Cancel" Command="{Binding DownloadCommand.CancelCommand}" />
}
```

### Example with SupportsCancellation

```csharp
public partial class SearchViewModel : ViewModelBase
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task SearchAsync(string query)
    {
        // Your existing method without CancellationToken
        var results = await searchService.SearchAsync(query);
        SearchResults = results;
    }

    // Generator creates CancelSearch() method for easy access
    public void ClearSearch()
    {
        CancelSearch(); // Generated method
        SearchResults = null;
    }

    // XAML binding:
    // <Button Content="Search" Command="{Binding SearchCommand}" CommandParameter="{Binding QueryText}" />
    // <Button Content="Cancel" Click="ClearSearchButton_Click" />
}
```

### Combining with CanExecute

```csharp
public partial class DataViewModel : ViewModelBase
{
    private bool hasConnection;

    [RelayCommand(CanExecute = nameof(HasConnection), SupportsCancellation = true)]
    private async Task LoadAsync()
    {
        await service.LoadDataAsync();
    }

    public bool HasConnection
    {
        get => hasConnection;
        set
        {
            if (SetProperty(ref hasConnection, value))
            {
                LoadCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
```

### Combining with AutoSetIsBusy

The most powerful combination is using `SupportsCancellation` with `AutoSetIsBusy`, which automatically manages the `IsBusy` property and enables cancellation. This combination also generates a dedicated cancel command for easy XAML binding.

**Key features when combining both:**

- ✅ **Automatic IsBusy management** - Sets `IsBusy = true` during execution, `false` when complete
- ✅ **Thread-safe UI updates** - Uses `Dispatcher.InvokeAsyncIfRequired()` for cross-thread property updates
- ✅ **Cancel command generation** - Creates `{CommandName}CancelCommand` property
- ✅ **Cancel method generation** - Creates `Cancel{CommandName}()` method
- ✅ **Proper exception handling** - `OperationCanceledException` is caught and handled silently

#### Basic Example: No Parameters with CancellationToken

```csharp
public partial class DownloadViewModel : ViewModelBase
{
    [ObservableProperty]
    private string statusMessage = "Ready to start...";

    [ObservableProperty]
    private int progress;

    [RelayCommand(AutoSetIsBusy = true, SupportsCancellation = true)]
    private async Task DownloadAsync(CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = "Starting download...";

            for (int i = 0; i <= 100; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(50, cancellationToken);
                Progress = i;
                StatusMessage = $"Downloading... {i}%";
            }

            StatusMessage = "Download completed!";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Download cancelled!";
        }
    }

    // Generator creates:
    // - DownloadCommand (IRelayCommandAsync)
    // - DownloadCancelCommand (IRelayCommand) for easy XAML binding
    // - CancelDownload() method
    // - Automatic IsBusy = true/false management with Dispatcher marshalling
}
```

**Generated code:**

```csharp
public partial class DownloadViewModel
{
    private IRelayCommandAsync? downloadCommand;
    private IRelayCommand? downloadCancelCommand;

    public IRelayCommandAsync DownloadCommand => downloadCommand ??= new RelayCommandAsync(
        async (CancellationToken cancellationToken) =>
        {
            await Application.Current.Dispatcher
                .InvokeAsyncIfRequired(() => IsBusy = true)
                .ConfigureAwait(false);

            try
            {
                await DownloadAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await Application.Current.Dispatcher
                    .InvokeAsyncIfRequired(() => IsBusy = false)
                    .ConfigureAwait(false);
            }
        });

    public IRelayCommand DownloadCancelCommand => downloadCancelCommand ??= new RelayCommand(CancelDownload);

    public void CancelDownload()
    {
        DownloadCommand.Cancel();
    }
}
```

**XAML binding:**

```xml
<StackPanel>
    <Button Content="Start Download" Command="{Binding DownloadCommand}" />
    <Button Content="Cancel Download" Command="{Binding DownloadCancelCommand}" />

    <TextBlock Text="{Binding StatusMessage}" Margin="0,10,0,0" />
    <ProgressBar Value="{Binding Progress}" Maximum="100" Height="20" />

    <!-- IsBusy indicator -->
    <TextBlock Text="Processing..."
               Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}" />
</StackPanel>
```

#### Example with Parameter and CancellationToken

When your command accepts a parameter, the generator creates the correct lambda signature with both the parameter and cancellation token:

```csharp
public partial class SearchViewModel : ViewModelBase
{
    [ObservableProperty]
    private string statusMessage = "Ready to search...";

    [ObservableProperty]
    private ObservableCollection<string> searchResults = new();

    [RelayCommand(AutoSetIsBusy = true, SupportsCancellation = true)]
    private async Task SearchAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = $"Searching for '{query}'...";
            SearchResults.Clear();

            // Simulate search with delay
            await Task.Delay(2000, cancellationToken);

            // Simulate results
            for (int i = 1; i <= 10; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
                SearchResults.Add($"Result {i} for '{query}'");
            }

            StatusMessage = $"Found {SearchResults.Count} results";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Search cancelled";
            SearchResults.Clear();
        }
    }

    // Generator creates:
    // - SearchCommand (IRelayCommandAsync<string>)
    // - SearchCancelCommand (IRelayCommand)
    // - CancelSearch() method
}
```

**Generated code:**

```csharp
public partial class SearchViewModel
{
    private IRelayCommandAsync<string>? searchCommand;
    private IRelayCommand? searchCancelCommand;

    public IRelayCommandAsync<string> SearchCommand => searchCommand ??= new RelayCommandAsync<string>(
        async (query, cancellationToken) =>
        {
            await Application.Current.Dispatcher
                .InvokeAsyncIfRequired(() => IsBusy = true)
                .ConfigureAwait(false);

            try
            {
                await SearchAsync(query, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await Application.Current.Dispatcher
                    .InvokeAsyncIfRequired(() => IsBusy = false)
                    .ConfigureAwait(false);
            }
        });

    public IRelayCommand SearchCancelCommand => searchCancelCommand ??= new RelayCommand(CancelSearch);

    public void CancelSearch()
    {
        SearchCommand.Cancel();
    }
}
```

**XAML binding:**

```xml
<StackPanel>
    <TextBox x:Name="QueryTextBox" Text="{Binding QueryText, UpdateSourceTrigger=PropertyChanged}" />

    <StackPanel Orientation="Horizontal">
        <Button Content="Search"
                Command="{Binding SearchCommand}"
                CommandParameter="{Binding Text, ElementName=QueryTextBox}" />
        <Button Content="Cancel" Command="{Binding SearchCancelCommand}" Margin="5,0,0,0" />
    </StackPanel>

    <TextBlock Text="{Binding StatusMessage}" Margin="0,10,0,0" />
    <ListBox ItemsSource="{Binding SearchResults}" Height="200" />
</StackPanel>
```

#### Example with CanExecute + AutoSetIsBusy + SupportsCancellation

All three features can be combined for full command control:

```csharp
public partial class DataProcessingViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool hasData;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [RelayCommand(CanExecute = nameof(CanProcess), AutoSetIsBusy = true, SupportsCancellation = true)]
    private async Task ProcessAsync(string input, CancellationToken cancellationToken)
    {
        try
        {
            StatusMessage = $"Processing '{input}'...";

            for (int i = 0; i < 100; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(50, cancellationToken);
                StatusMessage = $"Processing... {i}%";
            }

            StatusMessage = "Processing completed!";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Processing cancelled";
        }
    }

    private bool CanProcess(string input) => HasData && !string.IsNullOrEmpty(input);

    // Generator creates:
    // - ProcessCommand (IRelayCommandAsync<string>) with CanExecute logic
    // - ProcessCancelCommand (IRelayCommand)
    // - CancelProcess() method
    // - Automatic IsBusy management
}
```

**Generated code:**

```csharp
public partial class DataProcessingViewModel
{
    private IRelayCommandAsync<string>? processCommand;
    private IRelayCommand? processCancelCommand;

    public IRelayCommandAsync<string> ProcessCommand => processCommand ??= new RelayCommandAsync<string>(
        async (input, cancellationToken) =>
        {
            await Application.Current.Dispatcher
                .InvokeAsyncIfRequired(() => IsBusy = true)
                .ConfigureAwait(false);

            try
            {
                await ProcessAsync(input, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await Application.Current.Dispatcher
                    .InvokeAsyncIfRequired(() => IsBusy = false)
                    .ConfigureAwait(false);
            }
        },
        CanProcess);

    public IRelayCommand ProcessCancelCommand => processCancelCommand ??= new RelayCommand(CancelProcess);

    public void CancelProcess()
    {
        ProcessCommand.Cancel();
    }
}
```

#### Example Without CancellationToken Parameter

If your method doesn't have a `CancellationToken` parameter, the generator still creates a cancellable command by wrapping it:

```csharp
public partial class LegacyProcessingViewModel : ViewModelBase
{
    [ObservableProperty]
    private string statusMessage = "Ready";

    [RelayCommand(AutoSetIsBusy = true, SupportsCancellation = true)]
    private async Task ProcessAsync()
    {
        StatusMessage = "Processing...";
        await Task.Delay(5000);  // Note: No cancellation token
        StatusMessage = "Completed!";
    }

    // Generator creates a wrapper that accepts CancellationToken but doesn't pass it
    // Still generates cancel command and method
}
```

**Generated code:**

```csharp
public partial class LegacyProcessingViewModel
{
    private IRelayCommandAsync? processCommand;
    private IRelayCommand? processCancelCommand;

    public IRelayCommandAsync ProcessCommand => processCommand ??= new RelayCommandAsync(
        async (CancellationToken cancellationToken) =>
        {
            await Application.Current.Dispatcher
                .InvokeAsyncIfRequired(() => IsBusy = true)
                .ConfigureAwait(false);

            try
            {
                await ProcessAsync().ConfigureAwait(false);  // Original method without CT
            }
            finally
            {
                await Application.Current.Dispatcher
                    .InvokeAsyncIfRequired(() => IsBusy = false)
                    .ConfigureAwait(false);
            }
        });

    public IRelayCommand ProcessCancelCommand => processCancelCommand ??= new RelayCommand(CancelProcess);

    public void CancelProcess()
    {
        ProcessCommand.Cancel();
    }
}
```

**Note:** In this case, the command is cancellable, but the actual operation may not respect cancellation. For proper cancellation, update your method to accept and use a `CancellationToken`.

### DisposeCommands() Helper Method

When using the source generator with `SupportsCancellation = true`, a `DisposeCommands()` helper method is automatically generated. This method disposes all async commands with cancellation support, making resource cleanup simple and consistent.

#### Generated DisposeCommands() Method

The source generator creates a `DisposeCommands()` method that calls `Dispose()` on all cancellable async commands:

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    [RelayCommand(SupportsCancellation = true)]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    // Generator creates:
    // - LoadCommand (IRelayCommandAsync)
    // - SaveCommand (IRelayCommandAsync)
    // - DisposeCommands() method
}
```

**Generated code:**

```csharp
public partial class MyViewModel
{
    private IRelayCommandAsync? loadCommand;
    private IRelayCommand? loadCancelCommand;
    private IRelayCommandAsync? saveCommand;
    private IRelayCommand? saveCancelCommand;

    public IRelayCommandAsync LoadCommand => loadCommand ??= new RelayCommandAsync(/*...*/);
    public IRelayCommand LoadCancelCommand => loadCancelCommand ??= new RelayCommand(CancelLoad);

    public IRelayCommandAsync SaveCommand => saveCommand ??= new RelayCommandAsync(/*...*/);
    public IRelayCommand SaveCancelCommand => saveCancelCommand ??= new RelayCommand(CancelSave);

    public void CancelLoad()
    {
        LoadCommand.Cancel();
    }

    public void CancelSave()
    {
        SaveCommand.Cancel();
    }

    public void DisposeCommands()
    {
        LoadCommand.Dispose();
        SaveCommand.Dispose();
    }
}
```

#### Using DisposeCommands() in Your ViewModel

Simply call `DisposeCommands()` from your ViewModel's `Dispose()` method:

```csharp
public partial class MyViewModel : ViewModelBase, IDisposable
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public void Dispose()
    {
        DisposeCommands(); // Automatically disposes all cancellable commands
    }
}
```

#### Benefits

- ✅ **No manual disposal code** - The generator handles all command disposal
- ✅ **Consistent API** - Same disposal pattern across all ViewModels
- ✅ **No casting required** - Clean API without `(command as IDisposable)?.Dispose()`
- ✅ **Automatically updated** - Adding new cancellable commands updates `DisposeCommands()` automatically
- ✅ **Type-safe** - Only disposes commands that actually implement `IDisposable`

#### Multiple Commands Example

When you have multiple cancellable commands, `DisposeCommands()` disposes all of them:

```csharp
public partial class DashboardViewModel : ViewModelBase, IDisposable
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadUsersAsync(CancellationToken ct) { /*...*/ }

    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadOrdersAsync(CancellationToken ct) { /*...*/ }

    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadReportsAsync(CancellationToken ct) { /*...*/ }

    public void Dispose()
    {
        DisposeCommands(); // Disposes LoadUsersCommand, LoadOrdersCommand, and LoadReportsCommand
    }
}
```

**Generated DisposeCommands() method:**

```csharp
public void DisposeCommands()
{
    LoadUsersCommand.Dispose();
    LoadOrdersCommand.Dispose();
    LoadReportsCommand.Dispose();
}
```

#### Note

The `DisposeCommands()` method is only generated when at least one command has `SupportsCancellation = true`. Commands without cancellation support don't need disposal and are not included.

### Combining with ExecuteOnBackgroundThread

```csharp
public partial class ProcessingViewModel : ViewModelBase
{
    [RelayCommand(ExecuteOnBackgroundThread = true, SupportsCancellation = true)]
    private async Task ProcessLargeDataAsync(CancellationToken cancellationToken)
    {
        // Runs on background thread with cancellation support
        await Task.Run(() =>
        {
            for (int i = 0; i < 1000000; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ProcessItem(i);
            }
        }, cancellationToken);
    }

    // Generator creates:
    // - ProcessLargeDataCommand with background execution
    // - ProcessLargeDataCancelCommand
    // - CancelProcessLargeData() method
}
```

### Migration from Manual to Source Generator

**Before (Manual):**

```csharp
public class MyViewModel : ViewModelBase
{
    public IRelayCommandAsync LoadDataCommand { get; }

    public MyViewModel()
    {
        LoadDataCommand = new RelayCommandAsync(LoadDataAsync);
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    public void CancelLoad()
    {
        LoadDataCommand.Cancel();
    }
}
```

**After (Source Generator):**

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand(SupportsCancellation = true)]
    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }

    // CancelLoadData() method is auto-generated
    // No constructor needed!
}
```

### When to Use SupportsCancellation

| Scenario | Use SupportsCancellation? |
|----------|--------------------------|
| Method has `CancellationToken` parameter | Optional (auto-detected) |
| Method doesn't have `CancellationToken` parameter | Yes, if you need `Cancel{CommandName}()` method |
| Need explicit cancel method in code | Yes |
| Need to bind cancel button in XAML | Optional (can use `CommandName.Cancel()` directly) |
| Want consistent API across ViewModels | Yes |

## See Also

- [RelayCommandAsync](RelayCommandAsync.md)
- [IErrorHandler](../ErrorHandling/IErrorHandler.md)
- [Command Pattern](../Patterns/CommandPattern.md)
- [MVVM Best Practices](../Mvvm/BestPractices.md)
