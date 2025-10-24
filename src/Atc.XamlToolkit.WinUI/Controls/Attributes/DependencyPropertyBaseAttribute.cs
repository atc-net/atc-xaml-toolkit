namespace Atc.XamlToolkit.Controls.Attributes;

public abstract class DependencyPropertyBaseAttribute : Attribute
{
    public object? DefaultValue { get; set; }

    public string? PropertyChangedCallback { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public override string ToString()
        => $"{nameof(DefaultValue)}: {DefaultValue}, {nameof(PropertyChangedCallback)}: {PropertyChangedCallback}, {nameof(Category)}: {Category}, {nameof(Description)}: {Description}";
}