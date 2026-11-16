namespace Atc.XamlToolkit.Avalonia.Tests.Command;

public sealed class RelayCommandGenericTests
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

        var command = new RelayCommand<string>(_ => { }, _ => canExecute);

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
        var command = new RelayCommand<string>(_ => { }, _ => canExecute);

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
    public void Execute(
        string expected,
        bool canExecute,
        object? parameter)
    {
        // Arrange
        var actual = "Not executed";

        var command = new RelayCommand<string>(_ => { actual = expected; }, _ => canExecute);

        // Act
        command.Execute(parameter);

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
        var command = new RelayCommand<int>(_ => { }, number => number == 42);

        // Act
        var actual = command.CanExecute(parameter);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Execute_ConvertsStringParameter_WithInvariantCulture()
    {
        // Arrange - a XAML CommandParameter literal is culture-invariant text.
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("da-DK");

        try
        {
            var actual = 0d;
            var command = new RelayCommand<double>(value => actual = value);

            // Act
            command.Execute("1.5");

            // Assert
            Assert.Equal(1.5, actual);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Execute_ConvertsStringParameter_ToEnum()
    {
        // Arrange
        DayOfWeek? actual = null;
        var command = new RelayCommand<DayOfWeek>(value => actual = value);

        // Act
        command.Execute("Friday");

        // Assert
        Assert.Equal(DayOfWeek.Friday, actual);
    }

    [Fact]
    public void Execute_ConvertsStringParameter_ToNullableValueType()
    {
        // Arrange
        int? actual = null;
        var command = new RelayCommand<int?>(value => actual = value);

        // Act
        command.Execute("5");

        // Assert
        Assert.Equal(5, actual);
    }

    [Fact]
    public void Execute_IsIgnored_WhenParameterHasWrongType_AndNoCanExecute()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand<string>(_ => executed = true);

        // Act
        var exception = Record.Exception(() => command.Execute(new object()));

        // Assert
        Assert.Null(exception);
        Assert.False(executed);
    }
}