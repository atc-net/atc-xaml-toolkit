// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.Command;

/// <summary>
/// An asynchronous command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// </summary>
public class RelayCommandAsync : IRelayCommandAsync
{
    private readonly Func<Task>? execute;
    private readonly WeakFunc<bool>? wfCanExecute;
    private readonly IErrorHandler? errorHandler;
    private bool isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsync"/> class that can always execute.
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
    public RelayCommandAsync(
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
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
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