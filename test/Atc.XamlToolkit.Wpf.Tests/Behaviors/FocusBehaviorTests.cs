namespace Atc.XamlToolkit.Wpf.Tests.Behaviors;

public sealed class FocusBehaviorTests
{
    [Fact]
    public void DetachingBeforeDeferredFocusRuns_DoesNotThrowOnDispatcher()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var textBox = new System.Windows.Controls.TextBox();
            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            // Act - the focus request is queued on the dispatcher and the behavior is detached before it runs
            behavior.FocusTrigger = new object();
            behavior.Detach();
            var exception = Record.Exception(StaThreadRunner.PumpDispatcher);

            // Assert
            Assert.Null(exception);
        });

    [Fact]
    public void FocusTrigger_FocusesAssociatedObject()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var textBox = new System.Windows.Controls.TextBox();
            var other = new System.Windows.Controls.TextBox();
            var panel = new System.Windows.Controls.StackPanel();
            panel.Children.Add(other);
            panel.Children.Add(textBox);

            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            var window = StaThreadRunner.ShowInWindow(panel);
            try
            {
                other.Focus();

                // Act
                behavior.FocusTrigger = new object();
                StaThreadRunner.PumpDispatcher();

                // Assert
                Assert.Same(textBox, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });
}