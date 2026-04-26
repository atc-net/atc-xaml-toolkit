# 🛟 Error handling for async commands — `IErrorHandler`

`IErrorHandler` is the toolkit's hook for catching exceptions that escape an async `[RelayCommand]` body. Bind one in at construction time and your command body gets a quiet, predictable failure path: the exception lands at `HandleError(Exception)` instead of vanishing into the dispatcher and surfacing as a runtime crash later.

## The interface

```csharp
namespace Atc.XamlToolkit.Command;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}
```

One method. The implementation chooses what to do with the exception — log, telemetry, user-facing toast, fail-fast in dev — and returns. Whatever you do, **return promptly**: the call happens on whichever context completed the failed `await`, so a long block here will stall the dispatcher.

## Where it wires in

Pass an `IErrorHandler` to the `RelayCommandAsync` / `RelayCommandAsync<T>` constructors:

```csharp
public class CustomerViewModel : ViewModelBase
{
    private readonly IErrorHandler errorHandler;

    public IRelayCommandAsync SaveCommand { get; }

    public CustomerViewModel(IErrorHandler errorHandler)
    {
        this.errorHandler = errorHandler;

        SaveCommand = new RelayCommandAsync(
            SaveAsync,
            errorHandler: errorHandler);
    }

    private async Task SaveAsync()
    {
        // …work that may throw…
    }
}
```

Generated commands (the `[RelayCommand]` attribute) **don't** currently take an `IErrorHandler` argument from the attribute itself — wire them through a manual `RelayCommandAsync` if you need per-command error routing, or set up a view-model-wide handler (see *Patterns* below).

## Async-only — and that's deliberate

The `try/catch (Exception ex) when (errorHandler is not null)` block lives in `RelayCommandAsyncBase.ExecuteAsync` — **only async commands route exceptions through `HandleError`**. Synchronous `RelayCommand` / `RelayCommand<T>` deliberately let exceptions propagate to the binding system, where they surface as binding-failure traces in the debug output (`PresentationTraceSources` on WPF, `DebugSettings.BindingFailed` on WinUI, `Logger` on Avalonia).

Why the asymmetry?

- A sync command's exception is on the UI dispatcher anyway. A handler is rarely useful — wrap your `execute` body in a try/catch yourself if you want one.
- An async command's exception lands wherever the awaited continuation resumed. Without an interception point, the exception either gets observed nowhere (silently swallowed by the framework) or kills the process via `TaskScheduler.UnobservedTaskException`. Neither is the UX you want.

If you genuinely need a sync error sink: don't try to retrofit one through `IErrorHandler`. Use a delegating wrapper around your sync action.

## Three behaviours you should know

### `OperationCanceledException` is silently swallowed

```csharp
catch (OperationCanceledException)
{
    // Cancellation is expected and not an error - just complete silently
}
catch (Exception ex) when (errorHandler is not null)
{
    errorHandler.HandleError(ex);
}
```

The `OperationCanceledException` catch sits **before** the handler catch. Cancellation never reaches `HandleError`. This is intentional — when a `CancellationToken` is triggered (typically because the user clicked Cancel on a `[RelayCommand(SupportsCancellation = true)]` command), the cancellation is the success path, not an error. If you want cancellation visibility, instrument the cancellation site (the `Cancel{Command}()` method, or the post-cancel cleanup) — not the error handler.

### Parameter-conversion failures **are** routed

When you bind `CommandParameter="abc"` to a typed `IRelayCommandAsync<int>`, the toolkit tries to `Convert.ChangeType` the string. On failure (`InvalidCastException`, `FormatException`, `OverflowException`) it routes the conversion exception through your handler:

```csharp
public IRelayCommandAsync<int> ParseCommand { get; }

ParseCommand = new RelayCommandAsync<int>(
    ParseAsync,
    errorHandler: errorHandler);

// In XAML:
// <Button Command="{Binding ParseCommand}" CommandParameter="not-an-int" />
// → errorHandler.HandleError(FormatException) is called.
```

