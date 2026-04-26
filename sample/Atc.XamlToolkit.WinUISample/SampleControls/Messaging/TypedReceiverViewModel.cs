namespace Atc.XamlToolkit.WinUISample.SampleControls.Messaging;

/// <summary>
/// Demonstrates the <see cref="Atc.XamlToolkit.Messaging.IRecipient{TMessage}"/> pattern
/// — a view-model implements the typed interface and registers itself once with
/// <c>Messenger.Default.Register(this)</c>. The toolkit dispatches incoming messages
/// to <see cref="Receive"/> via the same weak-reference pipeline as the delegate-based
/// path.
/// </summary>
public partial class TypedReceiverViewModel : ViewModelBase, IRecipient<ChatMessage>
{
    [ObservableProperty]
    private ObservableCollection<string> log = [];

    [ObservableProperty]
    private int messagesReceived;

    public TypedReceiverViewModel()
    {
        Messenger.Default.Register<ChatMessage>(this);
    }

    public void Receive(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Log.Add(message.ToString());
        MessagesReceived++;
    }

    [RelayCommand]
    private void Clear()
    {
        Log.Clear();
        MessagesReceived = 0;
    }
}