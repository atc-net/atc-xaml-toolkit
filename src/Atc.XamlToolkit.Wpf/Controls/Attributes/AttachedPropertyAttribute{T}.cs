// ReSharper disable UnusedMember.Global
namespace Atc.XamlToolkit.Controls.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AttachedPropertyAttribute<T>(string propertyName) : DependencyPropertyAttribute<T>(propertyName)
{
    /// <summary>
    /// Gets or sets the type of the element parameter of the generated <c>Get{Name}</c> and
    /// <c>Set{Name}</c> methods. The default is <c>DependencyObject</c>.
    /// </summary>
    public Type? TargetType { get; set; }

    /// <summary>
    /// Gets or sets what the generated <c>Get{Name}</c> and <c>Set{Name}</c> methods do when
    /// they are called with a <see langword="null"/> element. The default is
    /// <see cref="NullElementHandling.Default"/>: <c>Set{Name}</c> does nothing.
    /// </summary>
    public NullElementHandling NullElementHandling { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the generated <c>Get{Name}</c> method gets
    /// <c>[AttachedPropertyBrowsableForType(typeof(TargetType))]</c>, so designers offer the
    /// property only on elements of <see cref="TargetType"/>. Has no effect without
    /// <see cref="TargetType"/>.
    /// </summary>
    public bool BrowsableForTargetType { get; set; }
}