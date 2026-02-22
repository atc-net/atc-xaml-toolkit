// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Atc.XamlToolkit.Tests.Metadata;

public sealed class PropertyMetadataDescriptorTests
{
    [Fact]
    public void GetDescriptors_WithNoDecoratedProperties_ShouldReturnEmpty()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<EmptyModel>();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetDescriptors_ShouldOnlyIncludeOptedInProperties()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.PropertyName == "Name");
        Assert.Contains(result, x => x.PropertyName == "Age");
        Assert.DoesNotContain(result, x => x.PropertyName == "NotIncluded");
    }

    [Fact]
    public void GetDescriptors_ShouldResolveDisplayName()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();

        // Assert
        var name = result.First(x => x.PropertyName == "Name");
        Assert.Equal("Full Name", name.DisplayName);
    }

    [Fact]
    public void GetDescriptors_ShouldFallbackToPropertyNameWhenDisplayNameIsNull()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<FallbackModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Title");
        Assert.Equal("Title", prop.DisplayName);
    }

    [Fact]
    public void GetDescriptors_ShouldFallbackToBclDisplayNameAttribute()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BclFallbackModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Code");
        Assert.Equal("Product Code", prop.DisplayName);
    }

    [Fact]
    public void GetDescriptors_ShouldResolveGroupName()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<GroupedModel>();

        // Assert
        var appearance = result.First(x => x.PropertyName == "Color");
        Assert.Equal("Appearance", appearance.GroupName);

        var behavior = result.First(x => x.PropertyName == "IsEnabled");
        Assert.Equal("Behavior", behavior.GroupName);
    }

    [Fact]
    public void GetDescriptors_ShouldDefaultGroupNameToGeneral()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();

        // Assert
        var name = result.First(x => x.PropertyName == "Name");
        Assert.Equal("General", name.GroupName);
    }

    [Fact]
    public void GetDescriptors_ShouldFallbackToBclCategoryAttribute()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BclFallbackModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Code");
        Assert.Equal("Inventory", prop.GroupName);
    }

    [Fact]
    public void GetDescriptors_ShouldResolveDescription()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();

        // Assert
        var age = result.First(x => x.PropertyName == "Age");
        Assert.Equal("The person's age", age.Description);
    }

    [Fact]
    public void GetDescriptors_ShouldFallbackToBclDescriptionAttribute()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BclFallbackModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Code");
        Assert.Equal("The product code", prop.Description);
    }

    [Fact]
    public void GetDescriptors_ShouldExcludePropertyBrowsableFalse()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BrowsableModel>();

        // Assert
        Assert.DoesNotContain(result, x => x.PropertyName == "Hidden");
        Assert.Contains(result, x => x.PropertyName == "Visible");
    }

    [Fact]
    public void GetDescriptors_ShouldExcludeBclBrowsableFalse()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BrowsableModel>();

        // Assert
        Assert.DoesNotContain(result, x => x.PropertyName == "BclHidden");
    }

    [Fact]
    public void GetDescriptors_ShouldSortByGroupThenOrderThenDisplayName()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<SortedModel>();

        // Assert
        Assert.Equal("Alpha", result[0].PropertyName);
        Assert.Equal("Beta", result[1].PropertyName);
        Assert.Equal("Delta", result[2].PropertyName);
        Assert.Equal("Gamma", result[3].PropertyName);
    }

    [Fact]
    public void GetDescriptors_ShouldDetectRangeFromPropertyRangeAttribute()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<RangeModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Opacity");
        Assert.True(prop.HasRange);
        Assert.Equal(0.0, prop.RangeMinimum);
        Assert.Equal(1.0, prop.RangeMaximum);
        Assert.Equal(0.1, prop.RangeStep);
    }

    [Fact]
    public void GetDescriptors_ShouldDetectRangeWithoutStep()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<RangeModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Volume");
        Assert.True(prop.HasRange);
        Assert.Equal(0.0, prop.RangeMinimum);
        Assert.Equal(100.0, prop.RangeMaximum);
        Assert.Null(prop.RangeStep);
    }

    [Fact]
    public void GetDescriptors_ShouldDetectEditorHint()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<EditorHintModel>();

        // Assert
        var slider = result.First(x => x.PropertyName == "SliderProp");
        Assert.Equal(EditorHint.Slider, slider.EditorHint);

        var color = result.First(x => x.PropertyName == "ColorProp");
        Assert.Equal(EditorHint.ColorPicker, color.EditorHint);
    }

    [Fact]
    public void GetDescriptors_ShouldDetectReadOnlyFromMissingSetter()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<ReadOnlyModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Computed");
        Assert.True(prop.IsReadOnly);
    }

    [Fact]
    public void GetDescriptors_ShouldDetectReadOnlyFromBclAttribute()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<ReadOnlyModel>();

        // Assert
        var prop = result.First(x => x.PropertyName == "Locked");
        Assert.True(prop.IsReadOnly);
    }

    [Fact]
    public void GetDescriptors_ShouldSetPropertyType()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();

        // Assert
        var name = result.First(x => x.PropertyName == "Name");
        Assert.Equal(typeof(string), name.PropertyType);

        var age = result.First(x => x.PropertyName == "Age");
        Assert.Equal(typeof(int), age.PropertyType);
    }

    [Fact]
    public void GetDescriptors_Generic_ShouldReturnSameAsNonGeneric()
    {
        // Act
        var generic = PropertyMetadataDescriptor.GetDescriptors<BasicModel>();
        var nonGeneric = PropertyMetadataDescriptor.GetDescriptors(typeof(BasicModel));

        // Assert
        Assert.Equal(generic.Count, nonGeneric.Count);
        for (var i = 0; i < generic.Count; i++)
        {
            Assert.Equal(generic[i].PropertyName, nonGeneric[i].PropertyName);
        }
    }

    [Fact]
    public void GetGroupedDescriptors_ShouldGroupByGroupName()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetGroupedDescriptors(typeof(GroupedModel));

        // Assert
        Assert.Equal(2, result.Count);

        var appearance = result.First(x => x.GroupName == "Appearance");
        Assert.Single(appearance.Properties);
        Assert.Equal("Color", appearance.Properties[0].PropertyName);

        var behavior = result.First(x => x.GroupName == "Behavior");
        Assert.Single(behavior.Properties);
        Assert.Equal("IsEnabled", behavior.Properties[0].PropertyName);
    }

    [Fact]
    public void GetGroupedDescriptors_WithNoProperties_ShouldReturnEmpty()
    {
        // Act
        var result = PropertyMetadataDescriptor.GetGroupedDescriptors(typeof(EmptyModel));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetDescriptors_WithNullType_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PropertyMetadataDescriptor.GetDescriptors(null!));
    }

    private sealed class EmptyModel
    {
        [SuppressMessage("S1144", "S1144", Justification = "Used by reflection in tests.")]
        public string? NotDecorated { get; set; }
    }

    private sealed class BasicModel
    {
        [PropertyDisplay("Full Name")]
        public string? Name { get; set; }

        [PropertyDisplay("Age", Description = "The person's age")]
        public int Age { get; set; }

        [SuppressMessage("S1144", "S1144", Justification = "Used by reflection in tests.")]
        public string? NotIncluded { get; set; }
    }

    private sealed class FallbackModel
    {
        [PropertyDisplay]
        public string? Title { get; set; }
    }

    private sealed class BclFallbackModel
    {
        [PropertyDisplay]
        [DisplayName("Product Code")]
        [Category("Inventory")]
        [Description("The product code")]
        public string? Code { get; set; }
    }

    private sealed class GroupedModel
    {
        [PropertyDisplay("Color", "Appearance")]
        public string? Color { get; set; }

        [PropertyDisplay("Enabled", "Behavior")]
        public bool IsEnabled { get; set; }
    }

    private sealed class BrowsableModel
    {
        [PropertyDisplay("Visible")]
        public string? Visible { get; set; }

        [PropertyDisplay("Hidden")]
        [PropertyBrowsable(false)]
        public string? Hidden { get; set; }

        [PropertyDisplay("BCL Hidden")]
        [Browsable(false)]
        public string? BclHidden { get; set; }
    }

    private sealed class SortedModel
    {
        // Group "A", Order 2
        [PropertyDisplay("Beta", "A", 2)]
        public string? Beta { get; set; }

        // Group "A", Order 1
        [PropertyDisplay("Alpha", "A", 1)]
        public string? Alpha { get; set; }

        // Group "B", Order MaxValue — sorted by display name
        [PropertyDisplay("Gamma", "B")]
        public string? Gamma { get; set; }

        // Group "B", Order MaxValue — sorted by display name (before Gamma)
        [PropertyDisplay("Delta", "B")]
        public string? Delta { get; set; }
    }

    private sealed class RangeModel
    {
        [PropertyDisplay("Opacity")]
        [PropertyRange(0.0, 1.0, 0.1)]
        public double Opacity { get; set; }

        [PropertyDisplay("Volume")]
        [PropertyRange(0, 100)]
        public int Volume { get; set; }
    }

    private sealed class EditorHintModel
    {
        [PropertyDisplay("Slider")]
        [PropertyEditorHint(EditorHint.Slider)]
        public double SliderProp { get; set; }

        [PropertyDisplay("Color")]
        [PropertyEditorHint(EditorHint.ColorPicker)]
        public string? ColorProp { get; set; }
    }

    private sealed class ReadOnlyModel
    {
        [PropertyDisplay("Computed")]
        public string Computed => "computed";

        [PropertyDisplay("Locked")]
        [ReadOnly(true)]
        public string? Locked { get; set; }
    }
}