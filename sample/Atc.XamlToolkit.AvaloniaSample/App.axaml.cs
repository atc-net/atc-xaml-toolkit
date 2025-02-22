#pragma warning disable S3168
#pragma warning disable AsyncFixer03
// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.AvaloniaSample;

public partial class App : Application
{
    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Start the host and assign the MainWindow from the DI container.
            InitializeHostAsync(desktop);

            // Hook into the exit event to stop and dispose the host.
            desktop.Exit += Desktop_Exit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void InitializeHostAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        await host.StartAsync().ConfigureAwait(true);
        desktop.MainWindow = host.Services.GetService<MainWindow>();
    }

    private async void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        await host.StopAsync().ConfigureAwait(true);
        host.Dispose();
    }

    private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            // For simplicity, log the unhandled exception to the console.
            System.Diagnostics.Trace.TraceError("Unhandled exception: " + ex);
        }
    }
}