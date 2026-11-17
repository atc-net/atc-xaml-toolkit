namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for a property template against real WPF.
/// </summary>
public sealed class PropertyTemplateTests
{
    [Fact]
    public void EachControl_GetsItsOwnProperties_WithItselfAsOwner()
    {
        Assert.NotSame(TemplateDrivePicker.ValueProperty, TemplateNumberPicker.ValueProperty);
        Assert.NotSame(TemplateDrivePicker.ShowRefreshButtonProperty, TemplateNumberPicker.ShowRefreshButtonProperty);
        Assert.Equal(typeof(TemplateDrivePicker), TemplateDrivePicker.ValueProperty.OwnerType);
        Assert.Equal(typeof(TemplateNumberPicker), TemplateNumberPicker.ValueProperty.OwnerType);
    }

    [Fact]
    public void TheTypeArgument_IsThePropertyType()
    {
        Assert.Equal(typeof(TemplateDriveInfo), TemplateDrivePicker.ValueProperty.PropertyType);
        Assert.Equal(typeof(int), TemplateNumberPicker.ValueProperty.PropertyType);
        Assert.Equal(typeof(TemplateDriveInfo), typeof(TemplateDrivePicker).GetProperty(nameof(TemplateDrivePicker.Value))!.PropertyType);
        Assert.Equal(typeof(int), typeof(TemplateNumberPicker).GetProperty(nameof(TemplateNumberPicker.Value))!.PropertyType);
    }

    [Fact]
    public void ANullableTypeArgument_MakesThePropertyAndTheEventNullable()
    {
        var context = new System.Reflection.NullabilityInfoContext();

        var property = context.Create(typeof(TemplateDrivePicker).GetProperty(nameof(TemplateDrivePicker.Value))!);
        var valueChanged = context.Create(typeof(TemplateDrivePicker).GetEvent(nameof(TemplateDrivePicker.ValueChanged))!);

        Assert.Equal(System.Reflection.NullabilityState.Nullable, property.ReadState);
        Assert.Equal(System.Reflection.NullabilityState.Nullable, valueChanged.GenericTypeArguments[0].ReadState);
    }

    [Fact]
    public void TheOptionsOfTheTemplate_AreApplied()
        => StaThreadRunner.Run(() =>
        {
            var sut = new TemplateDrivePicker();
            var metadata = Assert.IsType<FrameworkPropertyMetadata>(
                TemplateDrivePicker.ValueProperty.GetMetadata(typeof(TemplateDrivePicker)));

            Assert.True(metadata.BindsTwoWayByDefault);
            Assert.Null(sut.Value);
            Assert.True(sut.ShowRefreshButton);
            Assert.Equal(string.Empty, sut.WatermarkText);
            Assert.Equal(0, new TemplateNumberPicker().Value);
        });

    [Fact]
    public void AnUnboundHandlerType_IsClosedWithTheTypeArgument()
    {
        Assert.Equal(
            typeof(RoutedPropertyChangedEventHandler<TemplateDriveInfo>),
            TemplateDrivePicker.ValueChangedEvent.HandlerType);
        Assert.Equal(
            typeof(RoutedPropertyChangedEventHandler<int>),
            TemplateNumberPicker.ValueChangedEvent.HandlerType);
        Assert.Equal(typeof(TemplateDrivePicker), TemplateDrivePicker.ValueChangedEvent.OwnerType);
        Assert.NotSame(TemplateDrivePicker.ValueChangedEvent, TemplateNumberPicker.ValueChangedEvent);
    }

    [Fact]
    public void HooksAndRaiseMethodsFromTheTemplate_WorkInTheControl()
        => StaThreadRunner.Run(() =>
        {
            var drivePicker = new TemplateDrivePicker();
            var numberPicker = new TemplateNumberPicker();
            var drive = new TemplateDriveInfo { Name = "C:" };
            RoutedPropertyChangedEventArgs<TemplateDriveInfo?>? driveArgs = null;
            RoutedPropertyChangedEventArgs<int>? numberArgs = null;
            var refreshed = 0;
            drivePicker.ValueChanged += (_, e) => driveArgs = e;
            numberPicker.ValueChanged += (_, e) => numberArgs = e;
            drivePicker.Refreshed += (_, _) => refreshed++;

            drivePicker.Value = drive;
            numberPicker.Value = 7;
            drivePicker.Refresh();

            Assert.NotNull(driveArgs);
            Assert.Null(driveArgs.OldValue);
            Assert.Same(drive, driveArgs.NewValue);
            Assert.NotNull(numberArgs);
            Assert.Equal(7, numberArgs.NewValue);
            Assert.Equal(1, refreshed);
        });

    [Fact]
    public void MembersOfTheControl_AreKeptNextToThoseOfTheTemplate()
        => Assert.Equal(typeof(TemplateDrivePicker), TemplateDrivePicker.DriveCountProperty.OwnerType);

    [Fact]
    public void Exclude_LeavesOutPropertiesAndEvents()
    {
        const System.Reflection.BindingFlags all =
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.DeclaredOnly;

        var memberNames = typeof(TemplateLabelPicker).GetMembers(all).Select(x => x.Name).ToList();

        Assert.Contains("ShowRefreshButtonProperty", memberNames, StringComparer.Ordinal);
        Assert.Contains("WatermarkTextProperty", memberNames, StringComparer.Ordinal);
        Assert.DoesNotContain("ValueProperty", memberNames, StringComparer.Ordinal);
        Assert.DoesNotContain("ValueChangedEvent", memberNames, StringComparer.Ordinal);
        Assert.DoesNotContain("RefreshedEvent", memberNames, StringComparer.Ordinal);
    }

    [Fact]
    public void TheAttributeThatNamesTheTemplate_IsNotKeptInTheCompiledClass()
    {
        // It is only an instruction to the generator. Kept, it would put a reference to the
        // (usually internal) template type into the metadata of every public control.
        var attributeNames = typeof(TemplateDrivePicker)
            .GetCustomAttributesData()
            .Select(x => x.AttributeType.Name)
            .ToList();

        Assert.DoesNotContain("UseDependencyPropertyTemplateAttribute", attributeNames, StringComparer.Ordinal);
    }

    [Fact]
    public void TheTemplateClass_GetsNoGeneratedMembers()
    {
        const System.Reflection.BindingFlags all =
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.DeclaredOnly;

        var memberNames = typeof(TemplatePickerMembers<int>).GetMembers(all).Select(x => x.Name).ToList();

        Assert.DoesNotContain("ValueProperty", memberNames, StringComparer.Ordinal);
        Assert.DoesNotContain("ValueChangedEvent", memberNames, StringComparer.Ordinal);
    }
}