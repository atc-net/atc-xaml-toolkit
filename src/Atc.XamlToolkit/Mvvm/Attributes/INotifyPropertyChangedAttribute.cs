// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Marks a partial class so the source generator emits a minimal
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> implementation
/// directly on the class, without requiring inheritance from
/// <see cref="ObservableObject"/> or <see cref="ViewModelBase"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use when the target class already has a base class you cannot replace
/// (e.g., a domain entity, a DTO produced by a tool you don't own, or a
/// type from a third-party library that you wrap via partial). The generated
/// code adds:
/// </para>
/// <list type="bullet">
/// <item><description>The <c>PropertyChanged</c> event.</description></item>
/// <item><description>A <c>RaisePropertyChanged</c> method (so source-generated <c>[ObservableProperty]</c> setters resolve).</description></item>
/// <item><description>An <c>OnPropertyChanged</c> alias method.</description></item>
/// <item><description>A <c>Set&lt;T&gt;</c> helper for hand-written property setters.</description></item>
/// </list>
/// <para>
/// The generated <c>RaisePropertyChanged</c> shares the process-wide
/// <see cref="PropertyChangedEventArgsCache"/>, so allocation cost matches the
/// hand-written <see cref="ObservableObject"/> base class.
/// </para>
/// <para>
/// The class must be declared <c>partial</c>. The attribute can be combined with
/// <see cref="ObservablePropertyAttribute"/> on fields — the existing observable-property
/// generator emits <c>RaisePropertyChanged(...)</c> calls that resolve against the
/// scaffolding produced by this attribute.
/// </para>
/// <para>
/// Example:
/// <code language="csharp">
/// [INotifyPropertyChanged]
/// public partial class Customer
/// {
///     [ObservableProperty]
///     private string name = string.Empty;
/// }
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Name intentionally mirrors the System.ComponentModel.INotifyPropertyChanged interface for symmetry and parity with CommunityToolkit.Mvvm.")]
public sealed class INotifyPropertyChangedAttribute : Attribute;