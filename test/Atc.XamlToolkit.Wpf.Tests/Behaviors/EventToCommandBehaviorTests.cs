namespace Atc.XamlToolkit.Wpf.Tests.Behaviors;

public sealed class EventToCommandBehaviorTests
{
    [Fact]
    public void Attach_EventWithEventArgsHandler_ExecutesCommand()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new System.Windows.Controls.Button();
            var behavior = new Atc.XamlToolkit.Behaviors.EventToCommandBehavior
            {
                EventName = nameof(System.Windows.Controls.Button.Click),
                Command = new RelayCommand(() => executed++),
            };

            behavior.Attach(button);

            // Act
            button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

            // Assert
            Assert.Equal(1, executed);
        });

    [Fact]
    public void Attach_EventWithStructEventArgsHandler_ExecutesCommand()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new System.Windows.Controls.Button();
            var behavior = new Atc.XamlToolkit.Behaviors.EventToCommandBehavior
            {
                EventName = nameof(UIElement.IsEnabledChanged),
                Command = new RelayCommand(() => executed++),
            };

            // Act - IsEnabledChanged is a DependencyPropertyChangedEventHandler, whose event args is a struct
            behavior.Attach(button);
            button.IsEnabled = false;

            // Assert
            Assert.Equal(1, executed);
        });

    [Fact]
    public void Attach_EventWithStructEventArgsHandler_PassesEventArgsToCommand()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            object? received = null;
            var button = new System.Windows.Controls.Button();
            var behavior = new Atc.XamlToolkit.Behaviors.EventToCommandBehavior
            {
                EventName = nameof(FrameworkElement.DataContextChanged),
                Command = new RelayCommand<object>(parameter => received = parameter),
                PassEventArgsToCommand = true,
            };

            behavior.Attach(button);

            // Act
            button.DataContext = "context";

            // Assert
            var eventArgs = Assert.IsType<DependencyPropertyChangedEventArgs>(received);
            Assert.Equal("context", eventArgs.NewValue);
        });

    [Fact]
    public void Detach_EventWithStructEventArgsHandler_StopsExecutingCommand()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new System.Windows.Controls.Button();
            var behavior = new Atc.XamlToolkit.Behaviors.EventToCommandBehavior
            {
                EventName = nameof(UIElement.IsEnabledChanged),
                Command = new RelayCommand(() => executed++),
            };

            behavior.Attach(button);

            // Act
            behavior.Detach();
            button.IsEnabled = false;

            // Assert
            Assert.Equal(0, executed);
        });

    [Theory]
    [InlineData(nameof(System.Windows.Controls.Button.Click))]
    [InlineData(nameof(UIElement.IsEnabledChanged))]
    public void Attach_DerivedBehavior_ExecutesCommand(string eventName)
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new System.Windows.Controls.Button();
            var behavior = new DerivedEventToCommandBehavior
            {
                EventName = eventName,
                Command = new RelayCommand(() => executed++),
            };

            behavior.Attach(button);

            // Act
            button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            button.IsEnabled = false;

            // Assert
            Assert.Equal(1, executed);
        });

    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "The test verifies that a derived behavior works.")]
    private sealed class DerivedEventToCommandBehavior : Atc.XamlToolkit.Behaviors.EventToCommandBehavior;
}