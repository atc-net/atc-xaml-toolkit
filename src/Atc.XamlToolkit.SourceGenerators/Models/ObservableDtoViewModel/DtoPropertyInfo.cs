// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ObservableDtoViewModel;

internal sealed class DtoPropertyInfo(
    string name,
    string type,
    bool isRecordParameter = false,
    bool isReadOnly = false)
{
    public string Name { get; } = name;

    public string Type { get; } = type;

    public bool IsRecordParameter { get; } = isRecordParameter;

    public bool IsReadOnly { get; } = isReadOnly;

    public override string ToString()
        => $"{nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(IsRecordParameter)}: {IsRecordParameter}, {nameof(IsReadOnly)}: {IsReadOnly}";
}