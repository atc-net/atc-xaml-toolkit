namespace Atc.XamlToolkit.WpfSample.SampleControls.Commands;

/// <summary>
/// Demonstrates the canonical sync <c>[RelayCommand]</c> + <c>CanExecute</c> patterns —
/// distinct from the async-cancellation flavours in the sibling samples.
/// </summary>
/// <remarks>
/// <para>
/// Two patterns are shown here:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <b>Parameterless commands</b> (<c>Increment</c>, <c>Decrement</c>, <c>Reset</c>) — generated
/// as <c>IRelayCommand</c>. <c>Decrement</c> and <c>Reset</c> use <c>CanExecute = nameof(...)</c>
/// to gate on the current counter value. The <c>[NotifyCanExecuteChangedFor(...)]</c> attributes
/// on <c>counter</c> raise the commands' CanExecuteChanged automatically when the counter
/// changes — no manual <c>CommandManager</c> plumbing required.
/// </description>
/// </item>
/// <item>
/// <description>
/// <b>Parameterized command</b> (<c>AddValue</c>) — generated as <c>IRelayCommand&lt;int&gt;</c>.
/// Bind with <c>CommandParameter="5"</c> in XAML; the value flows to the method via
/// the parameter. <c>CanAddValue</c> sees the same parameter and can return
/// per-value enable state (here: never zero).
/// </description>
/// </item>
/// </list>
/// </remarks>
public partial class CounterViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DecrementCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    private int counter;

    [RelayCommand]
    private void Increment()
        => Counter++;

    [RelayCommand(CanExecute = nameof(CanDecrement))]
    private void Decrement()
        => Counter--;

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
        => Counter = 0;

    [RelayCommand(CanExecute = nameof(CanAddValue))]
    private void AddValue(int amount)
        => Counter += amount;

    private bool CanDecrement()
        => Counter > 0;

    private bool CanReset()
        => Counter != 0;

    private static bool CanAddValue(int amount)
        => amount != 0;
}