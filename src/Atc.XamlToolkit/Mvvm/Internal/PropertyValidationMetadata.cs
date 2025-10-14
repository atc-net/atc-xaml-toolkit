namespace Atc.XamlToolkit.Mvvm.Internal;

internal sealed class PropertyValidationMetadata
{
    public PropertyInfo Property { get; init; } = null!;

    public List<System.ComponentModel.DataAnnotations.ValidationAttribute> ValidationAttributes { get; init; } = [];
}