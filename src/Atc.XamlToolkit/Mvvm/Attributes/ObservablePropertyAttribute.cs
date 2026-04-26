// ReSharper disable RedundantAttributeUsageProperty
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Specifies a property in the ViewModel that should be generated for a field.
/// The class need to inherits from <see cref="IViewModelBase"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ObservablePropertyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservablePropertyAttribute"/> class.
    /// </summary>
    public ObservablePropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservablePropertyAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to generate</param>
    public ObservablePropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of property to generate.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Gets or sets the dependent property names.
    /// </summary>
    public string[]? DependentPropertyNames { get; set; }

    /// <summary>
    /// Gets or sets the dependent commands names.
    /// </summary>
    public string[]? DependentCommandNames { get; set; }

    /// <summary>
    /// Gets or sets the method(s) or expressions to execute before property changes.
    /// Example A: 'DoStuffA();'.
    /// Example B: nameof(DoStuffA).
    /// </summary>
    public string? BeforeChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets the method(s) or expressions to execute after property changes.
    /// Example A: 'EntrySelected?.Invoke(this, selectedEntry); DoStuffB();'.
    /// Example b: nameof(DoStuffB).
    /// </summary>
    public string? AfterChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated property should broadcast a property change message
    /// via the Messenger instance when the property changes.
    /// <para>
    /// Example usage in a ViewModel:
    /// <code language="csharp">
    /// public partial class PersonViewModel : ViewModelBase
    /// {
    ///     // When the 'FirstName' property changes, it will automatically broadcast a message.
    ///     [ObservableProperty(BroadcastOnChange = true)]
    ///     private string firstName = string.Empty;
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Example consumer registration to handle the broadcasted message:
    /// <code language="csharp">
    /// public partial class MyControl : UserControl
    /// {
    ///     public MyControl()
    ///     {
    ///         InitializeComponent();
    ///
    ///         // Register to receive notifications for changes to properties of type string.
    ///         Messenger.Default.Register&lt;PropertyChangedMessage&lt;string&gt;&gt;(this, OnPropertyChangedMessage);
    ///     }
    ///
    ///     private void OnPropertyChangedMessage(PropertyChangedMessage&lt;string&gt; message)
    ///     {
    ///         // Check if the message is for the "FirstName" property from the type PersonViewModel.
    ///         if (message.Sender?.GetType() == typeof(PersonViewModel) &amp;&amp;
    ///             message.PropertyName == nameof(PersonViewModel.FirstName))
    ///         {
    ///             var oldValue = message.OldValue;
    ///             var newValue = message.NewValue;
    ///             Debug.WriteLine($"PersonViewModel.FirstName: {oldValue} -> {newValue}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </summary>
    public bool BroadcastOnChange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated ViewModel should include
    /// an <c>IsDirty</c> property to track changes to its properties.
    /// </summary>
    public bool UseIsDirty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated property should be emitted with the
    /// C# 11+ <c>required</c> modifier, forcing callers to set it via an object initializer (or
    /// via a constructor annotated with <c>[SetsRequiredMembers]</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful when a ViewModel has properties that must be supplied at construction time (e.g.,
    /// dependencies injected via property initialization, or DTO-style ViewModels created from
    /// records).
    /// </para>
    /// <para>
    /// Example:
    /// <code language="csharp">
    /// public partial class CustomerViewModel : ViewModelBase
    /// {
    ///     [ObservableProperty(IsRequired = true)]
    ///     private string firstName = string.Empty;
    /// }
    ///
    /// // Caller:
    /// var vm = new CustomerViewModel { FirstName = "Ada" }; // OK
    /// var vm = new CustomerViewModel(); // CS9035 — required member 'FirstName' must be set.
    /// </code>
    /// </para>
    /// </remarks>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source generator should emit
    /// <c>partial void On{PropertyName}Changing({Type} value);</c> and
    /// <c>partial void On{PropertyName}Changed({Type} value);</c> declarations and call them from the
    /// generated setter — a compile-time-safe alternative to the string-named
    /// <see cref="BeforeChangedCallback"/> and <see cref="AfterChangedCallback"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see langword="true"/>, the generator emits two partial method <i>declarations</i>
    /// and unconditional calls to them inside the setter. If the consumer does not implement either
    /// partial method, the C# compiler elides the call entirely — so the feature has zero runtime
    /// cost when unused, but full type-safety and rename refactoring when used.
    /// </para>
    /// <para>
    /// Composes with <see cref="BeforeChangedCallback"/> / <see cref="AfterChangedCallback"/> if both
    /// are present: the partial-method calls fire first, then the string-named callbacks.
    /// </para>
    /// <para>
    /// Example:
    /// <code language="csharp">
    /// public partial class CustomerViewModel : ViewModelBase
    /// {
    ///     [ObservableProperty(GeneratePartialHooks = true)]
    ///     private string firstName = string.Empty;
    ///
    ///     // Optional — implement only the hooks you need.
    ///     partial void OnFirstNameChanged(string value)
    ///         => System.Diagnostics.Debug.WriteLine($"FirstName -> {value}");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public bool GeneratePartialHooks { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source generator should emit a default
    /// <c>/// &lt;summary&gt;Gets or sets the {PropertyName}.&lt;/summary&gt;</c> comment block
    /// above the generated property when the backing field has no XML documentation comments
    /// of its own.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Off by default to preserve byte-identical generated output for existing consumers.
    /// When the backing field already carries XML doc comments, those are propagated to the
    /// generated property regardless of this flag — this opt-in only governs the default-doc
    /// fallback. To turn it on globally for an entire project, see the project-level default
    /// follow-up tracked in the roadmap.
    /// </para>
    /// <para>
    /// Example:
    /// <code language="csharp">
    /// public partial class CustomerViewModel : ViewModelBase
    /// {
    ///     [ObservableProperty(GenerateDocumentation = true)]
    ///     private string firstName = string.Empty;
    /// }
    /// </code>
    /// emits
    /// <code language="csharp">
    /// /// &lt;summary&gt;Gets or sets the FirstName.&lt;/summary&gt;
    /// public string FirstName { get; set; }
    /// </code>
    /// </para>
    /// </remarks>
    public bool GenerateDocumentation { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(PropertyName)}: {PropertyName}, {nameof(DependentPropertyNames)}: {DependentPropertyNames}, {nameof(DependentCommandNames)}: {DependentCommandNames}, {nameof(BeforeChangedCallback)}: {BeforeChangedCallback}, {nameof(AfterChangedCallback)}: {AfterChangedCallback}, {nameof(BroadcastOnChange)}: {BroadcastOnChange}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(IsRequired)}: {IsRequired}, {nameof(GeneratePartialHooks)}: {GeneratePartialHooks}, {nameof(GenerateDocumentation)}: {GenerateDocumentation}";
}