// ReSharper disable AsyncVoidMethod
// ReSharper disable InconsistentNaming
// ReSharper disable InvertIf
namespace Atc.XamlToolkit.Command;

/// <summary>
/// Base class for generic asynchronous relay commands that provides common functionality for async command execution logic with typed parameters.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "OK")]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "OK")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "OK")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:Non-private readonly fields should begin with upper-case letter", Justification = "OK")]
public abstract class RelayCommandAsyncBase<T> : IRelayCommandAsync<T>, INotifyPropertyChanged
{
    protected readonly Func<T, Task>? execute;
    protected readonly Func<T, CancellationToken, Task>? executeWithCancellation;
    protected readonly WeakFunc<T, bool>? wfCanExecute;
    protected readonly IErrorHandler? errorHandler;
    protected bool isExecuting;
    protected CancellationTokenSource? cancellationTokenSource;
    private int executionCount;

    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    public bool IsExecuting => isExecuting;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsyncBase{T}"/> class.
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
    protected RelayCommandAsyncBase(
        Func<T, Task> execute,
        Func<T, bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
        }

        this.errorHandler = errorHandler;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandAsyncBase{T}"/> class with cancellation support.
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
    protected RelayCommandAsyncBase(
        Func<T, CancellationToken, Task> execute,
        Func<T, bool>? canExecute = null,
        IErrorHandler? errorHandler = null,
        bool keepTargetAlive = false)
    {
        executeWithCancellation = execute ?? throw new ArgumentNullException(nameof(execute));

        if (canExecute is not null)
        {
            wfCanExecute = new WeakFunc<T, bool>(canExecute, keepTargetAlive);
        }

        this.errorHandler = errorHandler;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public abstract event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public abstract void RaiseCanExecuteChanged();

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        handler(this, Mvvm.PropertyChangedEventArgsCache.Get(propertyName));
    }

    /// <summary>
    /// Attempts to cancel the ongoing execution if supported. Base implementation is a no-op because
    /// the provided delegate does not take a CancellationToken. Override in derived types if needed.
    /// </summary>
    public virtual void Cancel()
    {
        if (cancellationTokenSource is not null &&
            !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        // Cannot execute if already executing
        if (isExecuting)
        {
            return false;
        }

        if (wfCanExecute is null)
        {
            return true;
        }

        if (!wfCanExecute.IsStatic && !wfCanExecute.IsAlive)
        {
            return false;
        }

        if (!CommandParameterConverter.TryConvert<T>(parameter, out var val))
        {
            return false;
        }

        return val switch
        {
            null when typeof(T).IsValueType => wfCanExecute.Execute(),
            null or T => wfCanExecute.Execute((T)val!),
            _ => false,
        };
    }

    /// <inheritdoc />
    /// <remarks>
    /// <para>
    /// <b>Null pass-through.</b> When <paramref name="parameter"/> is <see langword="null"/>
    /// and <typeparamref name="T"/> is a reference type, the async <c>execute</c> delegate
    /// is invoked with <see langword="null"/>. The .NET runtime cannot distinguish between
    /// <c>T</c> and <c>T?</c> at runtime (both resolve to the same <see cref="Type"/>), so
    /// the toolkit cannot conditionally skip execution based on declared nullability — guard
    /// the parameter inside your delegate if a null value is unsafe.
    /// </para>
    /// <para>
    /// When <typeparamref name="T"/> is a value type, a null parameter is replaced with
    /// <c>default(T)</c> before the delegate runs.
    /// </para>
    /// <para>
    /// Parameter conversion (<see cref="Convert.ChangeType(object?, Type)"/>) failures and
    /// exceptions thrown by the async body are routed to the registered
    /// <see cref="IErrorHandler"/>. <see cref="OperationCanceledException"/> is silently
    /// swallowed (cancellation is the success path). See
    /// <c>docs/Command/ErrorHandling.md</c> for the full contract.
    /// </para>
    /// </remarks>
    [SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "OK - ICommand signature")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "OK - errorHandler will handle it")]
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public async void Execute(object? parameter)
    {
        object? val;

        // Convert.ChangeType throws on overflow / format / unsupported casts.
        // Without this guard the throw escapes from async void and can crash
        // the process (no AppDomain handler for fire-and-forget continuations).
        try
        {
            val = CommandParameterConverter.Convert<T>(parameter);
        }
        catch (Exception ex) when (errorHandler is not null)
        {
            errorHandler.HandleError(ex);
            return;
        }
        catch (InvalidCastException)
        {
            return;
        }
        catch (FormatException)
        {
            return;
        }
        catch (OverflowException)
        {
            return;
        }

        // A parameter that is not (convertible to) T can never be passed to the delegate.
        if (val is not null and not T)
        {
            return;
        }

        if (isExecuting || !CanExecute(val))
        {
            return;
        }

        try
        {
            await ExecuteAsync(val is null ? default! : (T)val).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected and not an error - just complete silently
        }
        catch (Exception ex) when (errorHandler is not null)
        {
            errorHandler.HandleError(ex);
        }
    }

    /// <summary>
    /// Executes the command asynchronously. The execution state (<see cref="IsExecuting"/>,
    /// <see cref="CanExecuteChanged"/> and the cancellation token) is managed the same way
    /// as for <see cref="Execute"/>, but exceptions are propagated to the caller.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ExecuteAsync(T parameter)
    {
        if (execute is null && executeWithCancellation is null)
        {
            return;
        }

        // Every run gets its own token source, so a canceled token is never reused
        // and Cancel() always targets the most recently started run.
        CancellationTokenSource? runTokenSource = null;
        if (executeWithCancellation is not null)
        {
            runTokenSource = new CancellationTokenSource();
            cancellationTokenSource = runTokenSource;
        }

        BeginExecution();

        try
        {
            if (executeWithCancellation is not null)
            {
                await executeWithCancellation(parameter, runTokenSource!.Token).ConfigureAwait(true);
            }
            else
            {
                await execute!(parameter).ConfigureAwait(true);
            }
        }
        finally
        {
            if (runTokenSource is not null)
            {
                Interlocked.CompareExchange(ref cancellationTokenSource, value: null, runTokenSource);
                runTokenSource.Dispose();
            }

            EndExecution();
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="RelayCommandAsyncBase{T}"/> and optionally releases the managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="RelayCommandAsyncBase{T}"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
    }

    private void BeginExecution()
    {
        if (Interlocked.Increment(ref executionCount) != 1)
        {
            return;
        }

        isExecuting = true;
        OnPropertyChanged(nameof(IsExecuting));
        RaiseCanExecuteChanged();
    }

    private void EndExecution()
    {
        if (Interlocked.Decrement(ref executionCount) != 0)
        {
            return;
        }

        isExecuting = false;
        OnPropertyChanged(nameof(IsExecuting));
        RaiseCanExecuteChanged();
    }
}