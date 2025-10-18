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
public abstract class RelayCommandAsyncBase : IRelayCommandAsync, IDisposable
{
    protected readonly Func<Task>? execute;
    protected readonly Func<CancellationToken, Task>? executeWithCancellation;
    protected readonly WeakFunc<bool>? wfCanExecute;
    protected readonly IErrorHandler? errorHandler;
    protected bool isExecuting;
    protected CancellationTokenSource? cancellationTokenSource;

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

    /// <inheritdoc />
    public abstract void RaiseCanExecuteChanged();

    /// <summary>
    /// Attempts to cancel the ongoing execution if supported. Base implementation is a no-op because
    /// the provided delegate does not take a CancellationToken. Override in derived types if needed.
    /// </summary>
    public virtual void Cancel()
    {
        if (cancellationTokenSource is not null &&
            !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
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

        if (execute is null && executeWithCancellation is null)
        {
            return;
        }

        isExecuting = true;

        // Create a new cancellation token source if we support cancellation
        if (executeWithCancellation is not null)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
        }

        if (errorHandler is null)
        {
            await ExecuteAsync(parameter).ConfigureAwait(true);
            RaiseCanExecuteChanged();
            isExecuting = false;
        }
        else
        {
            try
            {
                await ExecuteAsync(parameter).ConfigureAwait(true);
                RaiseCanExecuteChanged();
                isExecuting = false;
            }
            catch (Exception ex)
            {
                errorHandler.HandleError(ex);
                isExecuting = false;
            }
            finally
            {
                if (executeWithCancellation is not null)
                {
                    cancellationTokenSource?.Dispose();
                    cancellationTokenSource = null;
                }
            }
        }
    }

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The command parameter (ignored in this implementation).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync(object? parameter)
    {
        if (CanExecute(parameter))
        {
            if (executeWithCancellation is not null)
            {
                // Create cancellation token source if it doesn't exist
                if (cancellationTokenSource is null)
                {
                    cancellationTokenSource = new CancellationTokenSource();
                }

                return executeWithCancellation(cancellationTokenSource.Token);
            }

            if (execute is not null)
            {
                return execute();
            }
        }

        return Task.CompletedTask;
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

        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }
}