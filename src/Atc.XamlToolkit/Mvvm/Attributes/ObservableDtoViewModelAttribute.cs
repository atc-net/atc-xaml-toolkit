// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Indicates that a source generator should generate a ViewModel from the specified DTO type.
/// This attribute is applied to ViewModel classes to automatically generate properties
/// that correspond to the properties of the DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ObservableDtoViewModelAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDtoViewModelAttribute"/> class.
    /// </summary>
    /// <param name="dtoType">The type of the DTO to generate the ViewModel from.</param>
    public ObservableDtoViewModelAttribute(Type dtoType)
    {
        DtoType = dtoType;
    }

    /// <summary>
    /// Gets the DTO type that the ViewModel will be generated from.
    /// </summary>
    public Type DtoType { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated ViewModel should include
    /// an <c>IsDirty</c> property to track changes to its properties.
    /// </summary>
    public bool UseIsDirty { get; set; } = true;

    /// <summary>
    /// Gets or sets a collection of property names from the DTO that should be ignored
    /// when generating the ViewModel.
    /// </summary>
    public string[]? IgnorePropertyNames { get; set; }

    /// <summary>
    /// Gets or sets a collection of method names from the DTO that should be ignored
    /// when generating the ViewModel.
    /// </summary>
    public string[]? IgnoreMethodNames { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether validation should be enabled on property changed.
    /// </summary>
    public bool EnableValidationOnPropertyChanged { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether validation should be enabled during initialization.
    /// </summary>
    public bool EnableValidationOnInit { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(DtoType)}: {DtoType}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(EnableValidationOnPropertyChanged)}: {EnableValidationOnPropertyChanged}, {nameof(EnableValidationOnInit)}: {EnableValidationOnInit}";
}