namespace Atc.XamlToolkit.SourceGenerators.Models.ViewModel;

/// <summary>
/// Carrier for the data the <c>[INotifyPropertyChanged]</c> emission pipeline needs:
/// the namespace, class name, and access modifier of the partial class that should
/// gain the INPC scaffolding (event + RaisePropertyChanged + OnPropertyChanged + Set&lt;T&gt;).
/// </summary>
/// <param name="NamespaceName">The namespace of the target class.</param>
/// <param name="ClassName">The name of the target class.</param>
/// <param name="AccessModifier">The access modifier of the target class.</param>
[SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "Name intentionally mirrors the System.ComponentModel.INotifyPropertyChanged interface for symmetry with the public attribute.")]
internal sealed record INotifyPropertyChangedTarget(
    string NamespaceName,
    string ClassName,
    string AccessModifier)
{
    public string TypeParameterList { get; set; } = string.Empty;

    public EquatableArray<string> ContainingTypeDeclarations { get; set; } = EquatableArray<string>.Empty;

    public string? HintName { get; set; }
}