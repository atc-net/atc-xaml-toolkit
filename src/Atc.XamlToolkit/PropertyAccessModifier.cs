namespace Atc.XamlToolkit;

/// <summary>
/// The accessibility of a generated member. It is the type of the <c>SetterAccessModifier</c>
/// option of <c>[ObservableProperty]</c>, <c>[DependencyProperty]</c>, <c>[StyledProperty]</c>
/// and <c>[AttachedProperty]</c> (the setter, or the <c>Set{Name}</c> method), of their
/// <c>AccessModifier</c> option (the property identifier field and the property or accessor
/// methods), of <c>PropertyKeyAccessModifier</c> and of the <c>RaiseMethodAccessModifier</c>
/// option of <c>[RoutedEvent]</c>.
/// </summary>
public enum PropertyAccessModifier
{
    /// <summary>
    /// <c>public</c>. For <c>SetterAccessModifier</c>: the setter is as accessible as the
    /// property, <c>set</c>.
    /// </summary>
    Public = 0,

    /// <summary>
    /// <c>internal</c>, for a setter <c>internal set</c>.
    /// </summary>
    Internal = 1,

    /// <summary>
    /// <c>protected</c>, for a setter <c>protected set</c>.
    /// </summary>
    Protected = 2,

    /// <summary>
    /// <c>protected internal</c>, for a setter <c>protected internal set</c>.
    /// </summary>
    ProtectedInternal = 3,

    /// <summary>
    /// <c>private protected</c>, for a setter <c>private protected set</c>.
    /// </summary>
    PrivateProtected = 4,

    /// <summary>
    /// <c>private</c>, for a setter <c>private set</c>.
    /// </summary>
    Private = 5,
}