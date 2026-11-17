// ReSharper disable AsyncVoidMethod
// ReSharper disable InconsistentNaming
// ReSharper disable InvertIf
namespace Atc.XamlToolkit.Command;

/// <summary>
/// Base class for asynchronous relay commands that provides common functionality for async command execution logic.
/// </summary>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "OK")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "OK")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:Non-private readonly fields should begin with upper-case letter", Justification = "OK")]
public abstract class RelayCommandAsyncBase : IRelayCommandAsync, INotifyPropertyChanged
{
    protected readonly Func<Task>? execute;
    protected readonly Func<CancellationToken, Task>? executeWithCancellation;
    protected readonly WeakFunc<bool>? wfCanExecute;
    protected readonly IErrorHandler? errorHandler;
    protected bool isExecuting;
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Disposed in Dispose unless a run in progress owns it; that run disposes it when it ends.")]
    protected CancellationTokenSource? cancellationTokenSource;
    private readonly List<CancellationTokenSource> runningTokenSources = [];
    private int executionCount;

    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsExecuting => isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsyncBase"/> class.
    /// </summary>
    /// <param name="execute">
    /// The execution logic. IMPORTANT: If the action causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="canExecute">
    /// The execution status logic. IMPORTANT: If the func causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="errorHandler">
    /// The error handler used to process exceptions that occur during command execution.
    /// </param>
    /// <param name="keepTargetAlive">
    /// If true, the target of the Action will be kept as a hard reference, which might cause a memory leak. Only set this to true if the action is causing a closure.
    /// </param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    protected RelayCommandAsyncBase(
        Func<Task> execute,
        Func<bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
        }

        this.errorHandler = errorHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsyncBase"/> class with cancellation support.
    /// </summary>
    /// <param name="execute">
    /// The execution logic with cancellation token support. IMPORTANT: If the action causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="canExecute">
    /// The execution status logic. IMPORTANT: If the func causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="errorHandler">
    /// The error handler used to process exceptions that occur during command execution.
    /// </param>
    /// <param name="keepTargetAlive">
    /// If true, the target of the Action will be kept as a hard reference, which might cause a memory leak. Only set this to true if the action is causing a closure.
    /// </param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    protected RelayCommandAsyncBase(
        Func<CancellationToken, Task> execute,
        Func<bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
    {
        executeWithCancellation = execute ?? throw new ArgumentNullException(nameof(execute));

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
        }

        this.errorHandler = errorHandler;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public abstract event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public abstract void RaiseCanExecuteChanged();

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        handler(this, Mvvm.PropertyChangedEventArgsCache.Get(propertyName));
    }

    /// <summary>
    /// Attempts to cancel the ongoing execution if supported. Base implementation is a no-op because
    /// the provided delegate does not take a CancellationToken. Override in derived types if needed.
    /// </summary>
    public virtual void Cancel()
    {
        // Cancelling inside the lock keeps a finishing run from disposing its
        // token source while it is being cancelled.
        lock (runningTokenSources)
        {
            foreach (var tokenSource in runningTokenSources.ToArray())
            {
                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        // Cannot execute if already executing
        if (isExecuting)
        {
            return false;
        }

        return wfCanExecute is null ||
               ((wfCanExecute.IsStatic || wfCanExecute.IsAlive) && wfCanExecute.Execute());
    }

    /// <inheritdoc />
    [SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "OK - ICommand signature")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK - errorHandler will handle it")]
    public async void Execute(object? parameter)
    {
        if (isExecuting || !CanExecute(parameter))
        {
            return;
        }

        try
        {
            await ExecuteAsync(parameter).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected and not an error - just complete silently
        }
        catch (Exception ex) when (errorHandler is not null)
        {
            errorHandler.HandleError(ex);
        }
    }

    /// <summary>
    /// Executes the command asynchronously. The execution state (<see cref="IsExecuting"/>,
    /// <see cref="CanExecuteChanged"/> and the cancellation token) is managed the same way
    /// as for <see cref="Execute"/>, but exceptions are propagated to the caller.
    /// </summary>
    /// <param name="parameter">The command parameter (ignored in this implementation).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(object? parameter)
    {
        if (execute is null && executeWithCancellation is null)
        {
            return;
        }

        // Every run gets its own token source, so a canceled token is never reused.
        // Cancel() cancels all runs that are in progress.
        CancellationTokenSource? runTokenSource = null;
        if (executeWithCancellation is not null)
        {
            runTokenSource = new CancellationTokenSource();
            cancellationTokenSource = runTokenSource;

            lock (runningTokenSources)
            {
                runningTokenSources.Add(runTokenSource);
            }
        }

        BeginExecution();

        try
        {
            if (executeWithCancellation is not null)
            {
                await executeWithCancellation(runTokenSource!.Token).ConfigureAwait(true);
            }
            else
            {
                await execute!().ConfigureAwait(true);
            }
        }
        finally
        {
            if (runTokenSource is not null)
            {
                Interlocked.CompareExchange(ref cancellationTokenSource, value: null, runTokenSource);

                lock (runningTokenSources)
                {
                    runningTokenSources.Remove(runTokenSource);
                    runTokenSource.Dispose();
                }
            }

            EndExecution();
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="RelayCommandAsyncBase"/> and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="RelayCommandAsyncBase"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        // A run that is in progress owns its token source and disposes it when it ends.
        // Disposing it here would make a later Cancel() throw.
        lock (runningTokenSources)
        {
            if (cancellationTokenSource is { } tokenSource &&
                !runningTokenSources.Contains(tokenSource))
            {
                tokenSource.Dispose();
            }
        }

        cancellationTokenSource = null;
    }

    private void BeginExecution()
    {
        if (Interlocked.Increment(ref executionCount) != 1)
        {
            return;
        }

        isExecuting = true;
        OnPropertyChanged(nameof(IsExecuting));
        RaiseCanExecuteChanged();
    }

    private void EndExecution()
    {
        if (Interlocked.Decrement(ref executionCount) != 0)
        {
            return;
        }

        isExecuting = false;
        OnPropertyChanged(nameof(IsExecuting));
        RaiseCanExecuteChanged();
    }
}