namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Defects in generated code found in review, run against real WPF.
/// </summary>
public sealed class ReviewFindingsTests
{
    [Fact]
    public void HooksOnAnEnumOrStructProperty_DoNotBreakTheRegistration()
        => StaThreadRunner.Run(() =>
        {
            // Registering 'defaultValue: null' for a value type throws in the type initializer.
            var sut = new ReviewFindingsControl();

            Assert.Equal(ReviewFindingsMode.One, sut.Mode);
            Assert.Equal(System.Windows.Input.Key.None, sut.Key);

            sut.Mode = ReviewFindingsMode.Two;

            Assert.Equal(1, sut.ModeChanges);
        });

    [Fact]
    public void ACoerceCallbackThatRejectsTheValue_KeepsThePreviousValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ReviewFindingsControl { Amount = 5 };

            sut.Amount = -1;

            Assert.Equal(5d, sut.Amount);
        });

    [Fact]
    public void ACoerceCallbackThatAcceptsTheValue_HandsItToTheTypedHook()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ReviewFindingsControl { Amount = 250 };

            Assert.Equal(100d, sut.Amount);
        });

    [Fact]
    public void ARoutedEvent_WithATupleInItsHandlerType_HasThatHandlerType()
        => Assert.Equal(
            typeof(RoutedPropertyChangedEventHandler<(int A, string B)>),
            ReviewFindingsControl.PairChangedEvent.HandlerType);

    [Fact]
    public void ARoutedEvent_WithNullArguments_FallsBackToTheDefaults()
    {
        Assert.Equal(typeof(RoutedEventHandler), ReviewFindingsControl.PlainEvent.HandlerType);
        Assert.Equal(typeof(ReviewFindingsControl), ReviewFindingsControl.PlainEvent.OwnerType);
    }
}