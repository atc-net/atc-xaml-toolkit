namespace Atc.XamlToolkit.SourceGenerators.Tests.Inspectors;

public sealed class ComputedPropertyInspectorTests
{
    [Fact]
    public void LinkToObservableProperties_NullObservableProperties_DoesNotThrow()
    {
        var computed = new List<ComputedPropertyToGenerate>
        {
            new("FullName", new EquatableArray<string>(["FirstName", "LastName"])),
        };

        var exception = Record.Exception(() =>
            ComputedPropertyInspector.LinkToObservableProperties(null, computed));

        Assert.Null(exception);
    }

    [Fact]
    public void LinkToObservableProperties_LinksMatchingDependencies()
    {
        var observables = new List<ObservablePropertyToGenerate>
        {
            new("FirstName", "string", "firstName", false),
            new("LastName", "string", "lastName", false),
        };

        var computed = new List<ComputedPropertyToGenerate>
        {
            new("FullName", new EquatableArray<string>(["FirstName", "LastName"])),
        };

        ComputedPropertyInspector.LinkToObservableProperties(observables, computed);

        Assert.False(observables[0].PropertyNamesToInvalidate.IsEmpty);
        Assert.Contains("FullName", observables[0].PropertyNamesToInvalidate, StringComparer.Ordinal);
        Assert.False(observables[1].PropertyNamesToInvalidate.IsEmpty);
        Assert.Contains("FullName", observables[1].PropertyNamesToInvalidate, StringComparer.Ordinal);
    }

    [Fact]
    public void LinkToObservableProperties_DoesNotLinkNonMatchingProperties()
    {
        var observables = new List<ObservablePropertyToGenerate>
        {
            new("Age", "int", "age", false),
        };

        var computed = new List<ComputedPropertyToGenerate>
        {
            new("FullName", new EquatableArray<string>(["FirstName", "LastName"])),
        };

        ComputedPropertyInspector.LinkToObservableProperties(observables, computed);

        Assert.True(observables[0].PropertyNamesToInvalidate.IsEmpty);
    }

    [Fact]
    public void LinkToObservableProperties_PreventsDuplicateLinks()
    {
        var observables = new List<ObservablePropertyToGenerate>
        {
            new("FirstName", "string", "firstName", false),
        };

        var computed = new List<ComputedPropertyToGenerate>
        {
            new("FullName", new EquatableArray<string>(["FirstName"])),
        };

        // Link twice
        ComputedPropertyInspector.LinkToObservableProperties(observables, computed);
        ComputedPropertyInspector.LinkToObservableProperties(observables, computed);

        Assert.False(observables[0].PropertyNamesToInvalidate.IsEmpty);
        Assert.Single(observables[0].PropertyNamesToInvalidate);
    }

    [Fact]
    public void LinkToObservableProperties_MultipleComputedProperties()
    {
        var observables = new List<ObservablePropertyToGenerate>
        {
            new("FirstName", "string", "firstName", false),
        };

        var computed = new List<ComputedPropertyToGenerate>
        {
            new("FullName", new EquatableArray<string>(["FirstName", "LastName"])),
            new("Initials", new EquatableArray<string>(["FirstName", "LastName"])),
        };

        ComputedPropertyInspector.LinkToObservableProperties(observables, computed);

        Assert.False(observables[0].PropertyNamesToInvalidate.IsEmpty);
        Assert.Equal(2, observables[0].PropertyNamesToInvalidate.Count);
        Assert.Contains("FullName", observables[0].PropertyNamesToInvalidate, StringComparer.Ordinal);
        Assert.Contains("Initials", observables[0].PropertyNamesToInvalidate, StringComparer.Ordinal);
    }

    [Fact]
    public void LinkToObservableProperties_EmptyLists_DoesNotThrow()
    {
        var observables = new List<ObservablePropertyToGenerate>();
        var computed = new List<ComputedPropertyToGenerate>();

        var exception = Record.Exception(() =>
            ComputedPropertyInspector.LinkToObservableProperties(observables, computed));

        Assert.Null(exception);
    }
}