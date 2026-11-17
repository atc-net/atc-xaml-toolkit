namespace Atc.XamlToolkit.Wpf.Tests.Extensions;

public sealed class DispatcherExtensionsBeginInvokeIfRequiredTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task FromAnotherThread_WhenTheActionThrows_TheExceptionIsOnTheReturnedTask()
    {
        using var dispatcherThread = new DispatcherThread();

        var task = dispatcherThread.Dispatcher.BeginInvokeIfRequired(
            () => throw new InvalidOperationException("boom"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => task.WaitAsync(Timeout, TestContext.Current.CancellationToken));

        Assert.Equal("boom", exception.Message);
        Assert.False(dispatcherThread.UnhandledExceptionRaised);
    }

    [Fact]
    public async Task FromAnotherThread_TheTaskCompletesWhenTheActionHasRun()
    {
        using var dispatcherThread = new DispatcherThread();
        var ranOnThreadId = 0;

        await dispatcherThread.Dispatcher
            .BeginInvokeIfRequired(() => ranOnThreadId = Environment.CurrentManagedThreadId)
            .WaitAsync(Timeout, TestContext.Current.CancellationToken);

        Assert.Equal(dispatcherThread.ManagedThreadId, ranOnThreadId);
    }

    [Fact]
    public async Task FromAnotherThread_WhenTheDispatcherHasShutDown_TheTaskIsCanceled()
    {
        // The queued work never runs on a dispatcher that has shut down. Without a canceled
        // task, a caller that awaits during application shutdown would wait forever.
        var dispatcherThread = new DispatcherThread();
        dispatcherThread.Dispose();
        var ran = false;

        var task = dispatcherThread.Dispatcher.BeginInvokeIfRequired(() => ran = true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => task.WaitAsync(Timeout, TestContext.Current.CancellationToken));
        Assert.True(task.IsCanceled);
        Assert.False(ran);
    }

    [Fact]
    public void OnTheDispatcherThread_TheActionRunsBeforeTheCallReturns()
        => StaThreadRunner.Run(() =>
        {
            var ran = false;

            var task = Dispatcher.CurrentDispatcher.BeginInvokeIfRequired(() => ran = true);

            Assert.True(ran);
            Assert.True(task.IsCompletedSuccessfully);
        });

    [Fact]
    public void OnTheDispatcherThread_WhenTheActionThrows_TheCallThrows()
        => StaThreadRunner.Run(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            var exception = Assert.Throws<InvalidOperationException>(
                () => { _ = dispatcher.BeginInvokeIfRequired(() => throw new InvalidOperationException("boom")); });

            Assert.Equal("boom", exception.Message);
        });

    [Fact]
    public void ANullArgument_ThrowsWhenTheMethodIsCalled_NotWhenTheTaskIsAwaited()
        => StaThreadRunner.Run(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;

            Assert.Throws<ArgumentNullException>(() => { _ = dispatcher.BeginInvokeIfRequired(null!); });
            Assert.Throws<ArgumentNullException>(() => { _ = ((Dispatcher)null!).BeginInvokeIfRequired(() => { }); });
        });

    /// <summary>
    /// A thread that runs a dispatcher, so calls from the test thread have to be marshalled.
    /// </summary>
    private sealed class DispatcherThread : IDisposable
    {
        private readonly Thread thread;

        public DispatcherThread()
        {
            using var started = new ManualResetEventSlim();
            Dispatcher? dispatcher = null;

            thread = new Thread(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;

                // Without this, an exception that reaches the dispatcher would end the test process.
                dispatcher.UnhandledException += (_, e) =>
                {
                    UnhandledExceptionRaised = true;
                    e.Handled = true;
                };

                started.Set();
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            started.Wait();

            Dispatcher = dispatcher!;
            ManagedThreadId = thread.ManagedThreadId;
        }

        public Dispatcher Dispatcher { get; }

        public int ManagedThreadId { get; }

        public bool UnhandledExceptionRaised { get; private set; }

        public void Dispose()
        {
            Dispatcher.InvokeShutdown();
            thread.Join();
        }
    }
}