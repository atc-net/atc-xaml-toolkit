namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Lifecycle and chrome contract for a WPF main-window view model.
/// </summary>
/// <remarks>
/// Wire the corresponding window events to the matching method (typically via
/// <c>EventToCommandBehavior</c> or a code-behind one-liner):
/// <list type="table">
///   <listheader><term>Window event</term><description>Method</description></listheader>
///   <item><term>Loaded</term><description><see cref="OnLoaded"/></description></item>
///   <item><term>Closing</term><description><see cref="OnClosing"/></description></item>
///   <item><term>KeyDown</term><description><see cref="OnKeyDown"/></description></item>
///   <item><term>KeyUp</term><description><see cref="OnKeyUp"/></description></item>
/// </list>
/// Bind <c>WindowState</c> two-way on the <c>Window</c> element to keep the chrome state
/// in sync with the view model.
/// </remarks>
public interface IMainWindowViewModelBase : IViewModelBase
{
    /// <summary>
    /// Two-way bound to the host <see cref="System.Windows.Window.WindowState"/>. The default
    /// implementation toggles between <see cref="System.Windows.WindowState.Normal"/> and
    /// <see cref="System.Windows.WindowState.Maximized"/> on F11 (see <see cref="OnKeyDown"/>) and
    /// auto-maximises in <see cref="OnLoaded"/> when the window meets or exceeds the primary
    /// screen's working area.
    /// </summary>
    WindowState WindowState { get; set; }

    /// <summary>
    /// Bind to a Close / Exit menu item. Invokes <see cref="OnClosing"/>, which in turn shuts the
    /// app down with the value returned by the protected <c>ApplicationExitCode</c> property
    /// (override on the derived view model to set a non-zero exit code).
    /// </summary>
    ICommand ApplicationExitCommand { get; }

    /// <summary>
    /// Wire to <c>FrameworkElement.Loaded</c>. The default implementation auto-maximises when the
    /// host element is at least as large as the primary screen — useful for full-screen kiosk apps.
    /// </summary>
    /// <param name="sender">The sender (typically the main window).</param>
    /// <param name="e">The routed event data.</param>
    void OnLoaded(
        object sender,
        RoutedEventArgs e);

    /// <summary>
    /// Wire to <c>Window.Closing</c>. The default implementation calls
    /// <see cref="System.Windows.Application.Shutdown(int)"/> with the protected
    /// <c>ApplicationExitCode</c>.
    /// </summary>
    /// <param name="sender">The sender (typically the main window).</param>
    /// <param name="e">The cancel event data — set <c>e.Cancel = true</c> to abort shutdown.</param>
    void OnClosing(
        object sender,
        CancelEventArgs e);

    /// <summary>
    /// Wire to <c>Window.KeyDown</c>. The default implementation toggles fullscreen on F11.
    /// Override to add additional global shortcuts; call <c>base.OnKeyDown</c> first to keep F11.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyDown(
        object sender,
        KeyEventArgs e);

    /// <summary>
    /// Wire to <c>Window.KeyUp</c>. The default implementation is a no-op extension point.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyUp(
        object sender,
        KeyEventArgs e);
}