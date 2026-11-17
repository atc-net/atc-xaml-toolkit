namespace Atc.XamlToolkit.Wpf.Tests.Command;

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

        // Forces processing of CommandManager.InvalidateRequerySuggested by dispatching an empty action at background priority.
        _ = Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        // Assert
        Assert.Equal(expected, canExecuteChangedCalled);
    }

    [Fact]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    [SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "The test verifies that the command keeps its subscribers alive.")]
    public void RaiseCanExecuteChanged_NotifiesSubscriber_WhenOnlyTheCommandReferencesTheHandler()
    {
        // Arrange
        var canExecuteChangedCalled = new int[1];
        var command = new RelayCommand<string>(_ => { }, _ => true, keepTargetAlive: true);
        SubscribeCountingHandler(command, canExecuteChangedCalled);

        // CommandManager.RequerySuggested only holds weak references to its handlers
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Act
        command.RaiseCanExecuteChanged();

        // Forces processing of CommandManager.InvalidateRequerySuggested by dispatching an empty action at background priority.
        _ = Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

        // Assert
        Assert.Equal(1, canExecuteChangedCalled[0]);
        GC.KeepAlive(command);
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

    [Fact]
    public void Execute_DoesNotThrow_WhenParameterCannotBeConverted()
    {
        // Regression: Convert.ChangeType used to throw OverflowException /
        // FormatException / InvalidCastException uncaught from inside
        // Execute, propagating up through the binding system.
        var executed = false;
        var command = new RelayCommand<int>(_ => executed = true);

        // ulong.MaxValue overflows int — Convert.ChangeType throws OverflowException.
        var act = () => command.Execute(ulong.MaxValue);

        act.Should().NotThrow();
        executed.Should().BeFalse("execution must be skipped when parameter conversion fails");
    }

    [Theory]
    [InlineData("99")]
    [InlineData(99)]
    [InlineData("NotADay")]
    public void Execute_DoesNotRun_WhenTheParameterIsNotAValueOfTheEnum(
        object parameter)
    {
        var executed = false;
        var command = new RelayCommand<DayOfWeek>(_ => executed = true, _ => true);

        command.Execute(parameter);

        executed.Should().BeFalse("the parameter is not a day of the week");
        command.CanExecute(parameter).Should().BeFalse("a command with a predicate cannot run with a parameter it cannot convert");
    }

    [Theory]
    [InlineData("5", DayOfWeek.Friday)]
    [InlineData(5, DayOfWeek.Friday)]
    [InlineData("Friday", DayOfWeek.Friday)]
    public void Execute_ConvertsTextAndNumbers_ToADefinedEnumValue(
        object parameter,
        DayOfWeek expected)
    {
        DayOfWeek? actual = null;
        var command = new RelayCommand<DayOfWeek>(value => actual = value);

        command.Execute(parameter);

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("3")]
    [InlineData(3)]
    [InlineData("Assembly, Module")]
    public void Execute_AcceptsACombinationOfFlags(object parameter)
    {
        AttributeTargets? actual = null;
        var command = new RelayCommand<AttributeTargets>(value => actual = value);

        command.Execute(parameter);

        actual.Should().Be(AttributeTargets.Assembly | AttributeTargets.Module);
    }

    [Fact]
    public void Execute_DoesNotThrow_WhenParameterFormatIsInvalid()
    {
        var executed = false;
        var command = new RelayCommand<int>(_ => executed = true);

        // "not a number" cannot be converted to int — Convert.ChangeType throws FormatException.
        var act = () => command.Execute("not a number");

        act.Should().NotThrow();
        executed.Should().BeFalse();
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

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void SubscribeCountingHandler(
        RelayCommand<string> command,
        int[] counter)
        => command.CanExecuteChanged += (_, _) => counter[0]++;
}