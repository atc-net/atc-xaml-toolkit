// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.Command;

/// <summary>
/// A generic asynchronous command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// This class allows you to accept command parameters in the Execute and CanExecute callback methods.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommandAsync<T> : IRelayCommandAsync<T>
{
    private readonly Func<T, Task>? execute;
    private readonly WeakFunc<T, bool>? wfCanExecute;
    private readonly IErrorHandler? errorHandler;
    private bool isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsync{T}"/> class that can always execute.
    /// </summary>
    /// <param name="execute">
    /// The asynchronous execution logic. IMPORTANT: If the action causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="canExecute">
    /// The execution status logic. IMPORTANT: If the func causes a closure, you must set keepTargetAlive to true to avoid side effects.
    /// </param>
    /// <param name="errorHandler">
    /// The error handler used to process exceptions that occur during command execution.
    /// </param>
    /// <param name="keepTargetAlive">
    /// If true, the target of the action will be kept as a hard reference, which might cause a memory leak.
    /// Only set this parameter to true if the action is causing a closure.
    /// </param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    public RelayCommandAsync(
        Func<T, Task> execute,
        Func<T, bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
        }

        this.errorHandler = errorHandler;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
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
        var val = parameter;

        if (parameter is not null &&
            parameter.GetType() != typeof(T) &&
            parameter is IConvertible)
        {
            val = Convert.ChangeType(parameter, typeof(T), provider: null);
        }

        if (isExecuting || !CanExecute(val) || execute is null)
        {
            return;
        }

        isExecuting = true;

        if (errorHandler is null)
        {
            await DoExecute(val).ConfigureAwait(true);
            RaiseCanExecuteChanged();
            isExecuting = false;
        }
        else
        {
            try
            {
                await DoExecute(val).ConfigureAwait(true);
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

    /// <inheritdoc />
    public Task ExecuteAsync(T parameter)
    {
        if (CanExecute(parameter) && execute is not null)
        {
            return execute(parameter);
        }

        return Task.CompletedTask;
    }

    private async Task DoExecute(object? val)
    {
        if (execute is null)
        {
            return;
        }

        if (val is null)
        {
            if (typeof(T).IsValueType)
            {
                await ExecuteAsync(default!).ConfigureAwait(true);
            }
            else
            {
                await execute((T)val!).ConfigureAwait(true);
            }
        }
        else
        {
            await execute((T)val).ConfigureAwait(true);
        }
    }
}