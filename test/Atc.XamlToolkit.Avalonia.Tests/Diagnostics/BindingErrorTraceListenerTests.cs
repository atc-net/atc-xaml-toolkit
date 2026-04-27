using AvaloniaLogArea = global::Avalonia.Logging.LogArea;
using AvaloniaLogEventLevel = global::Avalonia.Logging.LogEventLevel;
using AvaloniaLogger = global::Avalonia.Logging.Logger;
using BindingErrorEventArgs = Atc.XamlToolkit.Diagnostics.BindingErrorEventArgs;
using BindingErrorTraceListener = Atc.XamlToolkit.Diagnostics.BindingErrorTraceListener;

namespace Atc.XamlToolkit.Avalonia.Tests.Diagnostics;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "xUnit test fixture.")]
public sealed class BindingErrorTraceListenerTests : IDisposable
{
    public BindingErrorTraceListenerTests()
    {
        // Make sure no prior test left a sink installed.
        BindingErrorTraceListener.CloseTrace();
    }

    public void Dispose()
    {
        BindingErrorTraceListener.CloseTrace();
    }

    [Fact]
    public void StartTrace_BindingArea_RaisesBindingErrorOccurred()
    {
        BindingErrorEventArgs? captured = null;
        EventHandler<BindingErrorEventArgs> handler = (_, args) => captured = args;

        BindingErrorTraceListener.BindingErrorOccurred += handler;
        try
        {
            BindingErrorTraceListener.StartTrace();

            AvaloniaLogger.Sink!.Log(
                AvaloniaLogEventLevel.Warning,
                AvaloniaLogArea.Binding,
                source: null,
                "Binding leak: {0}",
                "FooProperty");

            Assert.NotNull(captured);
            Assert.Contains("FooProperty", captured!.Message, StringComparison.Ordinal);
        }
        finally
        {
            BindingErrorTraceListener.BindingErrorOccurred -= handler;
        }
    }

    [Fact]
    public void StartTrace_NonBindingArea_DoesNotRaiseEvent()
    {
        var raisedCount = 0;
        EventHandler<BindingErrorEventArgs> handler = (_, _) => raisedCount++;

        BindingErrorTraceListener.BindingErrorOccurred += handler;
        try
        {
            BindingErrorTraceListener.StartTrace();

            AvaloniaLogger.Sink!.Log(
                AvaloniaLogEventLevel.Warning,
                AvaloniaLogArea.Layout,
                source: null,
                "Layout pass complete");

            Assert.Equal(0, raisedCount);
        }
        finally
        {
            BindingErrorTraceListener.BindingErrorOccurred -= handler;
        }
    }

    [Fact]
    public void StartTrace_BelowMinimumLevel_DoesNotRaiseEvent()
    {
        var raisedCount = 0;
        EventHandler<BindingErrorEventArgs> handler = (_, _) => raisedCount++;

        BindingErrorTraceListener.BindingErrorOccurred += handler;
        try
        {
            BindingErrorTraceListener.StartTrace(AvaloniaLogEventLevel.Error);

            AvaloniaLogger.Sink!.Log(
                AvaloniaLogEventLevel.Warning,
                AvaloniaLogArea.Binding,
                source: null,
                "Below threshold");

            Assert.Equal(0, raisedCount);
        }
        finally
        {
            BindingErrorTraceListener.BindingErrorOccurred -= handler;
        }
    }

    [Fact]
    public void CloseTrace_RestoresPreviousSink()
    {
        var previous = AvaloniaLogger.Sink;

        BindingErrorTraceListener.StartTrace();
        Assert.NotSame(previous, AvaloniaLogger.Sink);

        BindingErrorTraceListener.CloseTrace();
        Assert.Same(previous, AvaloniaLogger.Sink);
    }

    [Fact]
    public void CloseTrace_WithoutStart_IsSafe()
    {
        // Should not throw or modify the sink.
        var previous = AvaloniaLogger.Sink;
        BindingErrorTraceListener.CloseTrace();
        Assert.Same(previous, AvaloniaLogger.Sink);
    }
}