Without an error handler, the conversion failure becomes a no-op (the command silently doesn't run). With one, you get the diagnostic.

### A null `errorHandler` re-throws

The `when (errorHandler is not null)` filter means: when there's no handler, the catch doesn't engage and the exception propagates. The same mechanic that routes errors to your handler routes them up the dispatch stack when no handler is present. This matters for tests — wire a recording handler in unit tests so async-command failures fail loud rather than vanishing.

## Patterns

### Logger-backed handler

The simplest and most common shape:

```csharp
public sealed class LogErrorHandler(ILogger<LogErrorHandler> logger) : IErrorHandler
{
    public void HandleError(Exception ex)
        => logger.LogError(ex, "Async command failed: {Message}", ex.Message);
}
```

Register once in DI, inject into every view-model that owns async commands.

### User-facing toast / dialog

If you want to surface errors to the user, **don't** call `MessageBox.Show(...)` synchronously — `HandleError` runs on the awaited-continuation context, which may not be the UI thread. Marshal:

```csharp
public sealed class ToastErrorHandler(IDispatcher dispatcher, IToastService toasts) : IErrorHandler
{
    public void HandleError(Exception ex)
        => dispatcher.InvokeAsyncIfRequired(() => toasts.Error(ex.Message));
}
```

The toolkit ships `Dispatcher.InvokeAsyncIfRequired(...)` on all three platforms (see [Dispatcher extensions](../Mvvm/Readme.md)) — same code on each.

### View-model-wide handler

When several commands inside one view-model share the same handling (most cases), expose it as an injected dependency and pass the same instance to every command:

```csharp
public partial class CustomerViewModel : ViewModelBase
{
    private readonly IErrorHandler errorHandler;

    public IRelayCommandAsync SaveCommand { get; }
    public IRelayCommandAsync DeleteCommand { get; }

    public CustomerViewModel(IErrorHandler errorHandler)
    {
        this.errorHandler = errorHandler;

        SaveCommand = new RelayCommandAsync(SaveAsync, errorHandler: errorHandler);
        DeleteCommand = new RelayCommandAsync(DeleteAsync, errorHandler: errorHandler);
    }
}
```

### Static / process-wide handler

For small apps and prototypes, a static singleton works:

```csharp
public static class GlobalErrorHandler : IErrorHandler
{
    public static GlobalErrorHandler Instance { get; } = new();

    public void HandleError(Exception ex)
        => Debug.WriteLine($"[Async error] {ex}");
}

// usage
SaveCommand = new RelayCommandAsync(SaveAsync, errorHandler: GlobalErrorHandler.Instance);
```

Acceptable for sample apps and tests; in production, prefer DI so the handler is mockable.

## What not to do

| Anti-pattern | Why |
|---|---|
| Rethrow inside `HandleError` | The toolkit's catch was the last line of defense — rethrowing climbs into framework code that has no further interception. Process termination via `TaskScheduler.UnobservedTaskException` is likely. |
| Block the calling thread | `HandleError` runs on whichever context resumed the failed `await`. A blocking call (`Thread.Sleep`, `.Result`, `.Wait()`) stalls the dispatcher in WPF/WinUI samples. Use async-throughout — but `HandleError` itself is `void`, so you can't `await` inside it. Marshal asynchronously to a background queue if you need long work. |
| Show a modal dialog without dispatcher marshalling | `MessageBox.Show` from a background thread on WPF deadlocks; `ContentDialog.ShowAsync` on WinUI throws `RPC_E_WRONG_THREAD`. Marshal first. |
| Treat `OperationCanceledException` as an error | It never reaches the handler — the catch order in `RelayCommandAsyncBase` filters it out. Don't write defensive code that re-checks. |
| Use `IErrorHandler` for **sync** commands | Sync commands don't route to the handler. Wrap your sync `execute` in a try/catch if you need one. |
| Catch all and discard | Even `Debug.WriteLine` is better than `catch { }`. Silent failures are diagnosis-hostile. |

## Composition notes

### With `AutoSetIsBusy = true`

`AutoSetIsBusy` wraps the `execute` body in `try { IsBusy = true; … } finally { IsBusy = false; }`. The handler runs **after** the inner exception escapes the body but **before** the finally fires — so by the time `HandleError` is called, `IsBusy` is still `true`. The `finally` block resets it as the call unwinds:

```
[user clicks Save]
→ IsBusy = true
→ SaveAsync() throws
→ errorHandler.HandleError(ex)        // IsBusy is still true here
→ IsBusy = false (in finally)
→ command returns
```

If you want a guaranteed `IsBusy == false` at the time the handler reports the error, observe `IsBusy` from the handler; don't try to reorder.

### With `SupportsCancellation = true`

Cancellation does not reach the handler (see above). The cancel command's `CanExecute` and the busy-state lifecycle compose normally.

### With `AsyncCommandCancellation` patterns

The full async cancellation guide ([AsyncCommandCancellation.md](AsyncCommandCancellation.md)) shows three concrete examples — each one wires `errorHandler:` on the `RelayCommandAsync` constructor in the same shape as above.

## Cross-references

- [`IErrorHandler` source](../../src/Atc.XamlToolkit/Command/IErrorHandler.cs)
- [`RelayCommandAsyncBase.ExecuteAsync`](../../src/Atc.XamlToolkit/Command/RelayCommandAsyncBase.cs) — the actual try/catch site
- [Async cancellation — full guide](AsyncCommandCancellation.md)
- [Commands overview](Readme.md)
- [Source-generated `[RelayCommand]`](../SourceGenerators/ViewModel.md)
