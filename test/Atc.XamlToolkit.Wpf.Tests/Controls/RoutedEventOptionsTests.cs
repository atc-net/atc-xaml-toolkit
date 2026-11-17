namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for the <c>[RoutedEvent]</c> options against real WPF.
/// </summary>
public sealed class RoutedEventOptionsTests
{
    [Fact]
    public void GeneratedRaiseMethod_ForRoutedEventHandler_RaisesTheEventWithTheControlAsSource()
        => StaThreadRunner.Run(() =>
        {
            var sut = new RoutedEventControl();
            System.Windows.RoutedEventArgs? received = null;
            sut.Opened += (_, e) => received = e;

            sut.Open();

            Assert.NotNull(received);
            Assert.Same(RoutedEventControl.OpenedEvent, received.RoutedEvent);
            Assert.Same(sut, received.Source);
        });

    [Fact]
    public void GeneratedRaiseMethod_ForRoutedPropertyChangedEventHandler_PassesOldAndNewValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new RoutedEventControl();
            System.Windows.RoutedPropertyChangedEventArgs<int>? received = null;
            sut.ValueChanged += (_, e) => received = e;

            sut.ChangeValue(oldValue: 2, newValue: 5);

            Assert.NotNull(received);
            Assert.Equal(2, received.OldValue);
            Assert.Equal(5, received.NewValue);
            Assert.Equal(System.Windows.RoutingStrategy.Direct, RoutedEventControl.ValueChangedEvent.RoutingStrategy);
        });

    [Fact]
    public void GeneratedRaiseMethod_ForEventHandlerOfArgs_SetsTheRoutedEventOnTheArgs()
        => StaThreadRunner.Run(() =>
        {
            var sut = new RoutedEventControl();
            RoutedEventTestEventArgs? received = null;
            sut.DeviceError += (_, e) => received = e;

            sut.Fail("no device");

            Assert.NotNull(received);
            Assert.Equal("no device", received.Message);
            Assert.Same(RoutedEventControl.DeviceErrorEvent, received.RoutedEvent);
        });

    [Fact]
    public void AddOwnerOf_SharesTheEventOfTheOtherClass()
        => Assert.Same(SharedRoutedEvents.DeviceErrorEvent, RoutedEventControl.DeviceErrorEvent);

    [Fact]
    public void AStaticClass_RegistersTheEventUnderItsName()
    {
        Assert.Equal("DeviceError", SharedRoutedEvents.DeviceErrorEvent.Name);
        Assert.Equal(typeof(SharedRoutedEvents), SharedRoutedEvents.DeviceErrorEvent.OwnerType);
        Assert.Equal(typeof(EventHandler<RoutedEventTestEventArgs>), SharedRoutedEvents.DeviceErrorEvent.HandlerType);
    }

    [Fact]
    public void AStaticClass_AddHandler_ReceivesTheEventBubblingFromAChild()
        => StaThreadRunner.Run(() =>
        {
            var sut = new RoutedEventControl();
            var parent = new System.Windows.Controls.Border { Child = sut };
            var messages = new List<string>();
            EventHandler<RoutedEventTestEventArgs> handler = (_, e) => messages.Add(e.Message);

            SharedRoutedEvents.AddDeviceErrorHandler(parent, handler);
            sut.Fail("first");

            SharedRoutedEvents.RemoveDeviceErrorHandler(parent, handler);
            sut.Fail("second");

            Assert.Equal(["first"], messages);
        });
}