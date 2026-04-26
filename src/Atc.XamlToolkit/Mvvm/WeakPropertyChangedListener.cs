// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Sugar helper for subscribing to <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// without holding a hard reference to the subscriber.
/// </summary>
/// <remarks>
/// Useful when one view-model wants to react to changes in a longer-lived
/// model object: the model can outlive the view-model, and the listener cleans
/// itself up on the first raise after the view-model is collected.
/// </remarks>
public static class WeakPropertyChangedListener
{
    /// <summary>
    /// Subscribes <paramref name="subscriber"/>'s callback to the source's
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event. The returned
    /// <see cref="IDisposable"/> removes the subscription when disposed; otherwise
    /// the listener auto-cleans on the first raise after the subscriber is collected.
    /// </summary>
    /// <typeparam name="TSubscriber">The class that owns the handler logic.</typeparam>
    /// <param name="source">The object raising property-changed notifications.</param>
    /// <param name="subscriber">
    /// The object whose handler logic will run. <b>Held weakly.</b>
    /// </param>
    /// <param name="onChanged">
    /// The handler logic. The first argument is the live <paramref name="subscriber"/>;
    /// pass a static lambda to avoid accidentally capturing <c>this</c>.
    /// </param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes when disposed.</returns>
    public static IDisposable Subscribe<TSubscriber>(
        INotifyPropertyChanged source,
        TSubscriber subscriber,
        Action<TSubscriber, object?, PropertyChangedEventArgs> onChanged)
        where TSubscriber : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(subscriber);
        ArgumentNullException.ThrowIfNull(onChanged);

        return new WeakEventListener<TSubscriber, PropertyChangedEventArgs>(
            subscriber,
            onChanged,
            handler => source.PropertyChanged += new PropertyChangedEventHandler(handler),
            handler => source.PropertyChanged -= new PropertyChangedEventHandler(handler));
    }
}