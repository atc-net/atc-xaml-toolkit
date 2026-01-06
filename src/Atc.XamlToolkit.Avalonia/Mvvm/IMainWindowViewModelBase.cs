namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Represents the base interface for main window view models in a Avalonia application.
/// </summary>
public interface IMainWindowViewModelBase : IViewModelBase
{
    /// <summary>
    /// Gets or sets the state of the window.
    /// </summary>
    /// <value>
    /// The current window state.
    /// </value>
    WindowState WindowState { get; set; }

    /// <summary>
    /// Gets the command to exit the application.
    /// </summary>
    /// <value>
    /// The application exit command.
    /// </value>
    ICommand ApplicationExitCommand { get; }

    /// <summary>
    /// Called when the window is loaded.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event data.</param>
    void OnLoaded(
        object sender,
        EventArgs e);

    /// <summary>
    /// Called when the window is closing.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The cancel event data.</param>
    void OnClosing(
        object sender,
        CancelEventArgs e);

    /// <summary>
    /// Called when a key is pressed down.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyDown(
        object sender,
        KeyEventArgs e);

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The key event data.</param>
    void OnKeyUp(
        object sender,
        KeyEventArgs e);
}