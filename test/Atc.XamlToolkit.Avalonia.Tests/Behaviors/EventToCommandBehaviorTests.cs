namespace Atc.XamlToolkit.Avalonia.Tests.Behaviors;

public sealed class EventToCommandBehaviorTests
{
    [Fact]
    public void Attach_ExecutesCommand_WhenEventIsRaised()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new global::Avalonia.Controls.Button();
            var behavior = new Atc.XamlToolkit.Behaviors.EventToCommandBehavior
            {
                EventName = nameof(global::Avalonia.Controls.Button.Click),
                Command = new RelayCommand(() => executed++),
            };

            behavior.Attach(button);

            // Act
            button.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));

            // Assert
            Assert.Equal(1, executed);
        });

    [Fact]
    public void Attach_DerivedBehavior_ExecutesCommand()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var executed = 0;
            var button = new global::Avalonia.Controls.Button();
            var behavior = new DerivedEventToCommandBehavior
            {
                EventName = nameof(global::Avalonia.Controls.Button.Click),
                Command = new RelayCommand(() => executed++),
            };

            behavior.Attach(button);

            // Act
            button.RaiseEvent(new global::Avalonia.Interactivity.RoutedEventArgs(global::Avalonia.Controls.Button.ClickEvent));

            // Assert
            Assert.Equal(1, executed);
        });

    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty", Justification = "The test verifies that a derived behavior works.")]
    private sealed class DerivedEventToCommandBehavior : Atc.XamlToolkit.Behaviors.EventToCommandBehavior;
}