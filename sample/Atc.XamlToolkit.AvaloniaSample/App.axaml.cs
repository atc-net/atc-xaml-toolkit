// ReSharper disable AsyncVoidEventHandlerMethod
// ReSharper disable AsyncVoidMethod
// ReSharper disable PartialTypeWithSinglePart
namespace Atc.XamlToolkit.AvaloniaSample;

[SuppressMessage("Major Code Smell", "S3168:\"async\" methods should not return \"void\"", Justification = "Event handlers")]
[SuppressMessage("AsyncUsage.CSharp.Naming", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "Event handlers")]
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
            desktop.Exit += OnDesktopExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void InitializeHostAsync(
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        await host
            .StartAsync()
            .ConfigureAwait(false);

        desktop.MainWindow = host.Services.GetService<MainWindow>();
    }

    private async void OnDesktopExit(
        object? sender,
        ControlledApplicationLifetimeExitEventArgs e)
    {
        await host
            .StopAsync()
            .ConfigureAwait(false);

        host.Dispose();
    }

    private static void CurrentDomainUnhandledException(
        object sender,
        UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            // For simplicity, log the unhandled exception to the console.
            System.Diagnostics.Trace.TraceError("Unhandled exception: " + ex);
        }
    }
}