// ReSharper disable once CheckNamespace
namespace Avalonia.Threading;

/// <summary>
/// Provides extension methods for the <see cref="Dispatcher"/> class to invoke actions based on thread access requirements.
/// </summary>
/// <remarks>
/// Avalonia's <see cref="DispatcherPriority"/> is a struct (not an enum) in 11.x, so it cannot
/// appear as a default parameter value. The no-priority overloads default to
/// <see cref="DispatcherPriority.Default"/>, matching WPF / WinUI's <c>Normal</c> default.
/// </remarks>
public static class DispatcherExtensions
{
    /// <summary>
    /// Invokes the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// Uses <see cref="DispatcherPriority.Default"/>.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static void InvokeIfRequired(
        this Dispatcher dispatcher,
        Action action)
        => InvokeIfRequired(dispatcher, action, DispatcherPriority.Default);

    /// <summary>
    /// Invokes the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static void InvokeIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority)
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
    /// Uses <see cref="DispatcherPriority.Default"/>.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static Task InvokeAsyncIfRequired(
        this Dispatcher dispatcher,
        Action action)
        => InvokeAsyncIfRequired(dispatcher, action, DispatcherPriority.Default);

    /// <summary>
    /// Asynchronously invokes the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static async Task InvokeAsyncIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority)
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
    /// Uses <see cref="DispatcherPriority.Default"/>.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    public static Task BeginInvokeIfRequired(
        this Dispatcher dispatcher,
        Action action)
        => BeginInvokeIfRequired(dispatcher, action, DispatcherPriority.Default);

    /// <summary>
    /// Asynchronously begins invoking the specified action on the dispatcher thread if required, otherwise executes it directly.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to use for invoking the action.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="priority">The priority at which the action is invoked, if required.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dispatcher"/> or <paramref name="action"/> is null.</exception>
    /// <remarks>
    /// Avalonia does not have a separate <c>BeginInvoke</c> primitive — <see cref="Dispatcher.InvokeAsync(Action, DispatcherPriority)"/>
    /// already queues the action and returns an awaitable handle. This method exists for parity with the WPF and WinUI
    /// surfaces and is functionally identical to <see cref="InvokeAsyncIfRequired(Dispatcher, Action, DispatcherPriority)"/>.
    /// </remarks>
    [SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "Intentional duplication for cross-platform API parity — Avalonia has no separate BeginInvoke primitive.")]
    public static async Task BeginInvokeIfRequired(
        this Dispatcher dispatcher,
        Action action,
        DispatcherPriority priority)
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
}