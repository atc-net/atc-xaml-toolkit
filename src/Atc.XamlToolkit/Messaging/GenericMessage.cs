// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Atc.XamlToolkit.Messaging;

/// <summary>
/// Passes a generic value (Content) to a recipient.
/// </summary>
/// <typeparam name="T">The type of the Content property.</typeparam>
public class GenericMessage<T> : MessageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericMessage{T}"/> class.
    /// </summary>
    /// <param name="content">The message content.</param>
    public GenericMessage(T content)
    {
        Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericMessage{T}"/> class.
    /// </summary>
    /// <param name="sender">The message's sender.</param>
    /// <param name="content">The message content.</param>
    public GenericMessage(object sender, T content)
        : base(sender)
    {
        Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericMessage{T}"/> class.
    /// </summary>
    /// <param name="sender">The message's sender.</param>
    /// <param name="target">The message's target.</param>
    /// <param name="content">The message content.</param>
    public GenericMessage(object sender, object target, T content)
        : base(sender, target)
    {
        Content = content;
    }

    /// <summary>
    /// Gets or sets the message's content.
    /// </summary>
    public T Content { get; protected set; }
}