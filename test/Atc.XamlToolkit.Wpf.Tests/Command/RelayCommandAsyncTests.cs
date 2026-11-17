// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local
namespace Atc.XamlToolkit.Wpf.Tests.Command;

public sealed class RelayCommandAsyncTests
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

        using var command = new RelayCommandAsync(MyTask, () => canExecute);

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

    [Fact]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    public void RaiseCanExecuteChanged_WithoutCanExecute_NotifiesSubscribers()
    {
        // Arrange
        var canExecuteChangedCalled = 0;
        var canExecuteChangedEventHandler = new EventHandler((_, _) => canExecuteChangedCalled++);

        using var command = new RelayCommandAsync(MyTask);
        command.CanExecuteChanged += canExecuteChangedEventHandler;

        // Act
        command.RaiseCanExecuteChanged();

        // Forces processing of CommandManager.InvalidateRequerySuggested by dispatching an empty action at background priority.
        _ = Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        // Assert
        Assert.Equal(1, canExecuteChangedCalled);
    }

    [Theory]
    [InlineData(true, true, null)]
    [InlineData(false, false, null)]
    [InlineData(true, true, 42)]
    [InlineData(false, false, 42)]
    public void CanExecute(
        bool expected,
        bool canExecute,
        object? parameter)
    {
        // Arrange
        using var command = new RelayCommandAsync(MyTask, () => canExecute);

        // Act
        var actual = command.CanExecute(parameter);

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

        using var command = new RelayCommandAsync(() => MyTask(ref actual, ref expected), () => canExecute);

        // Act
        await command.ExecuteAsync(parameter);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ExecuteAsync_SetsIsExecuting_WhileRunning()
    {
        // Arrange
        var gate = new TaskCompletionSource();
        var isExecutingChangedCount = 0;

        using var command = new RelayCommandAsync(() => gate.Task);
        command.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "IsExecuting")
            {
                isExecutingChangedCount++;
            }
        };

        // Act
        var task = command.ExecuteAsync(null);

        // Assert
        Assert.True(command.IsExecuting);
        Assert.False(command.CanExecute(null));

        gate.SetResult();
        await task;

        Assert.False(command.IsExecuting);
        Assert.True(command.CanExecute(null));
        Assert.Equal(2, isExecutingChangedCount);
    }

    [Fact]
    public async Task Execute_IsIgnored_WhileExecuteAsyncRuns()
    {
        // Arrange
        var gate = new TaskCompletionSource();
        var executeCount = 0;

        using var command = new RelayCommandAsync(() =>
        {
            executeCount++;
            return gate.Task;
        });

        // Act
        var task = command.ExecuteAsync(null);
        ((System.Windows.Input.ICommand)command).Execute(null);

        gate.SetResult();
        await task;

        // Assert
        Assert.Equal(1, executeCount);
    }

    [Fact]
    public async Task ExecuteAsync_CancelThenRestart_CancelsOnlyPreviousRun()
    {
        // Arrange
        var gate = new TaskCompletionSource();
        var tokens = new List<CancellationToken>();

        using var command = new RelayCommandAsync(cancellationToken =>
        {
            tokens.Add(cancellationToken);
            return gate.Task;
        });

        // Act
        var first = command.ExecuteAsync(null);
        command.Cancel();
        var second = command.ExecuteAsync(null);

        // Assert
        Assert.True(tokens[0].IsCancellationRequested);
        Assert.False(tokens[1].IsCancellationRequested);
        Assert.True(command.IsExecuting);

        gate.SetResult();
        await Task.WhenAll(first, second);

        Assert.False(command.IsExecuting);
    }

    [Fact]
    public async Task Cancel_CancelsEveryRunningExecution_WhenRunsOverlap()
    {
        // Arrange
        var gate = new TaskCompletionSource();
        var tokens = new List<CancellationToken>();

        using var command = new RelayCommandAsync(cancellationToken =>
        {
            tokens.Add(cancellationToken);
            return gate.Task;
        });

        var first = command.ExecuteAsync(null);
        var second = command.ExecuteAsync(null);

        // Act
        command.Cancel();

        // Assert
        Assert.True(tokens[0].IsCancellationRequested);
        Assert.True(tokens[1].IsCancellationRequested);

        gate.SetResult();
        await Task.WhenAll(first, second);
    }

    private static Task MyTask()
        => Task.Delay(1);

    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "OK.")]
    [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed", Justification = "OK.")]
    [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "OK.")]
    private static Task MyTask(
        ref string actual,
        ref string expected)
    {
        actual = expected;
        return Task.Delay(1);
    }
}