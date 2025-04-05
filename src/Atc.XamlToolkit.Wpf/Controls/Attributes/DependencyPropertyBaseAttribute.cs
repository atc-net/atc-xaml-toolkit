namespace Atc.XamlToolkit.Controls.Attributes;

public abstract class DependencyPropertyBaseAttribute : Attribute
{
    public object? DefaultValue { get; set; }

    public string? PropertyChangedCallback { get; set; }

    public string? CoerceValueCallback { get; set; }

    public string? ValidateValueCallback { get; set; }

    public FrameworkPropertyMetadataOptions Flags { get; set; }

    public UpdateSourceTrigger DefaultUpdateSourceTrigger { get; set; }

    public bool IsAnimationProhibited { get; set; }

    public string? Category { get; set; }

    public string? Description { get; set; }

    public override string ToString()
        => $"{nameof(DefaultValue)}: {DefaultValue}, {nameof(PropertyChangedCallback)}: {PropertyChangedCallback}, {nameof(CoerceValueCallback)}: {CoerceValueCallback}, {nameof(ValidateValueCallback)}: {ValidateValueCallback}, {nameof(Flags)}: {Flags}, {nameof(DefaultUpdateSourceTrigger)}: {DefaultUpdateSourceTrigger}, {nameof(IsAnimationProhibited)}: {IsAnimationProhibited}, {nameof(Category)}: {Category}, {nameof(Description)}: {Description}";
}