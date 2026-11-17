namespace Atc.XamlToolkit.Wpf.Tests.Behaviors;

public sealed class KeyboardNavigationBehaviorTests
{
    [Fact]
    public void FocusingChildControl_KeepsKeyboardFocusOnChild()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var textBox = new System.Windows.Controls.TextBox();
            var container = new System.Windows.Controls.Border
            {
                Focusable = true,
                Child = textBox,
            };

            var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
            behavior.Attach(container);

            var window = StaThreadRunner.ShowInWindow(container);
            try
            {
                // Act
                textBox.Focus();
                StaThreadRunner.PumpDispatcher();

                // Assert - the bubbling GotFocus of the child must not move focus to the container
                Assert.Same(textBox, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void FocusingAssociatedObject_SetsKeyboardFocus()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var container = new System.Windows.Controls.Border
            {
                Focusable = true,
                Child = new System.Windows.Controls.TextBox(),
            };

            var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
            behavior.Attach(container);

            var window = StaThreadRunner.ShowInWindow(container);
            try
            {
                // Act
                container.Focus();
                StaThreadRunner.PumpDispatcher();

                // Assert
                Assert.Same(container, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });
}