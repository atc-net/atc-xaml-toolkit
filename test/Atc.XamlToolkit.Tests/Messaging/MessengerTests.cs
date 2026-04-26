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

    [Fact]
    public void Send_DoesNotDoubleFire_WhenMultipleSubclassListenerTypesRegistered()
    {
        // Regression: SendToTargetOrType used to call SendToList unconditionally
        // inside the foreach over subclass-listener types, reusing the previous
        // iteration's list when the current iteration's type did not match.
        // That caused the matching handler to fire once per registered type.
        var messenger = new Messenger();
        var derivedReceiver = new object();
        var unrelatedReceiver = new object();

        var derivedHits = 0;
        messenger.Register<DerivedMessage>(
            derivedReceiver,
            receiveDerivedMessagesToo: true,
            _ => derivedHits++);

        // A second subclass-listener type forces the bug path: the loop sees
        // two keys, only one matches, and the unconditional dispatch used to
        // re-send the matched list a second time.
        messenger.Register<UnrelatedMessage>(
            unrelatedReceiver,
            receiveDerivedMessagesToo: true,
            _ => { });

        messenger.Send(new DerivedMessage());

        derivedHits
            .Should()
            .Be(1, "subclass-listener handlers must fire exactly once per Send");
    }

    [Fact]
    public void Send_DispatchesToBothSubclassAndStrictListeners()
    {
        var messenger = new Messenger();
        var subclassReceiver = new object();
        var strictReceiver = new object();

        var subclassHits = 0;
        var strictHits = 0;

        messenger.Register<DerivedMessage>(
            subclassReceiver,
            receiveDerivedMessagesToo: true,
            _ => subclassHits++);

        messenger.Register<DerivedMessage>(
            strictReceiver,
            _ => strictHits++);

        messenger.Send(new DerivedMessage());

        subclassHits.Should().Be(1);
        strictHits.Should().Be(1);
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Required to verify weak-reference cleanup behaviour.")]
    public void Cleanup_RemovesDeadRecipients_AndDropsEmptyTypeKeys()
    {
        var messenger = new Messenger();

        RegisterAndAbandonRecipient<int>(messenger);
        ForceFullGarbageCollection();

        var typeMap = (System.Collections.IDictionary)GetPrivateField(messenger, "recipientsStrictAction")!;
        typeMap.Contains(typeof(int))
            .Should()
            .BeTrue("the type bucket exists until Cleanup runs");

        messenger.Cleanup();

        typeMap.Contains(typeof(int))
            .Should()
            .BeFalse("Cleanup must drop type keys whose recipient list is empty");
    }

    [Fact]
    public void IRecipient_Register_DispatchesMessageToRecipientReceive()
    {
        var messenger = new Messenger();
        var recipient = new RecordingRecipient();

        messenger.Register(recipient, keepTargetAlive: true);
        messenger.Send(new DerivedMessage());

        recipient.ReceivedMessages.Should().HaveCount(1);
    }

    [Fact]
    public void IRecipient_Register_WithToken_FiltersByToken()
    {
        var messenger = new Messenger();
        var alpha = new RecordingRecipient();
        var beta = new RecordingRecipient();
        var tokenA = new object();
        var tokenB = new object();

        messenger.Register(alpha, tokenA, keepTargetAlive: true);
        messenger.Register(beta, tokenB, keepTargetAlive: true);

        messenger.Send(new DerivedMessage(), tokenA);

        alpha.ReceivedMessages.Should().HaveCount(1);
        beta.ReceivedMessages.Should().BeEmpty();
    }

    [Fact]
    public void IRecipient_UnRegister_StopsFurtherDelivery()
    {
        var messenger = new Messenger();
        var recipient = new RecordingRecipient();

        messenger.Register(recipient, keepTargetAlive: true);
        messenger.Send(new DerivedMessage());
        messenger.UnRegister(recipient);
        messenger.Send(new DerivedMessage());

        recipient.ReceivedMessages.Should().HaveCount(1, "the second send must not reach the unregistered recipient");
    }

    [Fact]
    public void Send_DispatchesToAllRegisteredHandlersForMessageType()
    {
        var messenger = new Messenger();
        var receiverA = new object();
        var receiverB = new object();
        var receiverC = new object();
        var hitsA = 0;
        var hitsB = 0;
        var hitsC = 0;

        messenger.Register<DerivedMessage>(receiverA, _ => hitsA++);
        messenger.Register<DerivedMessage>(receiverB, _ => hitsB++);
        messenger.Register<DerivedMessage>(receiverC, _ => hitsC++);

        messenger.Send(new DerivedMessage());

        (hitsA, hitsB, hitsC).Should().Be((1, 1, 1));
    }

    [Fact]
    public void UnRegister_TargetingOneRecipient_LeavesOtherSubscriptionsIntact()
    {
        var messenger = new Messenger();
        var alpha = new object();
        var beta = new object();
        var alphaHits = 0;
        var betaHits = 0;

        messenger.Register<DerivedMessage>(alpha, _ => alphaHits++);
        messenger.Register<DerivedMessage>(beta, _ => betaHits++);

        messenger.UnRegister<DerivedMessage>(alpha);
        messenger.Send(new DerivedMessage());

        alphaHits.Should().Be(0, "unregistered recipient must not receive further messages");
        betaHits.Should().Be(1, "other recipients must remain subscribed");
    }

    [Fact]
    public void Send_WithToken_FiltersByToken_ForActionBasedRegistrations()
    {
        // Mirrors IRecipient_Register_WithToken_FiltersByToken but for the
        // Action<T>-based registration path.
        var messenger = new Messenger();
        var receiver = new object();
        var tokenA = new object();
        var tokenB = new object();
        var hitsA = 0;
        var hitsB = 0;

        messenger.Register<DerivedMessage>(receiver, tokenA, _ => hitsA++);
        messenger.Register<DerivedMessage>(receiver, tokenB, _ => hitsB++);

        messenger.Send(new DerivedMessage(), tokenA);

        hitsA.Should().Be(1);
        hitsB.Should().Be(0, "send is scoped to tokenA — tokenB subscribers must not fire");
    }

    [Fact]
    [SuppressMessage("Major Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Required to verify weak-reference cleanup behaviour.")]
    public void Send_DoesNotInvokeHandler_ForGarbageCollectedRecipient()
    {
        var messenger = new Messenger();
        var hits = 0;

        // Local helper keeps the recipient out of the calling stack frame so
        // GC can collect it once the helper returns.
        RegisterShortLivedHandler(messenger, () => hits++);
        ForceFullGarbageCollection();

        messenger.Send(new DerivedMessage());

        hits.Should().Be(0, "the registered recipient was collected — its handler must not fire");
    }

    [Fact]
    public void Send_DoesNotInvokeNewlyRegisteredRecipient_DuringSameDispatch()
    {
        // The dispatch path snapshots the recipient list before iterating, so a
        // recipient that registers itself from inside another recipient's
        // handler must NOT be invoked for the in-flight message — only for
        // subsequent sends.
        var messenger = new Messenger();
        var earlyHits = 0;
        var lateHits = 0;
        var earlyReceiver = new object();
        var lateReceiver = new object();

        messenger.Register<DerivedMessage>(
            earlyReceiver,
            _ =>
            {
                earlyHits++;

                // Register a second recipient mid-dispatch.
                if (earlyHits == 1)
                {
                    messenger.Register<DerivedMessage>(lateReceiver, _ => lateHits++);
                }
            });

        messenger.Send(new DerivedMessage());

        earlyHits.Should().Be(1, "the existing recipient fires once");
        lateHits.Should().Be(0, "the recipient registered mid-dispatch must not receive the in-flight message");

        messenger.Send(new DerivedMessage());

        earlyHits.Should().Be(2);
        lateHits.Should().Be(1, "a recipient registered mid-dispatch becomes active for subsequent sends");
    }

    [Fact]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Test thread captures any exception so the assertion can fail the test.")]
    [SuppressMessage("Usage", "xUnit1051:Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken", Justification = "Cancellation here is the test's internal time budget, not test-framework cancellation.")]
    public async Task Send_AndRegister_OnDifferentThreads_DoNotCorruptInternalState()
    {
        // Stress-test: spin one thread spamming Register while another spams
        // Send. Neither should throw, and the messenger's internal dictionaries
        // must remain consistent. The pre-fix `.Keys` enumeration in
        // SendToTargetOrType could throw InvalidOperationException under this
        // workload — the test pins the post-fix behaviour.
        var messenger = new Messenger();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var registerErrors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var sendErrors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var registerTask = Task.Run(() =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    var recipient = new object();
                    messenger.Register<DerivedMessage>(recipient, _ => { });
                    messenger.Register<UnrelatedMessage>(
                        recipient,
                        receiveDerivedMessagesToo: true,
                        _ => { });
                }
            }
            catch (Exception ex)
            {
                registerErrors.Add(ex);
            }
        });

        var sendTask = Task.Run(() =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    messenger.Send(new DerivedMessage());
                }
            }
            catch (Exception ex)
            {
                sendErrors.Add(ex);
            }
        });

        await Task.WhenAll(registerTask, sendTask);

        registerErrors.Should().BeEmpty("Register must not throw under concurrent Send");
        sendErrors.Should().BeEmpty("Send must not throw under concurrent Register");
    }

    [Fact]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Test thread captures any exception so the assertion can fail the test.")]
    [SuppressMessage("Usage", "xUnit1051:Calls to methods which accept CancellationToken should use TestContext.Current.CancellationToken", Justification = "Cancellation here is the test's internal time budget, not test-framework cancellation.")]
    [SuppressMessage("Reliability", "AsyncFixer02:Long-running or blocking operations inside an async method", Justification = "Synchronous test setup; the work below is genuinely async via Task.Run.")]
    public async Task Send_AndUnRegister_OnDifferentThreads_DoNotCorruptInternalState()
    {
        // Mirror of the register/send race: ensure UnRegister can run
        // concurrently with Send without throwing.
        var messenger = new Messenger();
        var recipients = new List<object>(32);
        for (var i = 0; i < 32; i++)
        {
            recipients.Add(new object());
        }

        foreach (var recipient in recipients)
        {
            messenger.Register<DerivedMessage>(recipient, _ => { });
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var unregisterErrors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var sendErrors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var unregisterTask = Task.Run(() =>
        {
            try
            {
                var index = 0;
                while (!cts.IsCancellationRequested)
                {
                    var recipient = recipients[index % recipients.Count];
                    messenger.UnRegister<DerivedMessage>(recipient);
                    messenger.Register<DerivedMessage>(recipient, _ => { });
                    index++;
                }
            }
            catch (Exception ex)
            {
                unregisterErrors.Add(ex);
            }
        });

        var sendTask = Task.Run(() =>
        {
            try
            {
                while (!cts.IsCancellationRequested)
                {
                    messenger.Send(new DerivedMessage());
                }
            }
            catch (Exception ex)
            {
                sendErrors.Add(ex);
            }
        });

        await Task.WhenAll(unregisterTask, sendTask);

        unregisterErrors.Should().BeEmpty("UnRegister must not throw under concurrent Send");
        sendErrors.Should().BeEmpty("Send must not throw under concurrent UnRegister");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RegisterShortLivedHandler(
        Messenger messenger,
        Action onMessage)
    {
        var recipient = new object();
        messenger.Register<DerivedMessage>(recipient, _ => onMessage());

        // recipient leaves scope — eligible for GC.
    }

    private sealed class DerivedMessage;

    private sealed class UnrelatedMessage;

    private sealed class RecordingRecipient : IRecipient<DerivedMessage>
    {
        public List<DerivedMessage> ReceivedMessages { get; } = [];

        public void Receive(DerivedMessage message)
        {
            ReceivedMessages.Add(message);
        }
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