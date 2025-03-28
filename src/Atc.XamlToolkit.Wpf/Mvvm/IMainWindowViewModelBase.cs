namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Represents the base interface for main window view models in an WPF application.
/// </summary>
public interface IMainWindowViewModelBase : IViewModelBase
{
    /// <summary>
    /// Gets or sets the state of the window.
    /// </summary>
    /// <value>
    /// The state of the window.
    /// </value>
    WindowState WindowState { get; set; }

    /// <summary>
    /// Gets the application exit command.
    /// </summary>
    /// <value>
    /// The application exit command.
    /// </value>
    ICommand ApplicationExitCommand { get; }

    /// <summary>
    /// Called when loaded.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    void OnLoaded(object sender, RoutedEventArgs e);

    /// <summary>
    /// Called when closing.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
    void OnClosing(object sender, CancelEventArgs e);

    /// <summary>
    /// Called when key down.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    void OnKeyDown(object sender, KeyEventArgs e);

    /// <summary>
    /// Called when key up.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    void OnKeyUp(object sender, KeyEventArgs e);
}