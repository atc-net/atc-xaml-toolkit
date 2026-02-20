namespace Atc.XamlToolkit.SourceGenerators.Tests.Models;

public sealed class BaseFrameworkElementPropertyToGenerateTests
{
    [Fact]
    public void Create_DependencyPropertyToGenerate_ReturnsCorrectType()
    {
        var result = BaseFrameworkElementPropertyToGenerate.Create<DependencyPropertyToGenerate>(
            isOwnerTypeStatic: false,
            ownerType: "MyControl",
            name: "MyProp",
            type: "string",
            isReadOnly: false,
            useNewKeyword: false,
            defaultValue: null,
            propertyChangedCallback: null,
            coerceValueCallback: null,
            validateValueCallback: null,
            flags: null,
            defaultUpdateSourceTrigger: null,
            isAnimationProhibited: null,
            category: null,
            description: null);

        Assert.IsType<DependencyPropertyToGenerate>(result);
        Assert.Equal("MyControl", result.OwnerType);
        Assert.Equal("MyProp", result.Name);
        Assert.Equal("string", result.Type);
    }

    [Fact]
    public void Create_AttachedPropertyToGenerate_ReturnsCorrectType()
    {
        var result = BaseFrameworkElementPropertyToGenerate.Create<AttachedPropertyToGenerate>(
            isOwnerTypeStatic: true,
            ownerType: "MyHelper",
            name: "IsActive",
            type: "bool",
            isReadOnly: false,
            useNewKeyword: false,
            defaultValue: false,
            propertyChangedCallback: "OnIsActiveChanged",
            coerceValueCallback: null,
            validateValueCallback: null,
            flags: null,
            defaultUpdateSourceTrigger: null,
            isAnimationProhibited: null,
            category: null,
            description: null);

        Assert.IsType<AttachedPropertyToGenerate>(result);
        Assert.Equal("MyHelper", result.OwnerType);
        Assert.Equal("IsActive", result.Name);
        Assert.Equal("bool", result.Type);
        Assert.True(result.IsOwnerTypeStatic);
        Assert.NotNull(result.DefaultValue);
        Assert.False((bool)result.DefaultValue);
        Assert.Equal("OnIsActiveChanged", result.PropertyChangedCallback);
    }

    [Fact]
    public void Create_PreservesAllProperties()
    {
        var result = BaseFrameworkElementPropertyToGenerate.Create<DependencyPropertyToGenerate>(
            isOwnerTypeStatic: false,
            ownerType: "Owner",
            name: "Test",
            type: "int",
            isReadOnly: true,
            useNewKeyword: true,
            defaultValue: 42,
            propertyChangedCallback: "OnChanged",
            coerceValueCallback: "OnCoerce",
            validateValueCallback: "OnValidate",
            flags: "AffectsRender",
            defaultUpdateSourceTrigger: "PropertyChanged",
            isAnimationProhibited: true,
            category: "Layout",
            description: "A test property");

        Assert.False(result.IsOwnerTypeStatic);
        Assert.Equal("Owner", result.OwnerType);
        Assert.Equal("Test", result.Name);
        Assert.Equal("int", result.Type);
        Assert.True(result.IsReadOnly);
        Assert.True(result.UseNewKeyword);
        Assert.Equal(42, result.DefaultValue);
        Assert.Equal("OnChanged", result.PropertyChangedCallback);
        Assert.Equal("OnCoerce", result.CoerceValueCallback);
        Assert.Equal("OnValidate", result.ValidateValueCallback);
        Assert.Equal("AffectsRender", result.Flags);
        Assert.Equal("PropertyChanged", result.DefaultUpdateSourceTrigger);
        Assert.True(result.IsAnimationProhibited);
        Assert.Equal("Layout", result.Category);
        Assert.Equal("A test property", result.Description);
    }

    [Fact]
    public void HasAnyMetadata_WithDefaults_ReturnsFalse()
    {
        var result = BaseFrameworkElementPropertyToGenerate.Create<DependencyPropertyToGenerate>(
            isOwnerTypeStatic: false,
            ownerType: "Owner",
            name: "Test",
            type: "string",
            isReadOnly: false,
            useNewKeyword: false,
            defaultValue: null,
            propertyChangedCallback: null,
            coerceValueCallback: null,
            validateValueCallback: null,
            flags: null,
            defaultUpdateSourceTrigger: null,
            isAnimationProhibited: null,
            category: null,
            description: null);

        Assert.False(result.HasAnyMetadata);
    }

    [Fact]
    public void HasAnyMetadata_WithDefaultValue_ReturnsTrue()
    {
        var result = BaseFrameworkElementPropertyToGenerate.Create<DependencyPropertyToGenerate>(
            isOwnerTypeStatic: false,
            ownerType: "Owner",
            name: "Test",
            type: "string",
            isReadOnly: false,
            useNewKeyword: false,
            defaultValue: "hello",
            propertyChangedCallback: null,
            coerceValueCallback: null,
            validateValueCallback: null,
            flags: null,
            defaultUpdateSourceTrigger: null,
            isAnimationProhibited: null,
            category: null,
            description: null);

        Assert.True(result.HasAnyMetadata);
    }
}