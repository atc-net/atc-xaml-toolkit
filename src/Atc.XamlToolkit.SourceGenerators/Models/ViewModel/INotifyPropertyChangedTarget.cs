namespace Atc.XamlToolkit.SourceGenerators.Models.ViewModel;

/// <summary>
/// Carrier for the data the <c>[INotifyPropertyChanged]</c> emission pipeline needs:
/// the namespace, class name, and access modifier of the partial class that should
/// gain the INPC scaffolding (event + RaisePropertyChanged + OnPropertyChanged + Set&lt;T&gt;).
/// </summary>
[SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Name intentionally mirrors the System.ComponentModel.INotifyPropertyChanged interface for symmetry with the public attribute.")]
internal sealed class INotifyPropertyChangedTarget(
    string namespaceName,
    string className,
    string accessModifier)
{
    public string NamespaceName { get; } = namespaceName;

    public string ClassName { get; } = className;

    public string AccessModifier { get; } = accessModifier;
}