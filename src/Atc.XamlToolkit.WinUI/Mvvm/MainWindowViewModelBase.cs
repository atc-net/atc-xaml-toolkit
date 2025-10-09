// ReSharper disable InvertIf
namespace Atc.XamlToolkit.Mvvm
{
    /// <summary>
    /// Provides a base implementation for main window view models in a WinUI application.
    /// </summary>
    public class MainWindowViewModelBase : ViewModelBase, IMainWindowViewModelBase
    {
        private ICommand? applicationExitCommand;

        protected virtual int ApplicationExitCode => 0;

        /// <inheritdoc />
        public ICommand ApplicationExitCommand => applicationExitCommand ??= new RelayCommand(ApplicationExitCommandHandler);

        /// <inheritdoc />
        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window)
            {
                return;
            }

            // Get the AppWindow for the current window
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow?.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
            {
                var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                    windowId,
                    Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

                if (displayArea is not null &&
                    window.Content is FrameworkElement content &&
                    (content.ActualWidth >= displayArea.WorkArea.Width ||
                     content.ActualHeight >= displayArea.WorkArea.Height))
                {
                    presenter.Maximize();
                }
            }
        }

        /// <inheritdoc />
        public void OnClosing(
            object sender,
            CancelEventArgs e)
        {
            Application.Current.Exit();
        }

        /// <inheritdoc />
        public void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e);

            if (e.Key != Windows.System.VirtualKey.F11)
            {
                return;
            }

            // Toggle between maximized and normal state
            if (sender is Window window)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow?.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    if (presenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized)
                    {
                        presenter.Restore();
                    }
                    else
                    {
                        presenter.Maximize();
                    }
                }
            }

            e.Handled = true;
        }

        /// <inheritdoc />
        public void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Method intentionally left empty.
        }

        private void ApplicationExitCommandHandler()
        {
            OnClosing(this, new CancelEventArgs());
        }
    }
}