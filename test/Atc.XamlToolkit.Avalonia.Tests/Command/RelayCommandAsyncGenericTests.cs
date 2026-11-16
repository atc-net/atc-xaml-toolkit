// ReSharper disable RedundantAssignment
// ReSharper disable UnusedParameter.Local
namespace Atc.XamlToolkit.Avalonia.Tests.Command;

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

    [Theory]
    [InlineData(true, "go")]
    [InlineData(false, "stop")]
    [InlineData(false, null)]
    public void CanExecute_PassesParameter(
        bool expected,
        string? parameter)
    {
        // Arrange
        using var command = new RelayCommandAsync<string>(_ => MyTask(), text => text == "go");

        // Act
        var actual = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, 42)]
    [InlineData(false, 7)]
    [InlineData(false, null)]
    public void CanExecute_PassesParameter_ValueType(
        bool expected,
        object? parameter)
    {
        // Arrange
        using var command = new RelayCommandAsync<int>(_ => MyTask(), number => number == 42);

        // Act
        var actual = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, "42")]
    [InlineData(false, "7")]
    [InlineData(false, "abc")]
    public void CanExecute_ConvertsParameter(
        bool expected,
        string parameter)
    {
        // Arrange
        using var command = new RelayCommandAsync<int>(_ => MyTask(), number => number == 42);

        // Act
        var actual = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(true, "go")]
    [InlineData(false, "stop")]
    public void Execute_UsesCanExecuteWithParameter(
        bool expected,
        string parameter)
    {
        // Arrange
        var executed = false;
        using var command = new RelayCommandAsync<string>(
            _ =>
            {
                executed = true;
                return Task.CompletedTask;
            },
            text => text == "go");

        // Act
        command.Execute(parameter);

        // Assert
        Assert.Equal(expected, executed);
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