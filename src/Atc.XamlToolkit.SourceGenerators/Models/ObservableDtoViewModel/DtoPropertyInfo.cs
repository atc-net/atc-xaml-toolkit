// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class DtoPropertyInfo(
    string name,
    string type,
    bool isRecordParameter = false,
    bool isReadOnly = false,
    List<string>? attributes = null,
    List<string>? documentationComments = null)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public bool IsRecordParameter { get; } = isRecordParameter;

    public bool IsReadOnly { get; } = isReadOnly;

    public List<string> Attributes { get; } = attributes ?? [];

    public List<string> DocumentationComments { get; } = documentationComments ?? [];

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(IsRecordParameter)}: {IsRecordParameter}, {nameof(IsReadOnly)}: {IsReadOnly}, {nameof(Attributes)}.Count: {Attributes.Count}, {nameof(DocumentationComments)}.Count: {DocumentationComments.Count}";
}