namespace Atc.XamlToolkit.Diagnostics;

/// <summary>
/// Listens to Avalonia binding errors via <see cref="Avalonia.Logging.Logger"/>
/// and forwards each error message to the <see cref="BindingErrorOccurred"/> event.
/// </summary>
/// <remarks>
/// Avalonia routes binding diagnostics through its global logger. <see cref="StartTrace"/>
/// installs a sink that filters for <see cref="Avalonia.Logging.LogArea.Binding"/> at the
/// configured minimum level and re-broadcasts each entry to <see cref="BindingErrorOccurred"/>.
/// Any sink already assigned to <see cref="Avalonia.Logging.Logger.Sink"/> is preserved
/// and forwarded to as well, so existing logging integrations continue to work.
/// </remarks>
public static class BindingErrorTraceListener
{
    private static ForwardingSink? activeSink;

    /// <summary>
    /// Raised once per binding-error log entry from Avalonia.
    /// Subscribe to log binding errors without blocking the UI.
    /// </summary>
    public static event EventHandler<BindingErrorEventArgs>? BindingErrorOccurred;

    /// <summary>
    /// Starts forwarding Avalonia binding-error logs to <see cref="BindingErrorOccurred"/>.
    /// </summary>
    /// <param name="minimumLevel">
    /// Minimum log level to forward. Defaults to <see cref="Avalonia.Logging.LogEventLevel.Warning"/>,
    /// which is what Avalonia uses for binding failures.
    /// </param>
    public static void StartTrace(
        Avalonia.Logging.LogEventLevel minimumLevel = Avalonia.Logging.LogEventLevel.Warning)
    {
        if (activeSink is not null)
        {
            return;
        }

        var inner = Avalonia.Logging.Logger.Sink;
        activeSink = new ForwardingSink(inner, minimumLevel, RaiseBindingErrorOccurred);
        Avalonia.Logging.Logger.Sink = activeSink;
    }

    /// <summary>
    /// Stops forwarding binding-error logs and restores the previously installed sink.
    /// Safe to call multiple times.
    /// </summary>
    public static void CloseTrace()
    {
        if (activeSink is null)
        {
            return;
        }

        Avalonia.Logging.Logger.Sink = activeSink.Inner;
        activeSink = null;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A subscriber should never break the logging pipeline.")]
    private static void RaiseBindingErrorOccurred(string message)
    {
        try
        {
            BindingErrorOccurred?.Invoke(null, new BindingErrorEventArgs(message));
        }
        catch
        {
            // A subscriber should not break the logging pipeline.
        }
    }

    [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "Implements ILogSink overloads called via interface dispatch.")]
    private sealed class ForwardingSink(
        Avalonia.Logging.ILogSink? inner,
        Avalonia.Logging.LogEventLevel minimumLevel,
        Action<string> raise)
        : Avalonia.Logging.ILogSink
    {
        public Avalonia.Logging.ILogSink? Inner => inner;

        public bool IsEnabled(
            Avalonia.Logging.LogEventLevel level,
            string area)
        {
            var ourMatch = level >= minimumLevel
                && string.Equals(area, Avalonia.Logging.LogArea.Binding, StringComparison.Ordinal);
            return ourMatch || (inner?.IsEnabled(level, area) ?? false);
        }

        public void Log(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate)
        {
            ForwardIfBinding(level, area, messageTemplate, []);
            inner?.Log(level, area, source, messageTemplate);
        }

        public void Log<T0>(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            T0 propertyValue0)
        {
            ForwardIfBinding(level, area, messageTemplate, [propertyValue0]);
            inner?.Log(level, area, source, messageTemplate, propertyValue0);
        }

        public void Log<T0, T1>(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            T0 propertyValue0,
            T1 propertyValue1)
        {
            ForwardIfBinding(level, area, messageTemplate, [propertyValue0, propertyValue1]);
            inner?.Log(level, area, source, messageTemplate, propertyValue0, propertyValue1);
        }

        public void Log<T0, T1, T2>(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            T0 propertyValue0,
            T1 propertyValue1,
            T2 propertyValue2)
        {
            ForwardIfBinding(level, area, messageTemplate, [propertyValue0, propertyValue1, propertyValue2]);
            inner?.Log(level, area, source, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
        }

        public void Log(
            Avalonia.Logging.LogEventLevel level,
            string area,
            object? source,
            string messageTemplate,
            params object?[] propertyValues)
        {
            ForwardIfBinding(level, area, messageTemplate, propertyValues);
            inner?.Log(level, area, source, messageTemplate, propertyValues);
        }

        private void ForwardIfBinding(
            Avalonia.Logging.LogEventLevel level,
            string area,
            string messageTemplate,
            object?[] propertyValues)
        {
            if (level < minimumLevel ||
                !string.Equals(area, Avalonia.Logging.LogArea.Binding, StringComparison.Ordinal))
            {
                return;
            }

            // Avalonia's Logger interpolates {0}, {1}, ... placeholders with positional values.
            // This mirrors the framework's own formatting closely enough for diagnostic display.
            var message = propertyValues.Length == 0
                ? messageTemplate
                : FormatTemplate(messageTemplate, propertyValues);
            raise(message);
        }

        private static string FormatTemplate(
            string template,
            object?[] values)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, values);
            }
            catch (FormatException)
            {
                return template;
            }
        }
    }
}