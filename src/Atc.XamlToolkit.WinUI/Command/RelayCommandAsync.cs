// ReSharper disable AsyncVoidMethod
namespace Atc.XamlToolkit.Command;

/// <summary>
/// An asynchronous command whose sole purpose is to relay its functionality to other
/// objects by invoking delegates. The default return value for the CanExecute method is 'true'.
/// </summary>
/// <remarks>
/// <para>
/// <b>WinUI threading notes — different from WPF and Avalonia:</b>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       The constructor captures the UI thread's <see cref="DispatcherQueue"/> by calling
///       <c>DispatcherQueue.GetForCurrentThread()</c>. Construct this command on the UI thread
///       (a ViewModel that is created on the UI thread is fine; one created on a worker thread
///       will not capture a dispatcher and the auto-marshalling described below becomes a no-op).
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> events for
///       <c>IsExecuting</c> are automatically marshalled to the UI thread via
///       <see cref="DispatcherQueue.TryEnqueue(DispatcherQueuePriority, DispatcherQueueHandler)"/>.
///       This is required because WinUI <c>x:Bind</c> only subscribes to <c>PropertyChanged</c> on
///       the root ViewModel, and raising the event from a background thread otherwise causes
///       <c>RPC_E_WRONG_THREAD</c> (0x8001010E) when bindings refresh.
///     </description>
///   </item>
///   <item>
///     <description>
///       WPF and Avalonia do not need this — their dispatchers cope with cross-thread
///       <c>PropertyChanged</c> raises and standard <c>{Binding}</c> subscribes to intermediate
///       objects in property paths.
///     </description>
///   </item>
/// </list>
/// <para>
/// To bind <c>IsExecuting</c> on a nested command property with <c>x:Bind</c>, use the property
/// the source generator emits on the ViewModel for compiled bindings:
/// <c>{x:Bind ViewModel.DoStuffCommand.IsExecuting, Mode=OneWay}</c>.
/// </para>
/// </remarks>
public class RelayCommandAsync : RelayCommandAsyncBase
{
    private readonly DispatcherQueue? dispatcherQueue;

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

    /// <inheritdoc />
    public override void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
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