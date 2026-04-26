// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Bridges any <see cref="EventHandler{TEventArgs}"/>-style event with a
/// <b>weak reference to the subscriber</b>. Use it when you need a long-lived
/// event source (a model, a static service) to notify a short-lived consumer
/// (a view-model, a control) without the source pinning the consumer in memory.
/// </summary>
/// <typeparam name="TSubscriber">The class that owns the handler logic.</typeparam>
/// <typeparam name="TEventArgs">The event-args type the source delivers.</typeparam>
/// <remarks>
/// <para>
/// Lifetime: the listener registers a strong handler with the event source —
/// that handler closes over a <see cref="WeakReference"/> to the subscriber. When
/// the next event fires, if the subscriber has been garbage-collected, the
/// listener auto-unsubscribes itself from the source and becomes inert.
/// </para>
/// <para>
/// Eager teardown: dispose the listener (or let its <see cref="IDisposable"/> token
/// leave scope) to remove the handler immediately rather than wait for the next
/// raise. Disposal is idempotent.
/// </para>
/// <para>
/// The <c>onEvent</c> callback receives the live subscriber as its first argument
/// — <b>do not</b> capture <c>this</c> in the lambda body, or you defeat the
/// weak-reference contract. Pass a static lambda <c>(s, sender, args) =&gt; s.…</c>.
/// </para>
/// <para>
/// For the common cases (<c>INotifyCollectionChanged</c>,
/// <c>INotifyPropertyChanged</c>) prefer the sugar helpers
/// <see cref="WeakCollectionChangedListener"/> and
/// <see cref="WeakPropertyChangedListener"/>.
/// </para>
/// </remarks>
public sealed class WeakEventListener<TSubscriber, TEventArgs> : IDisposable
    where TSubscriber : class
    where TEventArgs : EventArgs
{
    private readonly WeakReference<TSubscriber> subscriberReference;
    private readonly Action<TSubscriber, object?, TEventArgs> onEvent;
    private Action<EventHandler<TEventArgs>>? unsubscribe;
    private EventHandler<TEventArgs>? handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakEventListener{TSubscriber, TEventArgs}"/> class
    /// and immediately subscribes it to the source's event.
    /// </summary>
    /// <param name="subscriber">The object whose handler logic will be invoked. Held weakly.</param>
    /// <param name="onEvent">
    /// The handler logic. The first argument is the live <paramref name="subscriber"/>;
    /// pass a static lambda to avoid accidentally capturing <c>this</c>.
    /// </param>
    /// <param name="subscribe">Delegate that wires a handler into the source's event (e.g. <c>h =&gt; src.Foo += h</c>).</param>
    /// <param name="unsubscribe">Delegate that removes a handler from the source's event (e.g. <c>h =&gt; src.Foo -= h</c>).</param>
    public WeakEventListener(
        TSubscriber subscriber,
        Action<TSubscriber, object?, TEventArgs> onEvent,
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(onEvent);
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        subscriberReference = new WeakReference<TSubscriber>(subscriber);
        this.onEvent = onEvent;
        this.unsubscribe = unsubscribe;

        handler = OnSourceEvent;
        subscribe(handler);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var localUnsubscribe = unsubscribe;
        var localHandler = handler;
        if (localUnsubscribe is null || localHandler is null)
        {
            return;
        }

        unsubscribe = null;
        handler = null;
        localUnsubscribe(localHandler);
    }

    private void OnSourceEvent(
        object? sender,
        TEventArgs args)
    {
        if (subscriberReference.TryGetTarget(out var subscriber))
        {
            onEvent(subscriber, sender, args);
            return;
        }

        // Subscriber was collected — auto-unsubscribe so we don't keep firing
        // into a dead reference and so the source can drop our handler.
        Dispose();
    }
}