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
    /// When <see langword="true"/>, the generated ViewModel constructor calls
    /// <c>InitializeValidation(validateOnPropertyChanged: true)</c> on <see cref="ViewModelBase"/>,
    /// which copies any <see cref="System.ComponentModel.DataAnnotations.ValidationAttribute"/>
    /// declared on the DTO's properties onto the generated wrapper properties and re-validates
    /// each one whenever it changes.
    /// </summary>
    /// <remarks>
    /// Use this for live form-style validation as the user types. Combine with
    /// <see cref="EnableValidationOnInit"/> if you also want the DTO's current values validated
    /// the moment the wrapper is constructed (e.g., to disable a Save button while a fresh-loaded
    /// DTO already has invalid fields). Default <see langword="false"/> — validation is opt-in.
    /// </remarks>
    public bool EnableValidationOnPropertyChanged { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the generated ViewModel constructor calls
    /// <c>InitializeValidation(validateAllPropertiesOnInit: true)</c> on <see cref="ViewModelBase"/>,
    /// which validates every wrapped property exactly once at construction time so that
    /// <see cref="ObservableValidator.HasErrors"/> reflects the DTO's initial state immediately.
    /// </summary>
    /// <remarks>
    /// Independent of <see cref="EnableValidationOnPropertyChanged"/> — set both to validate at
    /// init **and** keep validating on each change. Default <see langword="false"/>.
    /// </remarks>
    public bool EnableValidationOnInit { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(DtoType)}: {DtoType}, {nameof(UseIsDirty)}: {UseIsDirty}, {nameof(EnableValidationOnPropertyChanged)}: {EnableValidationOnPropertyChanged}, {nameof(EnableValidationOnInit)}: {EnableValidationOnInit}";
}