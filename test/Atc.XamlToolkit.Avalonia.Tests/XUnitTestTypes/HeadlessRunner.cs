namespace Atc.XamlToolkit.Avalonia.Tests.XUnitTestTypes;

/// <summary>
/// Runs test code on the UI thread of a headless Avalonia application,
/// for tests that need a window, focus or the dispatcher.
/// </summary>
internal static class HeadlessRunner
{
    private static readonly Lazy<global::Avalonia.Headless.HeadlessUnitTestSession> Session = new(
        static () => global::Avalonia.Headless.HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication)));

    // Called by HeadlessApplicationFixture before any test runs.
    public static void Start()
        => _ = Session.Value;

    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Session.Value
            .Dispatch(action, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    // Shows the content in a headless window, so it is attached to the visual tree and can take focus.
    public static global::Avalonia.Controls.Window ShowInWindow(
        global::Avalonia.Controls.Control content)
    {
        var window = new global::Avalonia.Controls.Window
        {
            Content = content,
            Width = 200,
            Height = 100,
        };

        window.Show();
        PumpDispatcher();
        return window;
    }

    /// <summary>
    /// Processes everything queued on the UI thread's dispatcher.
    /// </summary>
    public static void PumpDispatcher()
        => global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
}