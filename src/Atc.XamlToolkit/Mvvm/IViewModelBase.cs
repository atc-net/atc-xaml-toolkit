namespace Atc.XamlToolkit.Mvvm;

[SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
public interface IViewModelBase : IObservableObject, ICleanup
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is enabled; otherwise, <see langword="false" />.
    /// </value>
    bool IsEnable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is visible.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is visible; otherwise, <see langword="false" />.
    /// </value>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is busy.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is busy; otherwise, <see langword="false" />.
    /// </value>
    bool IsBusy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is dirty.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is dirty; otherwise, <see langword="false" />.
    /// </value>
    bool IsDirty { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is selected.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is selected; otherwise, <see langword="false" />.
    /// </value>
    bool IsSelected { get; set; }

    /// <summary>
    /// Asynchronously sets a value indicating whether this instance is busy.
    /// This method functions similarly to setting the <see cref="IsBusy"/> property directly, but introduces a small delay (default is 1 millisecond)
    /// to allow the UI thread time to update the busy indicator.
    /// </summary>
    /// <param name="value">A Boolean value indicating the busy state of the instance.</param>
    /// <param name="delayInMs">The delay in milliseconds before setting the busy state. Default is 1 ms.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// Example usage: <code>await SetIsBusy(true).ConfigureAwait(false);</code>
    /// </remarks>
    Task SetIsBusy(
        bool value,
        ushort delayInMs = 1);

    /// <summary>
    /// Broadcasts a change for the specified property from the old value to the new value.
    /// </summary>
    /// <typeparam name="T">The type of the property values.</typeparam>
    /// <param name="propertyName">The name of the property whose value has changed.</param>
    /// <param name="oldValue">The previous value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    void Broadcast<T>(
        string propertyName,
        T oldValue,
        T newValue);

    /// <summary>
    /// Raises the property changed event for the specified property and optionally broadcasts the change.
    /// </summary>
    /// <typeparam name="T">The type of the property values.</typeparam>
    /// <param name="propertyName">The name of the property that has changed.</param>
    /// <param name="oldValue">The previous value of the property (optional).</param>
    /// <param name="newValue">The new value of the property (optional).</param>
    /// <param name="broadcast">If set to <see langword="true" />, the change is broadcasted to subscribers.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="propertyName"/> is an empty string.
    /// </exception>
    void RaisePropertyChanged<T>(
        string propertyName,
        T? oldValue = default,
        T? newValue = default,
        bool broadcast = false);
}