namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// A base class for objects of which the properties must be observable.
/// </summary>
/// <seealso cref="INotifyPropertyChanged" />
public abstract class ObservableObject : IObservableObject
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public void RaisePropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        VerifyPropertyName(propertyName);
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <inheritdoc />
    public void RaisePropertyChanged<T>(
        Expression<Func<T>> propertyExpression)
    {
        var handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        var propertyName = GetPropertyName(propertyExpression);
        if (!string.IsNullOrEmpty(propertyName))
        {
            RaisePropertyChanged(propertyName);
        }
    }

    /// <inheritdoc />
    public void VerifyPropertyName(
        string? propertyName)
    {
        var info = GetType().GetTypeInfo();
        if (string.IsNullOrEmpty(propertyName) ||
            info.GetDeclaredProperty(propertyName) is not null)
        {
            return;
        }

        // Check base types
        var found = false;
        while (info.BaseType is not null && info.BaseType != typeof(object))
        {
            info = info.BaseType.GetTypeInfo();
            if (info.GetDeclaredProperty(propertyName) is null)
            {
                continue;
            }

            found = true;
            break;
        }

        if (!found)
        {
            throw new ArgumentException("Property not found", propertyName);
        }
    }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>The name of the property.</returns>
    protected static string GetPropertyName<T>(
        Expression<Func<T>> propertyExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        if (propertyExpression.Body is not MemberExpression body)
        {
            throw new ArgumentException("Invalid argument", nameof(propertyExpression));
        }

        if (body.Member is not PropertyInfo property)
        {
            throw new ArgumentException("Argument is not a property", nameof(propertyExpression));
        }

        return property.Name;
    }

    /// <summary>
    /// Called when property changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    [SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "OK.")]
    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        VerifyPropertyName(propertyName);
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool Set<T>(
        Expression<Func<T>> propertyExpression,
        ref T field,
        T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        RaisePropertyChanged(propertyExpression);
        return true;
    }

    protected bool Set<T>(
        string? propertyName,
        ref T field,
        T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        RaisePropertyChanged(propertyName);
        return true;
    }

    protected bool Set<T>(
        ref T field,
        T newValue,
        [CallerMemberName] string? propertyName = null)
    {
        return Set(propertyName, ref field, newValue);
    }
}