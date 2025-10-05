namespace Atc.XamlToolkit.Command;

/// <summary>
/// A generic command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute
/// method is 'true'. This class allows you to accept command parameters in the
/// Execute and CanExecute callback methods.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommand<T> : RelayCommandBase<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
    /// you must set keepTargetAlive to true to avoid side effects. </param>
    /// <param name="canExecute">The execution status logic.  IMPORTANT: If the func causes a closure,
    /// you must set keepTargetAlive to true to avoid side effects. </param>
    /// <param name="keepTargetAlive">If true, the target of the Action will
    /// be kept as a hard reference, which might cause a memory leak. You should only set this
    /// parameter to true if the action is causing a closure.</param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    public RelayCommand(
        Action<T> execute,
        Func<T, bool>? canExecute = null,
        bool keepTargetAlive = false)
        : base(
            execute,
            canExecute,
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
            if (wfCanExecute is not null)
            {
                CommandManager.RequerySuggested += value;
            }
        }

        remove
        {
            if (wfCanExecute is not null)
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }

    /// <inheritdoc />
    public override void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}