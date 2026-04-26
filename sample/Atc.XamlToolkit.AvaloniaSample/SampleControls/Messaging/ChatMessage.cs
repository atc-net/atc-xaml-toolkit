namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Messaging;

/// <summary>
/// Sample message broadcast through <see cref="Atc.XamlToolkit.Messaging.Messenger"/>
/// to demonstrate the publish/subscribe pattern.
/// </summary>
public sealed class ChatMessage(
    string sender,
    string text)
{
    public string Sender { get; } = sender;

    public string Text { get; } = text;

    public DateTime SentAt { get; } = DateTime.Now;

    public override string ToString()
        => $"[{SentAt:HH:mm:ss}] {Sender}: {Text}";
}