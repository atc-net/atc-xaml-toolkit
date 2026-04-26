namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Lifecycle and chrome contract for an Avalonia main-window view model.
/// </summary>
/// <remarks>
/// Wire the corresponding window/control events to the matching method (typically via
/// <c>EventToCommandBehavior</c> or a code-behind one-liner):
/// <list type="table">
///   <listheader><term>Source event</term><description>Method</description></listheader>
///   <item><term>Control.Loaded / Window.Opened</term><description><see cref="OnLoaded"/></description></item>
///   <item><term>Window.Closing</term><description><see cref="OnClosing"/></description></item>
///   <item><term>InputElement.KeyDown</term><description><see cref="OnKeyDown"/></description></item>
///   <item><term>InputElement.KeyUp</term><description><see cref="OnKeyUp"/></description></item>
/// </list>
/// Bind <c>WindowState</c> two-way on the host <c>Window</c> to keep the chrome state in sync
/// with the view model.
/// </remarks>
public interface IMainWindowViewModelBase : IViewModelBase
{
    /// <summary>
    /// Two-way bound to the host <see cref="Avalonia.Controls.Window.WindowState"/>. The default
    /// implementation toggles between <see cref="Avalonia.Controls.WindowState.Normal"/> and
    /// <see cref="Avalonia.Controls.WindowState.Maximized"/> on F11 (see <see cref="OnKeyDown"/>)
    /// and auto-maximises in <see cref="OnLoaded"/> when the host control is at least as large as
    /// the primary screen's working area.
    /// </summary>
    WindowState WindowState { get; set; }

    /// <summary>
    /// Bind to a Close / Exit menu item. Invokes <see cref="OnClosing"/>, which calls
    /// <see cref="IClassicDesktopStyleApplicationLifetime.TryShutdown(int)"/> with the protected
    /// <c>ApplicationExitCode</c> (override on the derived view model to set a non-zero code).
    /// </summary>
    ICommand ApplicationExitCommand { get; }

    /// <summary>
    /// Wire to <c>Control.Loaded</c> or <c>Window.Opened</c>. The default implementation
    /// auto-maximises when the host control's <c>Bounds</c> match or exceed the primary screen's
    /// working area.
    /// </summary>
    /// <param name="sender">The sender — typically the main window or its content control.</param>
    /// <param name="e">The event data.</param>
    void OnLoaded(
        object sender,
        EventArgs e);

    /// <summary>
    /// Wire to <c>Window.Closing</c>. The default implementation requests application shutdown
    /// via <see cref="IClassicDesktopStyleApplicationLifetime.TryShutdown(int)"/>; if shutdown is
    /// vetoed (e.g., by another window's Closing handler), the call is a no-op.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The cancel event data — set <c>e.Cancel = true</c> to abort shutdown.</param>
    void OnClosing(
        object sender,
        CancelEventArgs e);

    /// <summary>
    /// Wire to <c>InputElement.KeyDown</c>. The default implementation toggles fullscreen on F11
    /// by flipping <see cref="WindowState"/>. Override to add additional global shortcuts; call
    /// <c>base.OnKeyDown</c> first to keep F11.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyDown(
        object sender,
        KeyEventArgs e);

    /// <summary>
    /// Wire to <c>InputElement.KeyUp</c>. The default implementation is a no-op extension point.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyUp(
        object sender,
        KeyEventArgs e);
}