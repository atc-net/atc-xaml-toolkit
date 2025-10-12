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
    /// Gets or set use of IsDirty for ViewModels.
    /// </summary>
    public bool UseIsDirty { get; set; } = true;

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(DtoType)}: {DtoType}, {nameof(UseIsDirty)}: {UseIsDirty}";
}