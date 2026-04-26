// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Field-level companion attribute that opts an <see cref="ObservablePropertyAttribute"/>-decorated
/// field into inline validation. The source generator emits a call to
/// <c>ValidateProperty(value, nameof(PropertyName))</c> in the generated setter, immediately after
/// the field assignment and before <c>RaisePropertyChanged</c> fires — so listeners reading
/// <see cref="ObservableValidator.HasErrors"/> see the correct value when they react to the
/// property change.
/// </summary>
/// <remarks>
/// <para>
/// The class containing the decorated field <b>must inherit (transitively) from
/// <see cref="ObservableValidator"/></b> — either directly or via <see cref="ViewModelBase"/>.
/// The generator surfaces diagnostic <c>AtcXamlToolkit0009</c> when the requirement isn't met.
/// </para>
/// <para>
/// You do <i>not</i> need to call <see cref="ObservableValidator"/>'s
/// <c>InitializeValidation()</c> for this attribute to work — <c>ValidateProperty</c> lazy-builds
/// its metadata cache on first invocation. <c>InitializeValidation()</c> remains useful when you
/// want auto-validation on every property change (not just opt-in setters) or want to validate all
/// properties at construction time.
/// </para>
/// <para>
/// Combining <c>InitializeValidation(validateOnPropertyChanged: true)</c> with
/// <c>[NotifyDataErrorInfo]</c> is supported but slightly redundant: each setter validates twice
/// (once inline, once from the PropertyChanged handler). The result is correct; the cost is
/// negligible (cached metadata lookup). Pick one or the other if you care about minimal work.
/// </para>
/// <para>
/// Example:
/// <code language="csharp">
/// public partial class CustomerViewModel : ViewModelBase
/// {
///     [ObservableProperty]
///     [NotifyDataErrorInfo]
///     [Required(ErrorMessage = "First name is required")]
///     [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
///     private string firstName = string.Empty;
/// }
/// </code>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotifyDataErrorInfoAttribute : Attribute;