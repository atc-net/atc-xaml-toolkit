namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class VariableDeclaratorSyntaxExtensions
{
    public static string GetFieldName(
        this VariableDeclaratorSyntax variableDeclaratorSyntax)
        => variableDeclaratorSyntax.Identifier.Text.RemovePrefixFromField();

    public static bool HasValidBackingFieldName(
        this VariableDeclaratorSyntax variableDeclaratorSyntax)
    {
        var fieldName = variableDeclaratorSyntax.GetFieldName();
        return fieldName.Length > 0 &&
               char.IsLower(fieldName[0]);
    }
}