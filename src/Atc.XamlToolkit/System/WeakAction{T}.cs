// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Stores an Action without causing a hard reference
/// to be created to the Action's owner. The owner can be garbage collected at any time.
/// </summary>
/// <typeparam name="T">The type of the Action's parameter.</typeparam>
public sealed class WeakAction<T> : WeakAction, IExecuteWithObject
{
    private Action<T>? staticAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakAction{T}"/> class.
    /// </summary>
    /// <param name="action">The action that will be associated to this instance.</param>
    /// <param name="keepTargetAlive">If true, the target of the Action will
    /// be kept as a hard reference, which might cause a memory leak.</param>
    public WeakAction(
        Action<T>? action,
        bool keepTargetAlive = false)
        : this(action?.Target, action, keepTargetAlive)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakAction{T}"/> class.
    /// </summary>
    /// <param name="target">The action's owner.</param>
    /// <param name="action">The action that will be associated to this instance.</param>
    /// <param name="keepTargetAlive">If true, the target of the Action will
    /// be kept as a hard reference, which might cause a memory leak.</param>
    public WeakAction(
        object? target,
        Action<T>? action,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (action.Method.IsStatic)
        {
            staticAction = action;

            if (target is not null)
            {
                // Keep a reference to the target to control the
                // WeakAction's lifetime.
                Reference = new WeakReference(target);
            }

            return;
        }

        Method = action.Method;
        ActionReference = new WeakReference(action.Target);

        LiveReference = keepTargetAlive
            ? action.Target
            : null;
        Reference = new WeakReference(target);

#if DEBUG
        if (ActionReference.Target is not null && !keepTargetAlive)
        {
            var type = ActionReference.Target.GetType();

            if (type.Name.StartsWith("<>", StringComparison.Ordinal) && type.Name.Contains("DisplayClass", StringComparison.Ordinal))
            {
                Diagnostics.Debug.WriteLine("You are attempting to register a lambda with a closure without using keepTargetAlive. Are you sure?");
            }
        }
#endif
    }

    /// <summary>
    /// Gets the name of the method that this WeakAction represents.
    /// </summary>
    public override string MethodName => staticAction is null
        ? Method!.Name
        : staticAction.Method.Name;

    /// <summary>
    /// Gets a value indicating whether the Action's owner is still alive, or if it was collected
    /// by the Garbage Collector already.
    /// </summary>
    public override bool IsAlive
    {
        get
        {
            // Snapshot the volatile fields so MarkForDeletion on another
            // thread cannot turn a passed null check into a deref-NRE.
            var localStaticAction = staticAction;
            var localReference = Reference;

            if (localStaticAction is null && localReference is null)
            {
                return false;
            }

            return localStaticAction is null
                ? localReference is not null && localReference.IsAlive
                : localReference is null || localReference.IsAlive;
        }
    }

    /// <summary>
    /// Executes the action. This only happens if the action's owner
    /// is still alive.
    /// </summary>
    /// <param name="parameter">A parameter to be passed to the action.</param>
    public void Execute(T? parameter = default)
    {
        // Snapshot the volatile fields once. MarkForDeletion can null them
        // from another thread between a check and a dereference, so we must
        // not read Method twice.
        var localStaticAction = staticAction;
        if (localStaticAction is not null)
        {
            localStaticAction(parameter!);
            return;
        }

        var actionTarget = ActionTarget;
        var localMethod = Method;

        if (!IsAlive
            || localMethod is null
            || (LiveReference is null && ActionReference is null)
            || actionTarget is null)
        {
            return;
        }

        _ = localMethod.Invoke(
            actionTarget,
            new object[] { parameter! });
    }

    /// <summary>
    /// Executes the action with a parameter of type object. This parameter
    /// will be casted to T. This method implements <see cref="IExecuteWithObject.ExecuteWithObject" />
    /// and can be useful if you store multiple WeakAction{T} instances but don't know in advance
    /// what type T represents.
    /// </summary>
    /// <param name="parameter">The parameter that will be passed to the action after
    ///     being casted to T.</param>
    public void ExecuteWithObject(object? parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var parameterCasted = (T)parameter;
        Execute(parameterCasted);
    }

    /// <summary>
    /// Sets all the actions that this WeakAction contains to null,
    /// which is a signal for containing objects that this WeakAction
    /// should be deleted.
    /// </summary>
    public new void MarkForDeletion()
    {
        staticAction = null;
        base.MarkForDeletion();
    }
}