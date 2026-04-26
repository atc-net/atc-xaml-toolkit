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
/// <remarks>
/// Builds on <see cref="ObservableValidator"/> (INotifyPropertyChanged + INotifyDataErrorInfo) by
/// adding the UI-state members (<see cref="IsEnabled"/>, <see cref="IsVisible"/>,
/// <see cref="IsBusy"/>, <see cref="IsDirty"/>, <see cref="IsSelected"/>) and a
/// <see cref="MessengerInstance"/> for decoupled communication. If you need only validation +
/// INPC and not the UI state, derive from <see cref="ObservableValidator"/> directly.
/// </remarks>
public abstract class ViewModelBase : ObservableValidator, IViewModelBase
{
    private bool isEnabled;
    private bool isVisible;
    private bool isBusy;
    private bool isDirty;
    private bool isSelected;

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
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value)
            {
                return;
            }

            isEnabled = value;
            RaisePropertyChanged();
        }
    }

    /// <inheritdoc />
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
    protected IMessenger MessengerInstance { get; init; }

    /// <inheritdoc />
    public Task SetIsBusy(
        bool value,
        ushort delayInMs = 1)
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
    public void Broadcast<T>(
        string propertyName,
        T oldValue,
        T newValue)
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

    /// <inheritdoc />
    protected override bool ShouldSkipValidationOnPropertyChanged(
        string propertyName)
        => base.ShouldSkipValidationOnPropertyChanged(propertyName)
           || propertyName is nameof(IsEnabled)
                            or nameof(IsVisible)
                            or nameof(IsBusy)
                            or nameof(IsDirty)
                            or nameof(IsSelected);
}