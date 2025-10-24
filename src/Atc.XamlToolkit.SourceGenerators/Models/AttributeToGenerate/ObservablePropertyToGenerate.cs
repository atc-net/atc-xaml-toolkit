// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

internal sealed class ObservablePropertyToGenerate(
    string name,
    string type,
    string backingFieldName,
    bool isReadOnly)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public string BackingFieldName { get; } = backingFieldName;

    public bool IsReadOnly { get; } = isReadOnly;

    public ICollection<string>? PropertyNamesToInvalidate { get; set; }

    public ICollection<string>? CommandNamesToInvalidate { get; set; }

    public string? BeforeChangedCallback { get; set; }

    public string? AfterChangedCallback { get; set; }

    public bool BroadcastOnChange { get; set; }

    public bool UseIsDirty { get; set; }

    public List<string>? CustomAttributes { get; set; }

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(BackingFieldName)}: {BackingFieldName}, {nameof(PropertyNamesToInvalidate)}.Count: {PropertyNamesToInvalidate?.Count}, {nameof(CommandNamesToInvalidate)}.Count: {CommandNamesToInvalidate?.Count}, {nameof(BeforeChangedCallback)}: {BeforeChangedCallback}, {nameof(AfterChangedCallback)}: {AfterChangedCallback}, {nameof(BroadcastOnChange)}: {BroadcastOnChange}, {nameof(UseIsDirty)}: {UseIsDirty}";
}