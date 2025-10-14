// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable InvertIf
// ReSharper disable LocalizableElement
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// A base class for the ViewModel class, to be used in the MVVM pattern design.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IViewModelBase
{
    private readonly Dictionary<string, List<string>> errors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PropertyValidationMetadata> validationCache = new(StringComparer.Ordinal);
    private bool isEnable;
    private bool isVisible;
    private bool isBusy;
    private bool isDirty;
    private bool isSelected;

    /// <summary>
    /// Occurs when the validation errors have changed for a property or for the entire entity.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    protected ViewModelBase()
        : this(messenger: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    /// <param name="messenger">The messenger.</param>
    protected ViewModelBase(IMessenger? messenger)
        => MessengerInstance = messenger ?? Messenger.Default;

    public static Guid ViewModelId => Guid.NewGuid();

    /// <inheritdoc />
    public bool IsEnable
    {
        get => isEnable;
        set
        {
            if (isEnable == value)
            {
                return;
            }

            isEnable = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value)
            {
                return;
            }

            isVisible = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (isBusy == value)
            {
                return;
            }

            isBusy = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    public bool IsDirty
    {
        get => isDirty;
        set
        {
            if (isDirty == value)
            {
                return;
            }

            isDirty = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Gets the messenger instance.
    /// </summary>
    /// <value>
    /// The messenger instance.
    /// </value>
    protected IMessenger MessengerInstance { get; init; }

    /// <summary>
    /// Gets a value indicating whether the entity has validation errors.
    /// </summary>
    public bool HasErrors => errors.Count > 0;

    /// <summary>
    /// Initializes validation for the ViewModel.
    /// </summary>
    /// <param name="validateOnPropertyChanged">If true, automatically validates properties when they change.</param>
    /// <param name="validateAllPropertiesOnInit">If true, validates all properties immediately.</param>
    protected void InitializeValidation(
        bool validateOnPropertyChanged = true,
        bool validateAllPropertiesOnInit = false)
    {
        BuildValidationCache();

        if (validateOnPropertyChanged)
        {
            PropertyChanged -= OnPropertyChangedValidation;
            PropertyChanged += OnPropertyChangedValidation;
        }

        if (validateAllPropertiesOnInit)
        {
            ValidateAllProperties();
        }
    }

    /// <inheritdoc />
    public Task SetIsBusy(bool value, ushort delayInMs = 1)
    {
        IsBusy = value;

        // Give the UI a moment to refresh - show BusyIndicator.
        return Task.Delay(delayInMs, CancellationToken.None);
    }

    /// <inheritdoc />
    public Task<bool> WaitUntilNotBusy()
        => WaitUntilNotBusy(TimeSpan.FromSeconds(30));

    /// <inheritdoc />
    public async Task<bool> WaitUntilNotBusy(
        TimeSpan timeout,
        ushort pollInMs = 100)
    {
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            while (IsBusy)
            {
                await Task.Delay(pollInMs, cts.Token).ConfigureAwait(false);
            }

            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public virtual void Cleanup()
    {
        MessengerInstance.UnRegister(this);
    }

    /// <inheritdoc />
    public void Broadcast<T>(string propertyName, T oldValue, T newValue)
    {
        var message = new PropertyChangedMessage<T>(this, propertyName, oldValue, newValue);
        MessengerInstance.Send(message);
    }

    /// <inheritdoc />
    public void RaisePropertyChanged<T>(
        string propertyName,
        T? oldValue = default,
        T? newValue = default,
        bool broadcast = false)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException("This method cannot be called with an empty string", propertyName);
        }

        RaisePropertyChanged(propertyName);
        if (broadcast)
        {
            Broadcast(propertyName, oldValue, newValue);
        }
    }

    /// <summary>
    /// Gets the validation errors for a specified property or for the entire entity.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property to retrieve validation errors for;
    /// or <see langword="null"/> or <see cref="string.Empty"/>, to retrieve entity-level errors.
    /// </param>
    /// <returns>The validation errors for the property or entity.</returns>
    public System.Collections.IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return errors.SelectMany(x => x.Value);
        }

        return errors.TryGetValue(propertyName, out var propertyErrors)
            ? propertyErrors
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Validates a property value using data annotation attributes.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <returns><see langword="true"/> if the property is valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method uses <see cref="System.ComponentModel.DataAnnotations.Validator"/> to validate
    /// the property against any data annotation attributes defined on it.
    /// If validation fails, errors are stored and the <see cref="ErrorsChanged"/> event is raised.
    /// For properties generated by source generators (e.g., ObservableProperty), this method will
    /// look for validation attributes on the backing field (camelCase field name).
    /// </remarks>
    protected bool ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return true;
        }

        ClearErrors(propertyName);

        var validationAttributes = GetValidationAttributes(propertyName);

        if (validationAttributes.Count == 0)
        {
            return true;
        }

        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this)
        {
            MemberName = propertyName,
        };

        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = true;
        foreach (var attribute in validationAttributes)
        {
            var result = attribute.GetValidationResult(value, validationContext);
            if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success &&
                result is not null)
            {
                isValid = false;
                validationResults.Add(result);
            }
        }

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                AddError(propertyName, validationResult.ErrorMessage ?? "Validation error");
            }
        }

        return isValid;
    }

    /// <summary>
    /// Validates all properties on the ViewModel that have data annotation attributes.
    /// </summary>
    /// <returns><see langword="true"/> if all properties are valid; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method iterates through all public properties and validates them using <see cref="ValidateProperty"/>.
    /// It also checks private backing fields for validation attributes (useful for source-generated properties).
    /// Uses cached metadata for optimal performance.
    /// </remarks>
    protected bool ValidateAllProperties()
    {
        errors.Clear();
        var isValid = true;

        // Use cached metadata instead of reflection
        foreach (var (propertyName, metadata) in validationCache)
        {
            var value = metadata.Property.GetValue(this);
            var propertyIsValid = ValidateProperty(value, propertyName);
            isValid = isValid && propertyIsValid;
        }

        OnErrorsChanged(string.Empty);
        return isValid;
    }

    /// <summary>
    /// Adds a validation error for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="error">The error message.</param>
    protected void AddError(string propertyName, string error)
    {
        if (!errors.ContainsKey(propertyName))
        {
            errors[propertyName] = [];
        }

        if (errors[propertyName].Contains(error, StringComparer.Ordinal))
        {
            return;
        }

        errors[propertyName].Add(error);
        OnErrorsChanged(propertyName);
    }

    /// <summary>
    /// Clears all validation errors for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    protected void ClearErrors(string propertyName)
    {
        if (errors.Remove(propertyName))
        {
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears all validation errors for all properties.
    /// </summary>
    protected void ClearAllErrors()
    {
        if (errors.Count <= 0)
        {
            return;
        }

        errors.Clear();
        OnErrorsChanged(string.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        // Raise PropertyChanged for HasErrors so bindings and commands can react to validation state changes
        RaisePropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// Handles property changes and validates properties that have validation attributes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The property changed event arguments.</param>
    private void OnPropertyChangedValidation(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is null)
        {
            return;
        }

        // Skip validation for special properties
        if (args.PropertyName
            is nameof(HasErrors)
            or nameof(IsEnable)
            or nameof(IsVisible)
            or nameof(IsBusy)
            or nameof(IsDirty)
            or nameof(IsSelected))
        {
            return;
        }

        // Use cached metadata for fast lookup
        if (validationCache.TryGetValue(args.PropertyName, out var metadata))
        {
            var value = metadata.Property.GetValue(this);
            ValidateProperty(value, args.PropertyName);
        }
    }

    /// <summary>
    /// Builds the validation cache by scanning all properties for validation attributes.
    /// This is done once during initialization to avoid repeated reflection calls.
    /// </summary>
    private void BuildValidationCache()
    {
        var type = GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var validationAttributes = GetValidationAttributesForProperty(property);

            if (validationAttributes.Count > 0)
            {
                validationCache[property.Name] = new PropertyValidationMetadata
                {
                    Property = property,
                    ValidationAttributes = validationAttributes,
                };
            }
        }
    }

    /// <summary>
    /// Gets validation attributes for a specific property, including backing fields.
    /// </summary>
    /// <param name="property">The property to scan for validation attributes.</param>
    /// <returns>A list of validation attributes.</returns>
    [SuppressMessage("Design", "S3011:Make sure this accessibility bypass is safe here", Justification = "Intended.")]
    private List<System.ComponentModel.DataAnnotations.ValidationAttribute> GetValidationAttributesForProperty(PropertyInfo property)
    {
        var attributes = new List<System.ComponentModel.DataAnnotations.ValidationAttribute>();

        // Try to get attributes from the public property first
        var propertyAttributes = property
            .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.ValidationAttribute), inherit: true)
            .OfType<System.ComponentModel.DataAnnotations.ValidationAttribute>();
        attributes.AddRange(propertyAttributes);

        // If no attributes found, try the backing field (for source-generated properties)
        if (attributes.Count == 0)
        {
            var backingFieldName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
            var backingField = GetType().GetField(backingFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (backingField is not null)
            {
                var fieldAttributes = backingField
                    .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.ValidationAttribute), inherit: true)
                    .OfType<System.ComponentModel.DataAnnotations.ValidationAttribute>();
                attributes.AddRange(fieldAttributes);
            }
        }

        return attributes;
    }

    /// <summary>
    /// Gets all validation attributes for a property, including those on backing fields.
    /// Uses cached metadata for optimal performance.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A list of validation attributes.</returns>
    private List<System.ComponentModel.DataAnnotations.ValidationAttribute> GetValidationAttributes(string propertyName)
        => validationCache.TryGetValue(propertyName, out var metadata)
            ? metadata.ValidationAttributes
            : [];
}