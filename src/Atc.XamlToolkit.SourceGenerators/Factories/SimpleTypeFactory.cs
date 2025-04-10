namespace Atc.XamlToolkit.SourceGenerators.Factories;

public static class SimpleTypeFactory
{
    public static string? CreateDefaultValueAsStrForType(
        string type)
        => type switch
        {
            "bool" => "BooleanBoxes.FalseBox",
            "decimal" => "0m",
            "double" => "0d",
            "float" => "0f",
            "int" => "0",
            "long" => "0",
            _ => null,
        };
}