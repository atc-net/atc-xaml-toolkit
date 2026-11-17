namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// A default value of a property typed <see cref="object"/> keeps the type it was written with.
/// </summary>
public sealed class ObjectDefaultValueTests
{
    [Fact]
    public void AStringDefaultValue_IsAString()
        => Assert.Equal("#", ObjectDefaultValueAttach.ButtonContentProperty.DefaultMetadata.DefaultValue);

    [Fact]
    public void ANumberDefaultValue_IsANumber()
        => Assert.Equal(3, ObjectDefaultValueAttach.CountProperty.DefaultMetadata.DefaultValue);
}