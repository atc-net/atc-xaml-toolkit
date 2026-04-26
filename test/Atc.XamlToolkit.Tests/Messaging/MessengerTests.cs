namespace Atc.XamlToolkit.Tests.Messaging;

public sealed class MessengerTests
{
    [Fact]
    [SuppressMessage("Major Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Required to verify weak-reference cleanup behaviour.")]
    [SuppressMessage("Usage", "xUnit1051:Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken", Justification = "Test does not need cancellation.")]
    public Task RequestCleanup_PrunesDeadRecipients_OnSubsequentCalls_WhenNoSynchronizationContext()
        => Task.Run(() =>
        {
            // Run on a worker thread so SynchronizationContext.Current is null
            // when the Messenger is constructed (the previously broken branch).
            SynchronizationContext.SetSynchronizationContext(null);

            var messenger = new Messenger();
            GetCapturedSynchronizationContext(messenger)
                .Should()
                .BeNull("the test must exercise the no-context branch of RequestCleanup");

            // Arrange: register a recipient and let it become eligible for collection.
            // A NoInlining helper guarantees the local is off the stack frame
            // before we trigger GC (Release/tiered JIT can elide locals otherwise).
            RegisterAndAbandonRecipient<int>(messenger);
            ForceFullGarbageCollection();

            GetStrictRecipientsCount<int>(messenger)
                .Should()
                .Be(1, "the dead WeakAction is still listed until Cleanup runs");

            // Act 1: first cleanup runs synchronously and must reset the flag.
            messenger.RequestCleanup();

            // Assert 1: cleanup ran — dead entry pruned and flag cleared so the
            // next RequestCleanup is allowed to dispatch again.
            GetStrictRecipientsCount<int>(messenger)
                .Should()
                .Be(0, "Cleanup should have removed the dead recipient");
            IsCleanupPending(messenger)
                .Should()
                .BeFalse("Cleanup resets the flag on its way out so future requests can fire");

            // Arrange 2: register another short-lived recipient to create a new dead entry.
            RegisterAndAbandonRecipient<int>(messenger);
            ForceFullGarbageCollection();

            // Act 2: second cleanup. With the bug, this is a no-op because the flag is stuck.
            messenger.RequestCleanup();

            // Assert 2: the second cleanup actually ran and pruned the new dead entry.
            GetStrictRecipientsCount<int>(messenger)
                .Should()
                .Be(0, "RequestCleanup must dispatch a fresh cleanup after a prior synchronous run");
            IsCleanupPending(messenger)
                .Should()
                .BeFalse();
        });

    [Fact]
    public void Reset_AndDefault_ProduceASingleSharedInstance()
    {
        Messenger.Reset();

        var first = Messenger.Default;
        var second = Messenger.Default;

        second.Should().BeSameAs(first);

        Messenger.Reset();

        var third = Messenger.Default;
        third.Should().NotBeSameAs(first);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterAndAbandonRecipient<TMessage>(
        Messenger messenger)
    {
        var recipient = new object();
        messenger.Register<TMessage>(recipient, _ => { });

        // recipient leaves scope when this helper returns — the JIT cannot
        // keep it alive past the method boundary.
    }

    [SuppressMessage("Major Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Required to verify weak-reference cleanup behaviour.")]
    private static void ForceFullGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static SynchronizationContext? GetCapturedSynchronizationContext(
        Messenger messenger)
        => (SynchronizationContext?)GetPrivateField(messenger, "context");

    private static bool IsCleanupPending(Messenger messenger)
        => (int)GetPrivateField(messenger, "cleanupPending")! != 0;

    private static int GetStrictRecipientsCount<TMessage>(Messenger messenger)
    {
        var dict = (System.Collections.IDictionary)GetPrivateField(messenger, "recipientsStrictAction")!;
        return dict.Contains(typeof(TMessage))
            ? ((System.Collections.ICollection)dict[typeof(TMessage)]!).Count
            : 0;
    }

    private static object? GetPrivateField(
        object instance,
        string fieldName)
    {
        var field = instance.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.Should().NotBeNull($"private field '{fieldName}' should exist on {instance.GetType().Name}");
        return field!.GetValue(instance);
    }
}