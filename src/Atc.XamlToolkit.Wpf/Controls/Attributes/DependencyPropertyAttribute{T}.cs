namespace Atc.XamlToolkit.Controls.Attributes;

[SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "OK - inherits for AttachedPropertyAttribute")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DependencyPropertyAttribute<T>(string propertyName) : DependencyPropertyBaseAttribute
{
    public string? PropertyName { get; } = propertyName;

    public Type Type { get; } = typeof(T);

    public override string ToString()
        => $"{nameof(PropertyName)}: {PropertyName}, {nameof(Type)}: {Type}, {base.ToString()}";
}