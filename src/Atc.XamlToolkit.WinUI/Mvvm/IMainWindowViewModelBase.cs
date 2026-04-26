namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Lifecycle and chrome contract for a WinUI 3 main-window view model.
/// </summary>
/// <remarks>
/// <para>
/// WinUI 3 has no <c>WindowState</c> property — chrome state is controlled through the
/// <see cref="Microsoft.UI.Windowing.AppWindow"/> presenter. The default
/// <see cref="OnLoaded"/> and <see cref="OnKeyDown"/> implementations resolve the
/// <c>AppWindow</c> from the host <c>Window</c> via <c>WindowNative.GetWindowHandle</c> +
/// <c>Win32Interop.GetWindowIdFromWindow</c> and call the presenter directly.
/// </para>
/// <para>
/// Wire window events as follows (typically via <c>EventToCommandBehavior</c> on the root
/// element or a code-behind one-liner):
/// <list type="table">
///   <listheader><term>Source event</term><description>Method</description></listheader>
///   <item><term>Window.Activated / FrameworkElement.Loaded</term><description><see cref="OnLoaded"/></description></item>
///   <item><term>Window.Closed</term><description><see cref="OnClosing"/></description></item>
///   <item><term>UIElement.KeyDown</term><description><see cref="OnKeyDown"/></description></item>
///   <item><term>UIElement.KeyUp</term><description><see cref="OnKeyUp"/></description></item>
/// </list>
/// </para>
/// </remarks>
public interface IMainWindowViewModelBase : IViewModelBase
{
    /// <summary>
    /// Bind to a Close / Exit menu item. Invokes <see cref="OnClosing"/>, which in turn calls
    /// <see cref="Microsoft.UI.Xaml.Application.Exit"/>. Override the protected
    /// <c>ApplicationExitCode</c> on the derived view model to return a non-zero exit code
    /// (note: WinUI's <c>Application.Exit()</c> ignores the code; the protected member is kept
    /// for parity with the WPF/Avalonia base classes).
    /// </summary>
    ICommand ApplicationExitCommand { get; }

    /// <summary>
    /// Wire to <c>Window.Activated</c> or <c>FrameworkElement.Loaded</c>. The default
    /// implementation auto-maximises when the window content is at least as large as the
    /// primary <see cref="Microsoft.UI.Windowing.DisplayArea"/>'s working area.
    /// </summary>
    /// <param name="sender">The sender — must be a <see cref="Microsoft.UI.Xaml.Window"/> for the auto-maximise check to run.</param>
    /// <param name="e">The routed event data.</param>
    void OnLoaded(
        object sender,
        RoutedEventArgs e);

    /// <summary>
    /// Wire to <c>Window.Closed</c>. The default implementation calls
    /// <see cref="Microsoft.UI.Xaml.Application.Exit"/>.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The cancel event data — note WinUI's <c>Window.Closed</c> uses <c>WindowEventArgs</c>; bridge to <see cref="CancelEventArgs"/> in the wiring code.</param>
    void OnClosing(
        object sender,
        CancelEventArgs e);

    /// <summary>
    /// Wire to <c>UIElement.KeyDown</c>. The default implementation toggles fullscreen on F11
    /// by switching the <see cref="Microsoft.UI.Windowing.OverlappedPresenter"/> between
    /// maximised and restored states.
    /// </summary>
    /// <param name="sender">The sender — must be a <see cref="Microsoft.UI.Xaml.Window"/> for F11 to work.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyDown(
        object sender,
        KeyRoutedEventArgs e);

    /// <summary>
    /// Wire to <c>UIElement.KeyUp</c>. The default implementation is a no-op extension point.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyUp(
        object sender,
        KeyRoutedEventArgs e);
}