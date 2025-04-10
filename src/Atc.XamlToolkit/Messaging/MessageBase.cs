// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Atc.XamlToolkit.Messaging;

/// <summary>
/// Base class for all messages broadcast by the Messenger.
/// You can create your own message types by extending this class.
/// </summary>
public class MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class.
    /// </summary>
    /// <param name="sender">The message's original sender.</param>
    public MessageBase(object sender)
    {
        Sender = sender;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class.
    /// </summary>
    /// <param name="sender">The message's original sender.</param>
    /// <param name="target">The message's intended target. This parameter can be used
    /// to give an indication as to whom the message was intended for. Of course
    /// this is only an indication, amd may be null.</param>
    public MessageBase(object sender, object target)
        : this(sender)
    {
        Target = target;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBase"/> class.
    /// </summary>
    protected MessageBase()
    {
    }

    /// <summary>
    /// Gets or sets the message's sender.
    /// </summary>
    public object? Sender { get; protected set; }

    /// <summary>
    /// Gets or sets the message's intended target. This property can be used
    /// to give an indication as to whom the message was intended for. Of course
    /// this is only an indication, amd may be null.
    /// </summary>
    public object? Target { get; protected set; }
}