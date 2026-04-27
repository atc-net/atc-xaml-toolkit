# 🔎 BindingErrorTraceListener

`BindingErrorTraceListener` is a cross-platform helper that taps the XAML framework's binding-error pipeline and re-broadcasts each error as a single `BindingErrorOccurred` event you can subscribe to from anywhere — a logger, a status bar, telemetry, or a debug overlay. Available on **WPF**, **WinUI 3**, and **Avalonia**, all in the shared `Atc.XamlToolkit.Diagnostics` namespace.

## Why this exists

XAML platforms produce binding errors at runtime — a `Path` doesn't exist on the `DataContext`, a converter throws, a binding source goes null. By default, these surface only in the debug output window. That's fine when you're at the keyboard with a debugger attached; it's silent everywhere else.

`BindingErrorTraceListener` makes binding errors observable so you can log them next to the rest of your application telemetry, surface them in a developer overlay, or fail fast in CI screenshot tests.

## Common API surface

All three platforms expose the same event:

```csharp
public static event EventHandler<BindingErrorEventArgs>? BindingErrorOccurred;
```

The `BindingErrorEventArgs.Message` property contains the formatted message that the platform itself produced.

## WPF

```csharp
using Atc.XamlToolkit.Diagnostics;

protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    BindingErrorTraceListener.BindingErrorOccurred += (_, args) =>
        logger.LogWarning("Binding error: {Message}", args.Message);

    // Modern apps should turn the modal MessageBox UX off and rely on the event.
    BindingErrorTraceListener.StartTrace(
        SourceLevels.Error,
        TraceOptions.None,
        showMessageBoxOnError: false);
}
```

The classic single-argument call still works for backward compatibility:

```csharp
// Default — emits both the event AND a modal MessageBox per error.
BindingErrorTraceListener.StartTrace();
```

The `ShowMessageBoxOnError` static property can also be flipped at runtime if you want to toggle the dialog UX based on a feature flag.

## WinUI 3

```csharp
using Atc.XamlToolkit.Diagnostics;

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    BindingErrorTraceListener.BindingErrorOccurred += (_, e) =>
        logger.LogWarning("Binding error: {Message}", e.Message);

    BindingErrorTraceListener.StartTrace();

    base.OnLaunched(args);
}
```

WinUI 3's binding tracing is debugger-only; the `StartTrace()` call sets `Application.Current.DebugSettings.IsBindingTracingEnabled = true` for you and subscribes to `DebugSettings.BindingFailed`. If no debugger is attached, the framework silently emits no events.

## Avalonia

```csharp
using Atc.XamlToolkit.Diagnostics;

public override void OnFrameworkInitializationCompleted()
{
    BindingErrorTraceListener.BindingErrorOccurred += (_, e) =>
        logger.LogWarning("Binding error: {Message}", e.Message);

    BindingErrorTraceListener.StartTrace();

    base.OnFrameworkInitializationCompleted();
}
```

Internally the Avalonia version installs an `ILogSink` that filters for `LogArea.Binding` at the configured level (default `Warning`). Any sink already on `Avalonia.Logging.Logger.Sink` is **preserved and forwarded to** — your existing logging integration keeps working.

You can lower the threshold to capture every binding diagnostic:

```csharp
BindingErrorTraceListener.StartTrace(Avalonia.Logging.LogEventLevel.Information);
```

## Stopping

All three platforms expose `CloseTrace()` to detach the listener (and, on Avalonia, restore the previously installed sink). Safe to call multiple times.

```csharp
BindingErrorTraceListener.CloseTrace();
```

## Event-handler exceptions

Each platform wraps the subscriber callback in a `try/catch` so that an exception inside one handler cannot break the framework's binding pipeline. Subscribers should still avoid heavy work on the callback path — the event fires synchronously on whatever thread the underlying platform raised the diagnostic from.
