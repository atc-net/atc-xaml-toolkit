// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.Command;

/// <summary>
/// An asynchronous command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// </summary>
public class RelayCommandAsync : RelayCommandAsyncBase
{
    private EventHandler requerySuggestedLocal = null!;

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
        : base(
            execute,
            canExecute,
            errorHandler,
            keepTargetAlive)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsync"/> class with cancellation support.
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
    public RelayCommandAsync(
        Func<CancellationToken, Task> execute,
        Func<bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
        : base(
            execute,
            canExecute,
            errorHandler,
            keepTargetAlive)
    {
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public override event EventHandler? CanExecuteChanged
    {
        add
        {
            if (wfCanExecute is null)
            {
                return;
            }

            EventHandler handler2;
            var canExecuteChanged = requerySuggestedLocal;

            do
            {
                handler2 = canExecuteChanged;
                var handler3 = (EventHandler)Delegate.Combine(handler2, value);
                canExecuteChanged = Interlocked.CompareExchange(
                    ref requerySuggestedLocal,
                    handler3,
                    handler2);
            }
            while (canExecuteChanged != handler2);

            CommandManager.RequerySuggested += value;
        }

        remove
        {
            if (wfCanExecute is null)
            {
                return;
            }

            EventHandler handler2;
            var canExecuteChanged = requerySuggestedLocal;

            do
            {
                handler2 = canExecuteChanged;
                var handler3 = (EventHandler)Delegate.Remove(handler2, value)!;
                canExecuteChanged = Interlocked.CompareExchange(
                    ref requerySuggestedLocal,
                    handler3,
                    handler2);
            }
            while (canExecuteChanged != handler2);

            CommandManager.RequerySuggested -= value;
        }
    }

    /// <inheritdoc />
    public override void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}