namespace Atc.XamlToolkit.Messaging;

/// <summary>
/// Sugar over <see cref="IMessenger"/> for the <see cref="IRecipient{TMessage}"/> pattern. Register
/// a recipient by passing <c>this</c> instead of a lambda. The underlying weak-action plumbing is
/// unchanged, so lifetime semantics (recipient may be GC'd unless <c>keepTargetAlive: true</c>) are
/// identical to the lambda-based <c>Register</c> overloads.
/// </summary>
public static class MessengerExtensions
{
    /// <summary>
    /// Registers a recipient for messages of type <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The type of message the recipient handles.</typeparam>
    /// <param name="messenger">The messenger.</param>
    /// <param name="recipient">The recipient. Its <see cref="IRecipient{TMessage}.Receive"/> method is invoked when a matching message is sent.</param>
    /// <param name="keepTargetAlive">If <see langword="true"/>, prevents the recipient from being garbage-collected for the duration of the registration.</param>
    public static void Register<TMessage>(
        this IMessenger messenger,
        IRecipient<TMessage> recipient,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.Register<TMessage>(recipient, recipient.Receive, keepTargetAlive);
    }

    /// <summary>
    /// Registers a recipient for messages of type <typeparamref name="TMessage"/> on a specific token channel.
    /// </summary>
    /// <typeparam name="TMessage">The type of message the recipient handles.</typeparam>
    /// <param name="messenger">The messenger.</param>
    /// <param name="recipient">The recipient.</param>
    /// <param name="token">A token identifying the channel; only senders using the same token are routed to this recipient.</param>
    /// <param name="keepTargetAlive">If <see langword="true"/>, prevents the recipient from being garbage-collected for the duration of the registration.</param>
    public static void Register<TMessage>(
        this IMessenger messenger,
        IRecipient<TMessage> recipient,
        object? token,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.Register<TMessage>(recipient, token, recipient.Receive, keepTargetAlive);
    }

    /// <summary>
    /// Registers a recipient for messages of type <typeparamref name="TMessage"/> with the option to
    /// also receive derived message types.
    /// </summary>
    /// <typeparam name="TMessage">The type of message the recipient handles.</typeparam>
    /// <param name="messenger">The messenger.</param>
    /// <param name="recipient">The recipient.</param>
    /// <param name="receiveDerivedMessagesToo">If <see langword="true"/>, message types derived from <typeparamref name="TMessage"/> (or implementing it, when it is an interface) are also delivered.</param>
    /// <param name="keepTargetAlive">If <see langword="true"/>, prevents the recipient from being garbage-collected for the duration of the registration.</param>
    public static void Register<TMessage>(
        this IMessenger messenger,
        IRecipient<TMessage> recipient,
        bool receiveDerivedMessagesToo,
        bool keepTargetAlive = false)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.Register<TMessage>(recipient, receiveDerivedMessagesToo, recipient.Receive, keepTargetAlive);
    }

    /// <summary>
    /// Unregisters a recipient registered through one of the <see cref="IRecipient{TMessage}"/>-based
    /// <c>Register</c> overloads.
    /// </summary>
    /// <typeparam name="TMessage">The type of message the recipient was registered for.</typeparam>
    /// <param name="messenger">The messenger.</param>
    /// <param name="recipient">The recipient.</param>
    public static void UnRegister<TMessage>(
        this IMessenger messenger,
        IRecipient<TMessage> recipient)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(recipient);

        messenger.UnRegister<TMessage>(recipient, recipient.Receive);
    }
}