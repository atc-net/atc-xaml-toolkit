// ReSharper disable once CheckNamespace
namespace System.Windows.Threading;

/// <summary>
/// Provides extension methods for the <see cref="Dispatcher"/> class to invoke actions based on thread access requirements.
/// </summary>
public static class DispatcherExtensions
{
    /// <summary>
    /// Invokes the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherPriority.Normal"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static void InvokeIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action, priority);
        }
    }

    /// <summary>
    /// Asynchronously invokes the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherPriority.Normal"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static async Task InvokeAsyncIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            await dispatcher.InvokeAsync(action, priority);
        }
    }

    /// <summary>
    /// Asynchronously begins invoking the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherPriority.Normal"/>.</param>
    /// <returns>
    /// A task that completes when the action has run. When the action throws on the dispatcher
    /// thread, the task carries the exception; it does not reach the dispatcher's
    /// unhandled-exception handling.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    /// <remarks>
    /// When the caller is already on the dispatcher thread the action runs before the method
    /// returns, and an exception it throws is thrown to the caller.
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The exception is passed on through the returned task.")]
    public static Task BeginInvokeIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority = DispatcherPriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        // Dispatcher.BeginInvoke does not report an exception to the caller: it goes to the
        // dispatcher's unhandled-exception handling. Carry it on the task instead.
        var completion = new TaskCompletionSource<object?>();
        var operation = dispatcher.BeginInvoke(
            priority,
            new Action(() =>
            {
                try
                {
                    action();
                    completion.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    completion.TrySetException(ex);
                }
            }));

        // The work never runs when the dispatcher shuts down first or the operation is
        // aborted. Without this the task would never complete.
        operation.Aborted += (_, _) => completion.TrySetCanceled();
        if (operation.Status == DispatcherOperationStatus.Aborted)
        {
            completion.TrySetCanceled();
        }

        return completion.Task;
    }
}