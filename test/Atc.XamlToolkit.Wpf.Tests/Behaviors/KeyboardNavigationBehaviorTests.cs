namespace Atc.XamlToolkit.Wpf.Tests.Behaviors;

public sealed class KeyboardNavigationBehaviorTests
{
    [Fact]
    public void ChildControlGotFocus_DoesNotMoveKeyboardFocusToAssociatedObject()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var textBox = new System.Windows.Controls.TextBox();
            var container = new System.Windows.Controls.Border
            {
                Focusable = true,
                Child = textBox,
            };

            // The behavior is attached after the element is loaded, so that it has not focused the element yet
            var window = StaThreadRunner.ShowInWindow(container);
            try
            {
                var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
                behavior.Attach(container);
                var focusRequests = CountKeyboardFocusRequests(container);

                // Act - GotFocus bubbles from the child to the container
                textBox.RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent, textBox));

                // Assert
                Assert.Equal(0, focusRequests());
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void AssociatedObjectGotFocus_MovesKeyboardFocusToAssociatedObject()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var container = new System.Windows.Controls.Border
            {
                Focusable = true,
                Child = new System.Windows.Controls.TextBox(),
            };

            // The behavior is attached after the element is loaded, so that it has not focused the element yet
            var window = StaThreadRunner.ShowInWindow(container);
            try
            {
                var behavior = new Atc.XamlToolkit.Behaviors.KeyboardNavigationBehavior();
                behavior.Attach(container);
                var focusRequests = CountKeyboardFocusRequests(container);

                // Act
                container.RaiseEvent(new RoutedEventArgs(UIElement.GotFocusEvent, container));

                // Assert
                Assert.Equal(1, focusRequests());
            }
            finally
            {
                window.Close();
            }
        });

    // Counts the requests to give the element keyboard focus. The preview event is raised for every request,
    // also when the window is not the active one, so the tests do not depend on the desktop's focus state.
    private static Func<int> CountKeyboardFocusRequests(UIElement element)
    {
        var count = 0;
        element.PreviewGotKeyboardFocus += (_, e) =>
        {
            if (ReferenceEquals(e.NewFocus, element))
            {
                count++;
            }
        };

        return () => count;
    }
}