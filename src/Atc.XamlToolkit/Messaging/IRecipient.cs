namespace Atc.XamlToolkit.Messaging;

/// <summary>
/// Represents a type that handles messages of a specific kind. Implement this on a view model (or
/// any class) instead of passing a lambda to <see cref="IMessenger"/> when you want compile-time
/// type-safety and a discoverable contract.
/// </summary>
/// <typeparam name="TMessage">The type of message this recipient handles.</typeparam>
/// <example>
/// <code>
/// public sealed class MainViewModel : ViewModelBase, IRecipient&lt;UserSignedInMessage&gt;
/// {
///     public MainViewModel()
///     {
///         MessengerInstance.Register(this);
///     }
///
///     public void Receive(UserSignedInMessage message)
///     {
///         // …handle the message…
///     }
/// }
/// </code>
/// </example>
public interface IRecipient<in TMessage>
{
    /// <summary>
    /// Called when a <typeparamref name="TMessage"/> is dispatched by the
    /// <see cref="IMessenger"/> this recipient registered with.
    /// </summary>
    /// <param name="message">The dispatched message.</param>
    void Receive(TMessage message);
}