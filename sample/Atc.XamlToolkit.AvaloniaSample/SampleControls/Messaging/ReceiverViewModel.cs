namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Messaging;

/// <summary>
/// Demonstrates the delegate-based <c>Messenger.Register&lt;TMessage&gt;(recipient, action)</c>
/// pattern. The view-model registers a handler on construction; the toolkit holds a
/// <see cref="WeakAction"/> internally so the receiver does not leak the messenger.
/// </summary>
public partial class ReceiverViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> log = [];

    [ObservableProperty]
    private int messagesReceived;

    public ReceiverViewModel()
    {
        Messenger.Default.Register<ChatMessage>(this, OnChatMessage);
    }

    [RelayCommand]
    private void Clear()
    {
        Log.Clear();
        MessagesReceived = 0;
    }

    private void OnChatMessage(ChatMessage message)
    {
        Log.Add(message.ToString());
        MessagesReceived++;
    }
}