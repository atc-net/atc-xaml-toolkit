// ReSharper disable InconsistentNaming
namespace Atc.XamlToolkit.Command;

/// <summary>
/// Base class for generic relay commands that provides common functionality for command execution logic with typed parameters.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "OK")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "OK")]
public abstract class RelayCommandBase<T> : IRelayCommand<T>
{
    protected readonly WeakAction<T>? waExecute;
    protected readonly WeakFunc<T, bool>? wfCanExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandBase{T}"/> class.
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
        Action<T> execute,
        Func<T, bool>? canExecute = null,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(execute);

        waExecute = new WeakAction<T>(execute, keepTargetAlive);

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
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
        if (wfCanExecute is null)
        {
            return true;
        }

        if (!wfCanExecute.IsStatic && !wfCanExecute.IsAlive)
        {
            return false;
        }

        return parameter switch
        {
            null when typeof(T).IsValueType => wfCanExecute.Execute(),
            null or T => wfCanExecute.Execute((T)parameter!),
            _ => false,
        };
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        var val = parameter;

        if (parameter is not null &&
            parameter.GetType() != typeof(T) &&
            parameter is IConvertible)
        {
            val = Convert.ChangeType(parameter, typeof(T), provider: null);
        }

        if (!CanExecute(val) ||
            waExecute is null ||
            (!waExecute.IsStatic && !waExecute.IsAlive))
        {
            return;
        }

        if (val is null)
        {
            if (typeof(T).IsValueType)
            {
                waExecute.Execute();
            }
            else
            {
                waExecute.Execute((T)val!);
            }
        }
        else
        {
            waExecute.Execute((T)val);
        }
    }
}