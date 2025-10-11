// ReSharper disable once CheckNamespace
namespace Microsoft.UI.Dispatching;

/// <summary>
/// Provides extension methods for the <see cref="DispatcherQueue"/> class to invoke actions based on thread access requirements.
/// </summary>
public static class DispatcherQueueExtensions
{
    /// <summary>
    /// Invokes the specified action on the dispatcher queue thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcherQueue">The dispatcher queue to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherQueuePriority.Normal"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcherQueue"/> or <paramref name="action"/> is null.</exception>
    public static void InvokeIfRequired(
        this DispatcherQueue dispatcherQueue,
        Action action,
        DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcherQueue.HasThreadAccess)
        {
            action();
        }
        else
        {
            dispatcherQueue.TryEnqueue(priority, () => action());
        }
    }

    /// <summary>
    /// Asynchronously invokes the specified action on the dispatcher queue thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcherQueue">The dispatcher queue to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherQueuePriority.Normal"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcherQueue"/> or <paramref name="action"/> is null.</exception>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK")]
    public static async Task InvokeAsyncIfRequired(
        this DispatcherQueue dispatcherQueue,
        Action action,
        DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcherQueue.HasThreadAccess)
        {
            action();
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();
            dispatcherQueue.TryEnqueue(priority, () =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }
    }

    /// <summary>
    /// Asynchronously begins invoking the specified action on the dispatcher queue thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcherQueue">The dispatcher queue to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required. The default is <see cref="DispatcherQueuePriority.Normal"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcherQueue"/> or <paramref name="action"/> is null.</exception>
    public static Task BeginInvokeIfRequired(
        this DispatcherQueue dispatcherQueue,
        Action action,
        DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueue);
        ArgumentNullException.ThrowIfNull(action);

        if (dispatcherQueue.HasThreadAccess)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();
        dispatcherQueue.TryEnqueue(priority, () =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types - Need to propagate all exceptions to caller
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }
}