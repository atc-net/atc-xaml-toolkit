namespace Atc.XamlToolkit.Diagnostics;

/// <summary>
/// Listens to WPF data-binding errors via <see cref="PresentationTraceSources.DataBindingSource"/>
/// and forwards each error message to the <see cref="BindingErrorOccurred"/> event. Optionally
/// surfaces each error as a modal <see cref="MessageBox"/> for ad-hoc debugging.
/// </summary>
/// <remarks>
/// Subscribe to <see cref="BindingErrorOccurred"/> to log binding errors to a sink of your
/// choice (logger, file, telemetry). Keep <see cref="ShowMessageBoxOnError"/> off in production —
/// a modal dialog per binding error blocks the UI. The default is <c>true</c> for backward
/// compatibility with earlier toolkit versions.
/// </remarks>
public sealed class BindingErrorTraceListener : DefaultTraceListener
{
    private static BindingErrorTraceListener? listener;
    private readonly StringBuilder errorMessage = new();
    private readonly List<string> ignoreErrorMessages = new();

    /// <summary>
    /// Prevents a default instance of the <see cref="BindingErrorTraceListener" /> class from being created.
    /// </summary>
    private BindingErrorTraceListener()
    {
        ignoreErrorMessages.Add("Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=Fill; DataItem=null; target element is 'GeometryDrawing'");
    }

    /// <summary>
    /// Raised once per binding-error trace line, on the dispatcher thread.
    /// Subscribe to log binding errors without blocking the UI.
    /// </summary>
    public static event EventHandler<BindingErrorEventArgs>? BindingErrorOccurred;

    /// <summary>
    /// Gets or sets a value indicating whether to display a modal <see cref="MessageBox"/>
    /// for each binding error. Defaults to <c>true</c> for backward compatibility;
    /// set to <c>false</c> in production and rely on <see cref="BindingErrorOccurred"/> instead.
    /// </summary>
    public static bool ShowMessageBoxOnError { get; set; } = true;

    /// <summary>
    /// Starts the trace.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="options">The options.</param>
    public static void StartTrace(
        SourceLevels level = SourceLevels.Error,
        TraceOptions options = TraceOptions.None)
    {
        if (listener is null)
        {
            listener = new BindingErrorTraceListener();
            _ = PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
        }

        listener.TraceOutputOptions = options;
        PresentationTraceSources.DataBindingSource.Switch.Level = level;
    }

    /// <summary>
    /// Starts the trace with explicit MessageBox UX control.
    /// </summary>
    /// <param name="level">The level.</param>
    /// <param name="options">The options.</param>
    /// <param name="showMessageBoxOnError">
    /// If <c>true</c>, each binding error surfaces as a modal <see cref="MessageBox"/>;
    /// if <c>false</c>, errors are silent and only routed to <see cref="BindingErrorOccurred"/>.
    /// </param>
    public static void StartTrace(
        SourceLevels level,
        TraceOptions options,
        bool showMessageBoxOnError)
    {
        ShowMessageBoxOnError = showMessageBoxOnError;
        StartTrace(level, options);
    }

    /// <summary>
    /// Closes the trace.
    /// </summary>
    public static void CloseTrace()
    {
        if (listener is null)
        {
            return;
        }

        listener.Flush();
        listener.Close();
        PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
        listener = null;
    }

    /// <inheritdoc />
    public override void Write(string? message)
    {
        _ = errorMessage.Append(message);
    }

    /// <inheritdoc />
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A subscriber should never break the trace pipeline.")]
    public override void WriteLine(string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (ignoreErrorMessages.Exists(x => message.Contains(x, StringComparison.Ordinal)))
        {
            return;
        }

        _ = errorMessage.Append(message);
        var error = errorMessage.ToString();
        errorMessage.Length = 0;

        // Notify subscribers regardless of the MessageBox UX so loggers always get the event.
        try
        {
            BindingErrorOccurred?.Invoke(null, new BindingErrorEventArgs(error));
        }
        catch
        {
            // A subscriber should not break the trace pipeline.
        }

        if (!ShowMessageBoxOnError)
        {
            return;
        }

        _ = Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Normal,
            new DispatcherOperationCallback(
                delegate
                {
                    _ = MessageBox.Show(error, "Binding Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }),
            arg: null);
    }
}