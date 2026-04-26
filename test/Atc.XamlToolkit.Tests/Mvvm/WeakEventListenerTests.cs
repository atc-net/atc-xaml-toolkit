// ReSharper disable RedundantAssignment
namespace Atc.XamlToolkit.Tests.Mvvm;

[SuppressMessage("Major Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "Required to verify weak-reference cleanup behaviour.")]
public sealed class WeakEventListenerTests
{
    [Fact]
    public void Subscribe_DispatchesEventToSubscriber()
    {
        var source = new TestEventSource();
        var subscriber = new TestSubscriber();

        using var listener = new WeakEventListener<TestSubscriber, TestEventArgs>(
            subscriber,
            (s, _, args) => s.Received.Add(args.Value),
            handler => source.Triggered += handler,
            handler => source.Triggered -= handler);

        source.Raise(new TestEventArgs(42));

        subscriber.Received.Should().Equal(42);
    }

    [Fact]
    public void Dispose_StopsDispatchingFutureEvents()
    {
        var source = new TestEventSource();
        var subscriber = new TestSubscriber();

        var listener = new WeakEventListener<TestSubscriber, TestEventArgs>(
            subscriber,
            (s, _, args) => s.Received.Add(args.Value),
            handler => source.Triggered += handler,
            handler => source.Triggered -= handler);

        source.Raise(new TestEventArgs(1));
        listener.Dispose();
        source.Raise(new TestEventArgs(2));

        subscriber.Received.Should().Equal(1);
    }

    [Fact]
    public void Subscriber_GarbageCollected_NextRaiseUnsubscribes()
    {
        var source = new TestEventSource();

        using var listener = SubscribeShortLivedSubscriber(source);
        ForceFullGarbageCollection();

        // The subscriber is gone — raising must not throw and the source must
        // have lost its handler reference (auto-cleanup on next raise).
        source.Raise(new TestEventArgs(99));

        source.HandlerCount.Should().Be(
            0,
            "the subscriber's handler must auto-unsubscribe once the weak target is collected");

        // Hold listener alive until the assertion to keep the test deterministic.
        GC.KeepAlive(listener);
    }

    [Fact]
    public void Constructor_WithNullSubscriber_Throws()
    {
        var source = new TestEventSource();

        var act = () => _ = new WeakEventListener<TestSubscriber, TestEventArgs>(
            subscriber: null!,
            onEvent: (s, _, _) => s.GetType(),
            subscribe: handler => source.Triggered += handler,
            unsubscribe: handler => source.Triggered -= handler);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Subscribe_InteropsWithNotifyCollectionChanged()
    {
        // The roadmap example: VM listens to a model collection without
        // leaking when the VM is GC'd.
        var collection = new ObservableCollection<int>();
        var subscriber = new TestSubscriber();

        using var listener = WeakCollectionChangedListener.Subscribe(
            collection,
            subscriber,
            (s, _, e) => s.Received.Add((int)e.Action));

        collection.Add(7);
        collection.Add(8);

        subscriber.Received.Should().Equal(
            (int)NotifyCollectionChangedAction.Add,
            (int)NotifyCollectionChangedAction.Add);
    }

    [Fact]
    public void Subscribe_InteropsWithNotifyPropertyChanged()
    {
        var source = new TestObservable();
        var subscriber = new TestSubscriber();

        using var listener = WeakPropertyChangedListener.Subscribe(
            source,
            subscriber,
            (s, _, e) => s.LastPropertyChanged = e.PropertyName);

        source.SetName("Alice");

        subscriber.LastPropertyChanged.Should().Be(nameof(TestObservable.Name));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IDisposable SubscribeShortLivedSubscriber(
        TestEventSource source)
    {
        var subscriber = new TestSubscriber();
        var listener = new WeakEventListener<TestSubscriber, TestEventArgs>(
            subscriber,
            (s, _, args) => s.Received.Add(args.Value),
            handler => source.Triggered += handler,
            handler => source.Triggered -= handler);

        // subscriber leaves scope — eligible for GC. The listener holds only
        // a WeakReference, so the JIT cannot keep the subscriber alive past
        // this method boundary.
        return listener;
    }

    private static void ForceFullGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private sealed class TestEventArgs(int value) : EventArgs
    {
        public int Value { get; } = value;
    }

    private sealed class TestEventSource
    {
        public event EventHandler<TestEventArgs>? Triggered;

        public int HandlerCount => Triggered?.GetInvocationList().Length ?? 0;

        public void Raise(TestEventArgs args)
            => Triggered?.Invoke(this, args);
    }

    private sealed class TestSubscriber
    {
        public List<int> Received { get; } = [];

        public string? LastPropertyChanged { get; set; }
    }

    private sealed class TestObservable : ObservableObject
    {
        private string? name;

        public string? Name
        {
            get => name;
            private set
            {
                if (name == value)
                {
                    return;
                }

                name = value;
                RaisePropertyChanged();
            }
        }

        public void SetName(string value) => Name = value;
    }
}