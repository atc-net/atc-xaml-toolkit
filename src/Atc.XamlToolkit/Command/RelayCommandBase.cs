// ReSharper disable InconsistentNaming
namespace Atc.XamlToolkit.Command;

/// <summary>
/// Base class for relay commands that provides common functionality for command execution logic.
/// </summary>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "OK")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "OK")]
public abstract class RelayCommandBase : IRelayCommand
{
    protected readonly WeakAction? waExecute;
    protected readonly WeakFunc<bool>? wfCanExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandBase"/> class.
    /// </summary>
    /// <param name="execute">The execution logic. IMPORTANT: If the action causes a closure,
    /// you must set keepTargetAlive to true to avoid side effects.</param>
    /// <param name="canExecute">The execution status logic. IMPORTANT: If the func causes a closure,
    /// you must set keepTargetAlive to true to avoid side effects.</param>
    /// <param name="keepTargetAlive">
    /// If true, the target of the Action will be kept as a hard reference,
    /// which might cause a memory leak. You should only set this parameter to true if the action is causing closures.
    /// </param>
    /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
    protected RelayCommandBase(
        Action execute,
        Func<bool>? canExecute = null,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(execute);

        waExecute = new WeakAction(execute, keepTargetAlive);

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
        }
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public abstract event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public abstract void RaiseCanExecuteChanged();

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return wfCanExecute is null ||
               ((wfCanExecute.IsStatic || wfCanExecute.IsAlive) && wfCanExecute.Execute());
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter)
            && waExecute is not null
            && (waExecute.IsStatic || waExecute.IsAlive))
        {
            waExecute.Execute();
        }
    }
}