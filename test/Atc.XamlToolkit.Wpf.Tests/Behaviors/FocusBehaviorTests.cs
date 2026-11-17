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
    public void FocusTrigger_RequestsKeyboardFocusForAssociatedObject()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var textBox = new System.Windows.Controls.TextBox();
            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            var window = StaThreadRunner.ShowInWindow(textBox);
            try
            {
                // The preview event is raised for every focus request, also when the window is not the active one.
                var focusRequests = 0;
                textBox.PreviewGotKeyboardFocus += (_, _) => focusRequests++;

                // Act
                behavior.FocusTrigger = new object();
                StaThreadRunner.PumpDispatcher();

                // Assert
                Assert.True(focusRequests > 0);
            }
            finally
            {
                window.Close();
            }
        });
}