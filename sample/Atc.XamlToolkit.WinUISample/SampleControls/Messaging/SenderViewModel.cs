namespace Atc.XamlToolkit.WinUISample.SampleControls.Messaging;

/// <summary>
/// Demonstrates <c>Messenger.Send</c>: a view-model broadcasts a typed message
/// every time the user clicks Send. The parameterless register/receive side
/// lives in <see cref="ReceiverViewModel"/> and <see cref="TypedReceiverViewModel"/>.
/// </summary>
public partial class SenderViewModel : ViewModelBase
{
    private const string DefaultSenderName = "Alice";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string draftText = "Hello, Messenger!";

    [ObservableProperty]
    private string senderName = DefaultSenderName;

    [ObservableProperty]
    private int messagesSent;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private void Send()
    {
        Messenger.Default.Send(new ChatMessage(SenderName, DraftText));

        MessagesSent++;
        DraftText = string.Empty;
    }

    private bool CanSend()
        => !string.IsNullOrWhiteSpace(DraftText);
}