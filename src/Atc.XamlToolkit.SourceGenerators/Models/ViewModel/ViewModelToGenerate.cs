// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ViewModel;

internal sealed record ViewModelToGenerate(
    string NamespaceName,
    string ClassName,
    string? ClassAccessModifier,
    XamlPlatform XamlPlatform,
    EquatableArray<ObservablePropertyToGenerate> PropertiesToGenerate,
    EquatableArray<RelayCommandToGenerate> RelayCommandsToGenerate)
    : GenerateBase(NamespaceName, ClassName, ClassAccessModifier, IsStatic: false)
{
    public bool ContainsRelayCommandNameDuplicates
    {
        get
        {
            if (RelayCommandsToGenerate.IsEmpty)
            {
                return false;
            }

            var names = RelayCommandsToGenerate
                .Select(static x => x.CommandName)
                .ToArray();

            return names.Length != names
                .Distinct(StringComparer.Ordinal)
                .Count();
        }
    }
}