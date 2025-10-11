// ReSharper disable AsyncVoidMethod
// ReSharper disable InvertIf
// ReSharper disable RedundantExtendsListEntry
// ReSharper disable StringLiteralTypo
namespace Atc.XamlToolkit.WinUISample;

public partial class App : Application
{
    private readonly IHost host;
    private Window? window;

    public static Window? MainWindow { get; private set; }

    [SuppressMessage("Usage", "CA1024:Use properties where appropriate", Justification = "OK")]
    [SupportedOSPlatform("windows10.0.17763.0")]
    public static XamlRoot? GetMainWindowXamlRoot()
    {
        return MainWindow?.Content?.XamlRoot;
    }

    public App()
    {
        InitializeComponent();

        host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    [SuppressMessage("Reliability", "CS4014:Because this call is not awaited, execution of the current method continues before the call is completed", Justification = "Fire and forget")]
    [SuppressMessage("Performance", "MA0134:Observe result of async calls", Justification = "Fire and forget")]
    [SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "Fire and forget")]
    protected override async void OnLaunched(
        LaunchActivatedEventArgs args)
    {
        await host
            .StartAsync()
            .ConfigureAwait(false);

        window = host.Services.GetService<MainWindow>();
        if (window is not null)
        {
            MainWindow = window;
            window.Closed += OnWindowClosed;
            window.Activate();
        }
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Event handler")]
    private async void OnWindowClosed(
        object sender, WindowEventArgs args)
    {
        await host
            .StopAsync()
            .ConfigureAwait(false);

        host.Dispose();
    }

    [SuppressMessage("Performance", "CA1848:For improved performance, use the LoggerMessage delegates", Justification = "Simple logging for unhandled exceptions")]
    private void OnUnhandledException(
        object sender,
        System.UnhandledExceptionEventArgs e)
    {
        var logger = host.Services.GetService<ILogger<App>>();
        logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled exception occurred");
    }
}