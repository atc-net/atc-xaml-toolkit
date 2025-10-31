namespace Atc.XamlToolkit.Mvvm;

[SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
public interface IViewModelBase : IObservableObject, ICleanup, INotifyDataErrorInfo
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if this instance is enabled; otherwise, <see langword="false" />.
    /// </value>
    bool IsEnabled { get; set; }

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
    /// Asynchronously waits until <see cref="IsBusy"/> becomes
    /// <see langword="false"/> or a default timeout of <c>30 seconds</c> elapses.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the view-model became idle within the default
    /// timeout; otherwise <see langword="false"/> if the wait timed out.
    /// </returns>
    /// <remarks>
    /// Internally this overload delegates to
    /// <see cref="WaitUntilNotBusy(TimeSpan, ushort)"/> using a timeout of
    /// <c>TimeSpan.FromSeconds(30)</c> and a polling interval of
    /// <c>100 ms</c>.
    /// Call that overload if you need a different timeout or polling cadence.
    /// </remarks>
    Task<bool> WaitUntilNotBusy();

    /// <summary>
    /// Repeatedly polls <see cref="IsBusy"/> until it becomes
    /// <see langword="false"/> or the specified <paramref name="timeout"/> expires.
    /// </summary>
    /// <param name="timeout">The maximum amount of time to wait before giving up.</param>
    /// <param name="pollInMs">
    /// The polling interval, in milliseconds.
    /// Defaults to <c>100 ms</c>; keep it small enough for a responsive
    /// UI but large enough to avoid wasting CPU.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the view-model became idle within the
    /// timeout; <see langword="false"/> if the wait timed out.
    /// </returns>
    /// <remarks>
    /// Typical usage:
    /// <code>
    /// if (!await WaitUntilNotBusy(TimeSpan.FromSeconds(30)))
    ///     logger.Warn("Timed out waiting for 'something' to become idle.");
    /// </code>
    /// </remarks>
    Task<bool> WaitUntilNotBusy(
        TimeSpan timeout,
        ushort pollInMs = 100);

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