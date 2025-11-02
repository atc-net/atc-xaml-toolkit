// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.Command;

/// <summary>
/// A generic asynchronous command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// This class allows you to accept command parameters in the Execute and CanExecute callback methods.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public class RelayCommandAsync<T> : RelayCommandAsyncBase<T>
{
    private readonly DispatcherQueue? dispatcherQueue;

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
        : base(
            execute,
            canExecute,
            errorHandler,
            keepTargetAlive)
    {
        try
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }
        catch (COMException)
        {
            // In test environments or without WinUI runtime, this may fail - that's OK
            // The PropertyChanged marshalling will handle null dispatcher gracefully
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsync{T}"/> class with cancellation support.
    /// </summary>
    /// <param name="execute">
    /// The asynchronous execution logic with cancellation token support. IMPORTANT: If the action causes a closure, you must set keepTargetAlive to true to avoid side effects.
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
        Func<T, CancellationToken, Task> execute,
        Func<T, bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
        : base(
            execute,
            canExecute,
            errorHandler,
            keepTargetAlive)
    {
        try
        {
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }
        catch (COMException)
        {
            // In test environments or without WinUI runtime, this may fail - that's OK
            // The PropertyChanged marshalling will handle null dispatcher gracefully
        }
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public override event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    public override void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // Always marshal PropertyChanged to UI thread for WinUI x:Bind compatibility
        DispatcherQueue? queue = null;
        try
        {
            queue = DispatcherQueue.GetForCurrentThread() ?? dispatcherQueue;
        }
        catch (COMException)
        {
            // In test environments, GetForCurrentThread may fail
            queue = dispatcherQueue;
        }

        if (queue is not null && !queue.HasThreadAccess)
        {
            // We're on a background thread, marshal to UI thread asynchronously
            _ = queue.TryEnqueue(DispatcherQueuePriority.High, () =>
            {
                base.OnPropertyChanged(propertyName);
            });
        }
        else
        {
            // Already on UI thread or no dispatcher available
            base.OnPropertyChanged(propertyName);
        }
    }
}