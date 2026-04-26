// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.Mvvm;

/// <summary>
/// Assembly-level attribute that flips the per-property
/// <c>[ObservableProperty(GenerateDocumentation = …)]</c> default for the
/// containing assembly. When applied, every <c>[ObservableProperty]</c>
/// field that does not explicitly set the flag emits a default
/// <c>/// &lt;summary&gt;Gets or sets the {PropertyName}.&lt;/summary&gt;</c>
/// block above the generated property.
/// </summary>
/// <remarks>
/// <para>
/// Per-property <c>[ObservableProperty(GenerateDocumentation = false)]</c>
/// still wins — the assembly-level attribute only changes the default for
/// fields that don't specify the flag at all.
/// </para>
/// <para>
/// Example (in <c>AssemblyInfo.cs</c> or any file in the project):
/// <code language="csharp">
/// using Atc.XamlToolkit.Mvvm;
///
/// [assembly: GenerateDocumentationDefault]
/// </code>
/// After this, every plain <c>[ObservableProperty]</c> in the assembly
/// gets a default summary on its generated property without writing
/// <c>GenerateDocumentation = true</c> on every field.
/// </para>
/// <para>
/// Fields whose backing field already carries XML doc comments still
/// have those propagated to the generated property — the assembly default
/// only affects the *fallback* path.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GenerateDocumentationDefaultAttribute : Attribute
{
}