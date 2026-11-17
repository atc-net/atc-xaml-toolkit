namespace Atc.XamlToolkit.Avalonia.Tests.Behaviors;

public sealed class KeyboardNavigationBehaviorTests
{
    [Fact]
    public void FocusingChildControl_KeepsFocusOnChild()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var textBox = new global::Avalonia.Controls.TextBox();
            var container = new global::Avalonia.Controls.Border
            {
                Focusable = true,
                Child = textBox,
            };

            var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
            behavior.Attach(container);

            var window = HeadlessRunner.ShowInWindow(container);
            try
            {
                // Act
                textBox.Focus();
                HeadlessRunner.PumpDispatcher();

                // Assert - the bubbling GotFocus of the child must not move focus to the container
                Assert.True(textBox.IsFocused);
                Assert.False(container.IsFocused);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void FocusingAssociatedObject_SetsFocus()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var container = new global::Avalonia.Controls.Border
            {
                Focusable = true,
                Child = new global::Avalonia.Controls.TextBox(),
            };

            var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
            behavior.Attach(container);

            var window = HeadlessRunner.ShowInWindow(container);
            try
            {
                // Act
                container.Focus();
                HeadlessRunner.PumpDispatcher();

                // Assert
                Assert.True(container.IsFocused);
            }
            finally
            {
                window.Close();
            }
        });
}