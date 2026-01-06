namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Provides a base implementation for main window view models in an WPF application.
/// </summary>
public class MainWindowViewModelBase : ViewModelBase, IMainWindowViewModelBase
{
    private WindowState windowState;
    private ICommand? applicationExitCommand;

    protected virtual int ApplicationExitCode => 0;

    /// <inheritdoc />
    public WindowState WindowState
    {
        get => windowState;
        set
        {
            if (windowState == value)
            {
                return;
            }

            windowState = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    public ICommand ApplicationExitCommand
        => applicationExitCommand ??= new RelayCommand(ApplicationExitCommandHandler);

    /// <inheritdoc />
    public void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement &&
            (frameworkElement.Width >= SystemParameters.PrimaryScreenWidth ||
             frameworkElement.Height >= SystemParameters.PrimaryScreenHeight))
        {
            WindowState = WindowState.Maximized;
        }
    }

    /// <inheritdoc />
    public void OnClosing(
        object sender,
        CancelEventArgs e)
    {
        Application.Current.Shutdown(ApplicationExitCode);
    }

    /// <inheritdoc />
    public void OnKeyDown(
        object sender,
        KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (e.Key != Key.F11)
        {
            return;
        }

        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
        e.Handled = true;
    }

    /// <inheritdoc />
    public void OnKeyUp(
        object sender,
        KeyEventArgs e)
    {
        // Method intentionally left empty.
    }

    private void ApplicationExitCommandHandler()
    {
        OnClosing(this, new CancelEventArgs());
    }
}