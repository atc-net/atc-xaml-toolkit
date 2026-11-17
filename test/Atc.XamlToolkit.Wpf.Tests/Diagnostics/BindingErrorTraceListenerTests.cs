using BindingErrorEventArgs = Atc.XamlToolkit.Diagnostics.BindingErrorEventArgs;
using BindingErrorTraceListener = Atc.XamlToolkit.Diagnostics.BindingErrorTraceListener;

namespace Atc.XamlToolkit.Wpf.Tests.Diagnostics;

public sealed class BindingErrorTraceListenerTests : IDisposable
{
    private const string IgnoredMessage = "Cannot find governing FrameworkElement or FrameworkContentElement for target element. BindingExpression:Path=Fill; DataItem=null; target element is 'GeometryDrawing'";

    private readonly List<string> messages = [];
    private readonly bool originalShowMessageBoxOnError = BindingErrorTraceListener.ShowMessageBoxOnError;

    public BindingErrorTraceListenerTests()
    {
        BindingErrorTraceListener.CloseTrace();
        BindingErrorTraceListener.BindingErrorOccurred += OnBindingErrorOccurred;
    }

    public void Dispose()
    {
        BindingErrorTraceListener.BindingErrorOccurred -= OnBindingErrorOccurred;
        BindingErrorTraceListener.CloseTrace();
        BindingErrorTraceListener.ShowMessageBoxOnError = originalShowMessageBoxOnError;
    }

    [Fact]
    public void TraceEvent_RaisesBindingErrorOccurredWithTheMessage()
    {
        // Arrange
        BindingErrorTraceListener.StartTrace(SourceLevels.Error, TraceOptions.None, showMessageBoxOnError: false);

        // Act
        PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, "First error");

        // Assert
        var message = Assert.Single(messages);
        Assert.EndsWith("First error", message, StringComparison.Ordinal);
    }

    [Fact]
    public void TraceEvent_AfterAnIgnoredMessage_DoesNotIncludeTheIgnoredHeader()
    {
        // Arrange
        BindingErrorTraceListener.StartTrace(SourceLevels.Error, TraceOptions.None, showMessageBoxOnError: false);
        PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, "First error");
        var expected = messages[0].Replace("First error", "Second error", StringComparison.Ordinal);
        messages.Clear();

        // Act
        PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, IgnoredMessage);
        PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, "Second error");

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal(expected, message);
    }

    [Fact]
    public void TraceEvent_WithTraceOptions_RaisesBindingErrorOccurredOnce()
    {
        // Arrange
        BindingErrorTraceListener.StartTrace(
            SourceLevels.Error,
            TraceOptions.DateTime | TraceOptions.ThreadId,
            showMessageBoxOnError: false);

        // Act
        PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, "First error");

        // Assert
        var message = Assert.Single(messages);
        Assert.Contains("First error", message, StringComparison.Ordinal);
    }

    [Fact]
    public void TraceEvent_WithoutApplication_DoesNotThrow_WhenShowMessageBoxOnErrorIsSet()
    {
        // Arrange
        Assert.Null(Application.Current);
        BindingErrorTraceListener.StartTrace(SourceLevels.Error, TraceOptions.None, showMessageBoxOnError: true);

        // Act
        var exception = Record.Exception(
            () => PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 40, "First error"));

        // Assert
        Assert.Null(exception);
        Assert.Single(messages);
    }

    [Fact]
    public void FailingBinding_RaisesBindingErrorOccurred()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            BindingErrorTraceListener.StartTrace(SourceLevels.Error, TraceOptions.None, showMessageBoxOnError: false);
            var textBlock = new System.Windows.Controls.TextBlock
            {
                DataContext = new object(),
            };

            // Act
            textBlock.SetBinding(
                System.Windows.Controls.TextBlock.TextProperty,
                new Binding("PropertyThatDoesNotExist"));
            StaThreadRunner.PumpDispatcher();

            // Assert
            Assert.Contains(messages, x => x.Contains("PropertyThatDoesNotExist", StringComparison.Ordinal));
        });

    private void OnBindingErrorOccurred(
        object? sender,
        BindingErrorEventArgs e)
        => messages.Add(e.Message);
}