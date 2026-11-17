namespace Atc.XamlToolkit;

/// <summary>
/// The accessibility of a generated property setter, for the <c>SetterAccessModifier</c>
/// option of <c>[ObservableProperty]</c>, <c>[DependencyProperty]</c> and <c>[StyledProperty]</c>.
/// </summary>
public enum PropertyAccessModifier
{
    /// <summary>
    /// The setter is as accessible as the property: <c>set</c>.
    /// </summary>
    Public = 0,

    /// <summary>
    /// <c>internal set</c>.
    /// </summary>
    Internal = 1,

    /// <summary>
    /// <c>protected set</c>.
    /// </summary>
    Protected = 2,

    /// <summary>
    /// <c>protected internal set</c>.
    /// </summary>
    ProtectedInternal = 3,

    /// <summary>
    /// <c>private protected set</c>.
    /// </summary>
    PrivateProtected = 4,

    /// <summary>
    /// <c>private set</c>.
    /// </summary>
    Private = 5,
}