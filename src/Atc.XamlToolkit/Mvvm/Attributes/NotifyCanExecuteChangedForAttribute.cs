// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Indicates that when the property generated for the decorated field changes, the named relay
/// command(s) should have <c>RaiseCanExecuteChanged()</c> called on them.
/// </summary>
/// <remarks>
/// <para>
/// This is the per-attribute companion to
/// <see cref="ObservablePropertyAttribute.DependentCommandNames"/>; both can be combined on the
/// same field.
/// </para>
/// <para>
/// Typical usage:
/// <code language="csharp">
/// public partial class CustomerViewModel : ViewModelBase
/// {
///     [ObservableProperty]
///     [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
///     private string customerName = string.Empty;
///
///     [RelayCommand(CanExecute = nameof(CanSave))]
///     private void Save() { /* … */ }
///
///     private bool CanSave() => !string.IsNullOrWhiteSpace(CustomerName);
/// }
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public sealed class NotifyCanExecuteChangedForAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyCanExecuteChangedForAttribute"/> class.
    /// </summary>
    /// <param name="dependentCommands">
    /// The names of the relay commands whose <c>CanExecute</c> should be re-evaluated when the
    /// property changes. Use <c>nameof(MyCommand)</c> for compile-time safety.
    /// </param>
    public NotifyCanExecuteChangedForAttribute(
        params string[] dependentCommands)
    {
        DependentCommands = dependentCommands;
    }

    /// <summary>
    /// Gets the dependent command names.
    /// </summary>
    public string[] DependentCommands { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(DependentCommands)}: {DependentCommands}";
}