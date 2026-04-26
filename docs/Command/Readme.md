# 🎮 Commands

`Atc.XamlToolkit` ships four ICommand implementations — sync and async, with and without a typed parameter — plus the `[RelayCommand]` source generator that wraps them in zero-boilerplate properties on your `ViewModelBase`-derived view model. This page is the entry point. For deep-dives, jump to:

- [Async cancellation, `AutoSetIsBusy`, error handlers, and `IsExecuting` bindings](AsyncCommandCancellation.md)
- [Error handling for async commands — `IErrorHandler`](ErrorHandling.md)
- [Source-generated `[RelayCommand]` recipe](../SourceGenerators/ViewModel.md)

## Pick the right command

| Command | Use when | Source |
|---|---|---|
| `RelayCommand` | Fire-and-forget action, no parameter | `Action execute, Func<bool>? canExecute` |
| `RelayCommand<T>` | Action with a typed parameter (e.g., from `CommandParameter`) | `Action<T> execute, Func<T, bool>? canExecute` |
| `RelayCommandAsync` | `async Task` work, no parameter, optional `CancellationToken` overload | `Func<Task> execute` or `Func<CancellationToken, Task> execute` |
| `RelayCommandAsync<T>` | `async Task` work with a typed parameter | `Func<T, Task>` or `Func<T, CancellationToken, Task>` |

All four are available on every platform package (WPF / WinUI / Avalonia); their public API is identical. WinUI's async variants additionally marshal `PropertyChanged(IsExecuting)` to the UI thread (see the [WinUI threading note](AsyncCommandCancellation.md#winui-threading-and-isexecuting-binding) below).

## Quick recipes

### Sync command with `CanExecute`

```csharp
public partial class CustomerViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? customerName;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // …persist…
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(CustomerName);
}
```

The generator emits `public IRelayCommand SaveCommand` and re-evaluates `CanSave` whenever `CustomerName` changes (because the `CanExecute` method's expression closes over an observable property).

### Typed-parameter command

```csharp
[RelayCommand]
private void Delete(Customer customer)
{
    Customers.Remove(customer);
}
```

```xml
<Button Content="Delete"
        Command="{Binding DeleteCommand}"
        CommandParameter="{Binding}" />
```

The toolkit converts the `object?` parameter from the binding to `T` via `IConvertible` when possible. Conversion failures (overflow, wrong type, bad format) are caught — the command becomes a no-op rather than throwing — and async commands additionally route the conversion exception through the registered `IErrorHandler`.

**Null pass-through.** When the binding evaluates to `null` and `T` is a reference type, the toolkit invokes your delegate with `null`. The .NET runtime cannot distinguish `Customer` from `Customer?` (both resolve to the same `Type`), so the toolkit can't conditionally skip execution based on declared nullability — guard the parameter inside your delegate if `null` is unsafe. When `T` is a value type, a null parameter is replaced with `default(T)`.

### Async command with `IsBusy` + cancellation

```csharp
[RelayCommand(AutoSetIsBusy = true, SupportsCancellation = true)]
private async Task LoadAsync(CancellationToken cancellationToken)
{
    var data = await api.GetAsync(cancellationToken);
    Items = data;
}
```

The generator emits `LoadCommand`, `LoadCancelCommand`, and `CancelLoad()`; the body is wrapped in a `try`/`finally` that marshals `IsBusy = true/false` onto the UI dispatcher. See [Async cancellation](AsyncCommandCancellation.md) for the full story.

### Manual command (no source generator)

```csharp
public class MyViewModel : ViewModelBase
{
    public IRelayCommand SaveCommand { get; }

    public MyViewModel()
    {
        SaveCommand = new RelayCommand(SaveCore, () => !HasErrors);
    }

    private void SaveCore() { … }
}
```

Use this when you can't use the generator (e.g., a non-partial class, a base type that isn't `ViewModelBase`, or commands constructed at runtime from a factory).

## Error handling

All four commands accept an optional `IErrorHandler errorHandler`:

```csharp
public class LogErrorHandler : IErrorHandler
{
    public void HandleError(Exception ex) => Log.Error(ex, "Command failed");
}

public MyViewModel()
{
    SaveCommand = new RelayCommandAsync(SaveAsync, errorHandler: new LogErrorHandler());
}
```

For async commands, exceptions thrown by the `execute` delegate **and** parameter-conversion failures are routed to `HandleError`. For sync commands, only parameter-conversion failures are caught (your `execute` delegate's exceptions propagate to the binding system as before; wrap in your own try/catch if you need to swallow them).

See **[ErrorHandling.md](ErrorHandling.md)** for the full contract: when handlers fire (and when they don't — `OperationCanceledException` is silently swallowed), implementation patterns (logger / toast / view-model-wide), and the anti-patterns that turn a useful diagnostic hook into a process-killer.

## Cross-references

- [Async cancellation — full guide](AsyncCommandCancellation.md)
- [Source-generated `[RelayCommand]`](../SourceGenerators/ViewModel.md)
- [`MainWindowViewModelBase.ApplicationExitCommand`](../Mvvm/Readme.md#-mainwindowviewmodelbase) — example of binding a built-in command.
- [`EventToCommandBehavior`](../Behaviors/EventToCommandBehavior.md) — invoke a command from any event, not just buttons.
