namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Messaging;

/// <summary>
/// Aggregate view-model for the Messenger sample. Owns one sender and two
/// receivers (delegate-based + IRecipient&lt;T&gt;) — both subscribe to the same
/// process-wide <c>Messenger.Default</c> so a single Send hits both.
/// </summary>
public sealed class MessengerViewModel
{
    public SenderViewModel Sender { get; } = new();

    public ReceiverViewModel DelegateReceiver { get; } = new();

    public TypedReceiverViewModel TypedReceiver { get; } = new();
}