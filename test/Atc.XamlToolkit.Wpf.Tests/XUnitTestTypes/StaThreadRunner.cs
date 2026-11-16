namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

/// <summary>
/// Runs test code on a dedicated STA thread, which WPF elements require.
/// </summary>
internal static class StaThreadRunner
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The exception is rethrown on the calling thread.")]
    public static void Run(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        System.Runtime.ExceptionServices.ExceptionDispatchInfo? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();

        failure?.Throw();
    }

    /// <summary>
    /// Processes everything queued on the current thread's dispatcher.
    /// </summary>
    public static void PumpDispatcher()
        => Dispatcher.CurrentDispatcher.Invoke(
            static () => { },
            DispatcherPriority.ApplicationIdle);
}