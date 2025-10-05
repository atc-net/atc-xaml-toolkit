// ReSharper disable AsyncVoidMethod
// ReSharper disable InconsistentNaming
namespace Atc.XamlToolkit.Command;

/// <summary>
/// Base class for asynchronous relay commands that provides common functionality for async command execution logic.
/// </summary>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "OK")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "OK")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "OK")]
public abstract class RelayCommandAsyncBase : IRelayCommandAsync
{
    protected readonly Func<Task>? execute;
    protected readonly WeakFunc<bool>? wfCanExecute;
    protected readonly IErrorHandler? errorHandler;
    protected bool isExecuting;

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
        // No cancellation support in base implementation.
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
        if (isExecuting || !CanExecute(parameter) || execute is null)
        {
            return;
        }

        isExecuting = true;

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
        }
    }

    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="parameter">The command parameter (ignored in this implementation).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ExecuteAsync(object? parameter)
    {
        if (CanExecute(parameter) && execute is not null)
        {
            return execute();
        }

        return Task.CompletedTask;
    }
}