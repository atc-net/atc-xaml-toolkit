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
    private bool isTracingEvent;

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
            // WPF only enables its trace sources when a debugger is attached or tracing is turned on
            // in the registry. Refresh enables them, so binding errors are also traced without a debugger.
            PresentationTraceSources.Refresh();

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
    public override void TraceEvent(
        TraceEventCache? eventCache,
        string source,
        TraceEventType eventType,
        int id,
        string? message)
    {
        if (IsIgnored(message))
        {
            return;
        }

        // The base implementation writes a header, the message and one line per trace option.
        // They are collected, so the event is raised once with the complete text.
        isTracingEvent = true;
        try
        {
            base.TraceEvent(eventCache, source, eventType, id, message);
        }
        finally
        {
            isTracingEvent = false;
        }

        ReportError();
    }

    /// <inheritdoc />
    public override void TraceEvent(
        TraceEventCache? eventCache,
        string source,
        TraceEventType eventType,
        int id,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string? format,
        params object?[]? args)
    {
        var message = format is null || args is null || args.Length == 0
            ? format
            : string.Format(CultureInfo.InvariantCulture, format, args);

        TraceEvent(eventCache, source, eventType, id, message);
    }

    /// <inheritdoc />
    public override void Write(string? message)
    {
        _ = errorMessage.Append(message);
    }

    /// <inheritdoc />
    public override void WriteLine(string? message)
    {
        if (isTracingEvent)
        {
            // The header ends with a space and is followed by the message; the trace options follow on their own lines
            if (errorMessage.Length > 0 && errorMessage[^1] != ' ')
            {
                _ = errorMessage.AppendLine();
            }

            _ = errorMessage.Append(message);
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (IsIgnored(message))
        {
            // Drop what was written for the ignored message, so it does not end up in the next error
            errorMessage.Length = 0;
            return;
        }

        _ = errorMessage.Append(message);
        ReportError();
    }

    private bool IsIgnored(string? message)
        => message is not null &&
           ignoreErrorMessages.Exists(x => message.Contains(x, StringComparison.Ordinal));

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A subscriber should never break the trace pipeline.")]
    private void ReportError()
    {
        var error = errorMessage.ToString();
        errorMessage.Length = 0;

        if (error.Length == 0)
        {
            return;
        }

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

        // Without an application there is no dispatcher to show the message box on
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
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