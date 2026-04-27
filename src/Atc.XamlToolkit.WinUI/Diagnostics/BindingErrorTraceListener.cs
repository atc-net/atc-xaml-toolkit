namespace Atc.XamlToolkit.Diagnostics;

/// <summary>
/// Listens to WinUI 3 binding failures via <see cref="DebugSettings.BindingFailed"/>
/// and forwards each error message to the <see cref="BindingErrorOccurred"/> event.
/// </summary>
/// <remarks>
/// WinUI 3 reports binding failures only when the debugger is attached and
/// <see cref="DebugSettings.IsBindingTracingEnabled"/> is <c>true</c>; <see cref="StartTrace"/>
/// turns the flag on for you. Subscribe to <see cref="BindingErrorOccurred"/> to log
/// binding failures to a sink of your choice (logger, file, telemetry).
/// </remarks>
public static class BindingErrorTraceListener
{
    private static BindingFailedEventHandler? handler;

    /// <summary>
    /// Raised once per binding-failed event reported by the framework.
    /// Subscribe to log binding errors without blocking the UI.
    /// </summary>
    public static event EventHandler<BindingErrorEventArgs>? BindingErrorOccurred;

    /// <summary>
    /// Starts forwarding <see cref="DebugSettings.BindingFailed"/> events to
    /// <see cref="BindingErrorOccurred"/>. Call from <c>App.OnLaunched</c> after the
    /// <see cref="Application"/> instance is constructed.
    /// </summary>
    /// <remarks>
    /// Enables <see cref="DebugSettings.IsBindingTracingEnabled"/> so the framework
    /// emits binding-failure events. Has no effect in non-debug builds (WinUI silently
    /// suppresses binding tracing when no debugger is attached).
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A subscriber should never break the binding pipeline.")]
    public static void StartTrace()
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        if (handler is not null)
        {
            return;
        }

        application.DebugSettings.IsBindingTracingEnabled = true;

        handler = (_, args) =>
        {
            try
            {
                BindingErrorOccurred?.Invoke(null, new BindingErrorEventArgs(args.Message));
            }
            catch
            {
                // A subscriber should not break the binding pipeline.
            }
        };

        application.DebugSettings.BindingFailed += handler;
    }

    /// <summary>
    /// Stops forwarding binding-failed events. Safe to call multiple times.
    /// </summary>
    public static void CloseTrace()
    {
        if (handler is null)
        {
            return;
        }

        var application = Application.Current;
        if (application is not null)
        {
            application.DebugSettings.BindingFailed -= handler;
            application.DebugSettings.IsBindingTracingEnabled = false;
        }

        handler = null;
    }
}