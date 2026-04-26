// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local
namespace Atc.XamlToolkit.Wpf.Tests.Command;

[SuppressMessage("AsyncUsage", "AsyncFixer06:Task<T> to Task conversion silently discards result", Justification = "OK - Tests")]
public sealed class RelayCommandAsyncGenericTests
{
    [Theory]
    [InlineData(0, true, 0)]
    [InlineData(1, true, 1)]
    [InlineData(2, true, 2)]
    [InlineData(0, false, 0)]
    [InlineData(1, false, 1)]
    [InlineData(2, false, 2)]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    public void RaiseCanExecuteChanged(
        int expected,
        bool canExecute,
        int registerOnChangeCount)
    {
        // Arrange
        var canExecuteChangedCalled = 0;
        var canExecuteChangedEventHandler = new EventHandler((_, _) => canExecuteChangedCalled++);

        using var command = new RelayCommandAsync<string>(_ => MyTask(), _ => canExecute);

        for (var i = 0; i < registerOnChangeCount; i++)
        {
            command.CanExecuteChanged += canExecuteChangedEventHandler;
        }

        // Act
        command.RaiseCanExecuteChanged();

        // Forces processing of CommandManager.InvalidateRequerySuggested by dispatching an empty action at background priority.
        _ = Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        // Assert
        Assert.Equal(expected, canExecuteChangedCalled);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void CanExecute(
        bool expected,
        bool canExecute)
    {
        // Arrange
        using var command = new RelayCommandAsync<string>(_ => MyTask(), _ => canExecute);

        // Act
        var actual = command.CanExecute(parameter: null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("Executed", true, null)]
    [InlineData("Not executed", false, null)]
    [InlineData("Executed", true, 42)]
    [InlineData("Not executed", false, 42)]
    public async Task Execute(
        string expected,
        bool canExecute,
        object? parameter)
    {
        // Arrange
        var actual = "Not executed";

        using var command = new RelayCommandAsync<string>(_ => MyTask(expected, op => actual = op), _ => canExecute);

        // Act
        await command.ExecuteAsync(parameter?.ToString());

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Execute_DoesNotThrow_WhenParameterCannotBeConverted()
    {
        // Regression: Convert.ChangeType used to throw OverflowException /
        // FormatException / InvalidCastException uncaught from inside the
        // async void Execute, propagating to AppDomain.UnhandledException
        // and risking a crash because there is no awaiter.
        var executed = false;
        using var command = new RelayCommandAsync<int>(
            _ =>
            {
                executed = true;
                return Task.CompletedTask;
            });

        // ulong.MaxValue overflows int — Convert.ChangeType throws OverflowException.
        var act = () => ((ICommand)command).Execute(ulong.MaxValue);

        act.Should().NotThrow();
        executed.Should().BeFalse("execution must be skipped when parameter conversion fails");
    }

    [Fact]
    public void Execute_RoutesConversionError_ThroughErrorHandler()
    {
        var handled = (Exception?)null;
        var errorHandler = Substitute.For<IErrorHandler>();
        errorHandler
            .When(x => x.HandleError(Arg.Any<Exception>()))
            .Do(call => handled = call.Arg<Exception>());

        using var command = new RelayCommandAsync<int>(
            _ => Task.CompletedTask,
            errorHandler: errorHandler);

        ((ICommand)command).Execute(ulong.MaxValue);

        handled.Should().NotBeNull();
        handled.Should().BeOfType<OverflowException>();
    }

    private delegate void OpDelegate(string op);

    private static async Task<string> MyTask()
    {
        await Task.Delay(1);
        return "Hello";
    }

    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "OK.")]
    [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "OK.")]
    [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "OK.")]
    private static async Task<string> MyTask(
        string expected,
        OpDelegate callback)
    {
        await Task.Delay(1);
        callback(expected);
        return "Hello";
    }
}