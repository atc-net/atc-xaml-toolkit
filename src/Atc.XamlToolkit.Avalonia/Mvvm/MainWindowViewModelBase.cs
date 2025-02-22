// ReSharper disable InvertIf
namespace Atc.XamlToolkit.Mvvm
{
    /// <summary>
    /// Provides a base implementation for main window view models in an Avalonia application.
    /// </summary>
    public class MainWindowViewModelBase : ViewModelBase, IMainWindowViewModelBase
    {
        private WindowState windowState;

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
        public ICommand ApplicationExitCommand => new RelayCommand(ApplicationExitCommandHandler);

        /// <inheritdoc />
        public void OnLoaded(object sender, EventArgs e)
        {
            if (sender is Control control &&
                Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var screens = desktopLifetime.MainWindow?.Screens;
                if (screens?.Primary is { } primaryScreen &&
                    (control.Bounds.Width >= primaryScreen.WorkingArea.Width ||
                     control.Bounds.Height >= primaryScreen.WorkingArea.Height))
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        /// <inheritdoc />
        public void OnClosing(object sender, CancelEventArgs e)
        {
            //// TODO:
            ////// Shutdown the application via the classic desktop style application lifetime.
            ////if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            ////{
            ////    desktopLifetime.Shutdown(-1);
            ////}
        }

        /// <inheritdoc />
        public void OnKeyDown(object sender, KeyEventArgs e)
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
        public void OnKeyUp(object sender, KeyEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void ApplicationExitCommandHandler()
        {
            OnClosing(this, new CancelEventArgs());
        }
    }
}