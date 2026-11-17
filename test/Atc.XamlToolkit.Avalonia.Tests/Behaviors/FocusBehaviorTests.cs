namespace Atc.XamlToolkit.Avalonia.Tests.Behaviors;

public sealed class FocusBehaviorTests
{
    [Fact]
    public void DetachingBeforeDeferredFocusRuns_DoesNotFailOnDispatcher()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var failures = new List<Exception>();
            EventHandler<UnobservedTaskExceptionEventArgs> handler = (_, e) =>
            {
                if (e.Exception.ToString().Contains(nameof(Atc.XamlToolkit.Behaviors.FocusBehavior), StringComparison.Ordinal))
                {
                    failures.Add(e.Exception);
                }
            };
            var textBox = new global::Avalonia.Controls.TextBox();
            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            TaskScheduler.UnobservedTaskException += handler;
            try
            {
                // Act - the focus request is queued on the dispatcher and the behavior is detached before it runs
                behavior.FocusTrigger = new object();
                behavior.Detach();
                var exception = Record.Exception(HeadlessRunner.PumpDispatcher);
                CollectGarbage();

                // Assert
                Assert.Null(exception);
                Assert.Empty(failures);
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= handler;
            }
        });

    [Fact]
    public void FocusTrigger_FocusesAssociatedObject()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var textBox = new global::Avalonia.Controls.TextBox();
            var other = new global::Avalonia.Controls.TextBox();
            var panel = new global::Avalonia.Controls.StackPanel();
            panel.Children.Add(other);
            panel.Children.Add(textBox);

            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            var window = HeadlessRunner.ShowInWindow(panel);
            try
            {
                other.Focus();

                // Act
                behavior.FocusTrigger = new object();
                HeadlessRunner.PumpDispatcher();

                // Assert
                Assert.True(textBox.IsFocused);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void FocusTrigger_FocusesAgain_WhenSetToTheSameValueAfterNull()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var trigger = new object();
            var textBox = new global::Avalonia.Controls.TextBox();
            var other = new global::Avalonia.Controls.TextBox();
            var panel = new global::Avalonia.Controls.StackPanel();
            panel.Children.Add(other);
            panel.Children.Add(textBox);

            var behavior = new Atc.XamlToolkit.Behaviors.FocusBehavior();
            behavior.Attach(textBox);

            var window = HeadlessRunner.ShowInWindow(panel);
            try
            {
                behavior.FocusTrigger = trigger;
                HeadlessRunner.PumpDispatcher();
                Assert.True(textBox.IsFocused);
                other.Focus();
                Assert.False(textBox.IsFocused);

                // Act
                behavior.FocusTrigger = null;
                behavior.FocusTrigger = trigger;
                HeadlessRunner.PumpDispatcher();

                // Assert
                Assert.True(textBox.IsFocused);
            }
            finally
            {
                window.Close();
            }
        });

    [SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "The test verifies that no faulted dispatcher operation is left unobserved.")]
    private static void CollectGarbage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}