namespace Atc.XamlToolkit.Wpf.Tests.Command;

/// <summary>
/// The life cycle of an asynchronous command: overlapping runs, failures and disposal.
/// </summary>
public sealed class RelayCommandAsyncLifecycleTests
{
    [Fact]
    public async Task Cancel_AfterDisposeWhileARunIsInProgress_DoesNotThrow()
    {
        var gate = new TaskCompletionSource();
        var command = new RelayCommandAsync(_ => gate.Task);
        var run = command.ExecuteAsync(null);

        command.Dispose();
        var exception = Record.Exception(command.Cancel);

        gate.SetResult();
        await run;

        Assert.Null(exception);
    }

    [Fact]
    public async Task Cancel_AfterDisposeWhileARunIsInProgress_DoesNotThrow_Generic()
    {
        var gate = new TaskCompletionSource();
        var command = new RelayCommandAsync<int>((_, _) => gate.Task);
        var run = command.ExecuteAsync(1);

        command.Dispose();
        var exception = Record.Exception(command.Cancel);

        gate.SetResult();
        await run;

        Assert.Null(exception);
    }

    [Fact]
    public async Task Cancel_DoesNotBlockOtherRuns_WhileACancellationCallbackIsRunning()
    {
        // A callback registered on the token runs inside Cancel(). If Cancel() held the
        // command's lock meanwhile, a run on another thread could neither start nor finish,
        // and a callback that waits for such a run would deadlock.
        var gate = new TaskCompletionSource();
        var calls = 0;
        var otherRunFinished = false;
        RelayCommandAsync command = null!;
        command = new RelayCommandAsync(token =>
        {
            if (Interlocked.Increment(ref calls) > 1)
            {
                return Task.CompletedTask;
            }

            token.Register(() =>
            {
                var otherRun = Task.Run(() => command.ExecuteAsync(null), CancellationToken.None);
                otherRunFinished = otherRun.Wait(TimeSpan.FromSeconds(10));
            });

            return gate.Task;
        });

        using (command)
        {
            var run = command.ExecuteAsync(null);

            command.Cancel();

            gate.SetResult();
            await run;
        }

        Assert.True(otherRunFinished);
    }

    [Fact]
    public async Task IsExecuting_StaysTrueUntilTheLastOverlappingRunHasFinished()
    {
        var first = new TaskCompletionSource();
        var second = new TaskCompletionSource();
        var gates = new Queue<TaskCompletionSource>();
        gates.Enqueue(first);
        gates.Enqueue(second);
        using var command = new RelayCommandAsync(() => gates.Dequeue().Task);
        var notifications = new List<bool>();
        command.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(command.IsExecuting))
            {
                notifications.Add(command.IsExecuting);
            }
        };

        var firstRun = command.ExecuteAsync(null);
        var secondRun = command.ExecuteAsync(null);

        first.SetResult();
        await firstRun;

        Assert.True(command.IsExecuting);
        Assert.Equal([true], notifications);

        second.SetResult();
        await secondRun;

        Assert.False(command.IsExecuting);
        Assert.Equal([true, false], notifications);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheDelegateThrows_PassesTheExceptionOn_AndTheCommandCanRunAgain()
    {
        var fail = true;
        using var command = new RelayCommandAsync(_ => fail
            ? Task.FromException(new InvalidOperationException("boom"))
            : Task.CompletedTask);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => command.ExecuteAsync(null));

        Assert.Equal("boom", exception.Message);
        Assert.False(command.IsExecuting);
        Assert.True(command.CanExecute(null));
        Assert.Null(Record.Exception(command.Cancel));

        fail = false;
        await command.ExecuteAsync(null);

        Assert.False(command.IsExecuting);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheDelegateThrows_PassesTheExceptionOn_Generic()
    {
        using var command = new RelayCommandAsync<int>(_ => Task.FromException(new InvalidOperationException("boom")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => command.ExecuteAsync(1));

        Assert.False(command.IsExecuting);
        Assert.True(command.CanExecute(1));
    }

    [Fact]
    public void Execute_WhenTheDelegateThrows_HandsTheExceptionToTheErrorHandler()
    {
        var handled = new List<Exception>();
        var errorHandler = Substitute.For<IErrorHandler>();
        errorHandler
            .When(x => x.HandleError(Arg.Any<Exception>()))
            .Do(call => handled.Add(call.Arg<Exception>()));

        using var command = new RelayCommandAsync(
            () => Task.FromException(new InvalidOperationException("boom")),
            errorHandler: errorHandler);

        ((System.Windows.Input.ICommand)command).Execute(parameter: null);

        Assert.IsType<InvalidOperationException>(Assert.Single(handled));
        Assert.False(command.IsExecuting);
    }

    [Fact]
    public void Execute_WhenTheRunIsCanceled_DoesNotCallTheErrorHandler()
    {
        var errorHandler = Substitute.For<IErrorHandler>();

        using var command = new RelayCommandAsync(
            _ => Task.FromCanceled(new CancellationToken(canceled: true)),
            errorHandler: errorHandler);

        ((System.Windows.Input.ICommand)command).Execute(parameter: null);

        errorHandler.DidNotReceive().HandleError(Arg.Any<Exception>());
        Assert.False(command.IsExecuting);
    }
}