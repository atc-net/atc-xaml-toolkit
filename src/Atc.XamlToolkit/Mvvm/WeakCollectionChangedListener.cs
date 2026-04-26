// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Sugar helper for subscribing to <see cref="INotifyCollectionChanged.CollectionChanged"/>
/// without holding a hard reference to the subscriber.
/// </summary>
/// <remarks>
/// Pairs naturally with view-model code that listens to a model collection
/// (e.g. an <c>ObservableCollection&lt;T&gt;</c>) for the lifetime of the model — the
/// view-model can be safely garbage-collected while the model lives on, and the
/// listener removes itself from the source on the next raise.
/// </remarks>
public static class WeakCollectionChangedListener
{
    /// <summary>
    /// Subscribes <paramref name="subscriber"/>'s callback to the source collection's
    /// <see cref="INotifyCollectionChanged.CollectionChanged"/> event. The returned
    /// <see cref="IDisposable"/> removes the subscription when disposed; otherwise
    /// the listener auto-cleans on the first raise after the subscriber is collected.
    /// </summary>
    /// <typeparam name="TSubscriber">The class that owns the handler logic.</typeparam>
    /// <param name="source">The collection raising the events.</param>
    /// <param name="subscriber">
    /// The object whose handler logic will run. <b>Held weakly.</b>
    /// </param>
    /// <param name="onChanged">
    /// The handler logic. The first argument is the live <paramref name="subscriber"/>;
    /// pass a static lambda to avoid accidentally capturing <c>this</c>.
    /// </param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes when disposed.</returns>
    public static IDisposable Subscribe<TSubscriber>(
        INotifyCollectionChanged source,
        TSubscriber subscriber,
        Action<TSubscriber, object?, NotifyCollectionChangedEventArgs> onChanged)
        where TSubscriber : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(onChanged);

        return new WeakEventListener<TSubscriber, NotifyCollectionChangedEventArgs>(
            subscriber,
            onChanged,
            handler => source.CollectionChanged += new NotifyCollectionChangedEventHandler(handler),
            handler => source.CollectionChanged -= new NotifyCollectionChangedEventHandler(handler));
    }
}