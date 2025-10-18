# Async Command Cancellation Support

The `RelayCommandAsync` and `RelayCommandAsync<T>` classes now support cancellation tokens, allowing you to cancel long-running asynchronous operations.

## Features

- **Cancellation Token Support**: Pass a `CancellationToken` to your async command handlers
- **Built-in Cancellation Management**: Commands automatically manage `CancellationTokenSource` lifecycle
- **IDisposable Implementation**: Proper resource cleanup for cancellation tokens

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
        var results = await _searchService.SearchAsync(query, cancellationToken);
        
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
    private readonly IErrorHandler _errorHandler;
    public IRelayCommandAsync DownloadCommand { get; }

    public DownloadViewModel(IErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
        
        DownloadCommand = new RelayCommandAsync(
            execute: DownloadFileAsync,
            canExecute: () => !DownloadCommand.IsExecuting,
            errorHandler: _errorHandler);
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
        (DownloadCommand as IDisposable)?.Dispose();
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

    private int _progress;
    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
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

1. **IDisposable**: Commands now implement `IDisposable`. If you create commands directly (not through dependency injection), consider disposing them when no longer needed.

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
        (LoadDataCommand as IDisposable)?.Dispose();
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

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
        await _service.LoadDataAsync(cancellationToken);
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
        var results = await _searchService.SearchAsync(query);
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
    private bool _hasConnection;
    
    [RelayCommand(CanExecute = nameof(HasConnection), SupportsCancellation = true)]
    private async Task LoadAsync()
    {
        await _service.LoadDataAsync();
    }
    
    public bool HasConnection
    {
        get => _hasConnection;
        set
        {
            if (SetProperty(ref _hasConnection, value))
            {
                LoadCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
```

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
