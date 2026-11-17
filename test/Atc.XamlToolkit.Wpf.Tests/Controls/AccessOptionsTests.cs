namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Checks the code the source generator emits for <c>TargetType</c>, <c>AccessModifier</c> and
/// <c>GenerateClrAccessors</c> in a real project.
/// </summary>
public sealed class AccessOptionsTests
{
    private const System.Reflection.BindingFlags AllStatic =
        System.Reflection.BindingFlags.Static |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic;

    private const System.Reflection.BindingFlags AllInstance =
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic;

    [Fact]
    public void TargetType_IsTheParameterTypeOfTheAccessors()
    {
        var getMethod = typeof(AccessOptionsAttach).GetMethod(nameof(AccessOptionsAttach.GetCheckCornerRadius))!;
        var setMethod = typeof(AccessOptionsAttach).GetMethod(nameof(AccessOptionsAttach.SetCheckCornerRadius))!;

        Assert.Equal(typeof(System.Windows.UIElement), getMethod.GetParameters()[0].ParameterType);
        Assert.Equal(typeof(System.Windows.UIElement), setMethod.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void TargetType_AccessorsReadAndWriteTheProperty()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();

            AccessOptionsAttach.SetCheckCornerRadius(button, new System.Windows.CornerRadius(4));

            Assert.Equal(new System.Windows.CornerRadius(4), AccessOptionsAttach.GetCheckCornerRadius(button));
        });

    [Fact]
    public void AccessModifier_OnAnAttachedProperty_AppliesToTheFieldAndTheAccessors()
    {
        var field = typeof(AccessOptionsAttach).GetField("IsFilteredProperty", AllStatic)!;
        var getMethod = typeof(AccessOptionsAttach).GetMethod("GetIsFiltered", AllStatic)!;
        var setMethod = typeof(AccessOptionsAttach).GetMethod("SetIsFiltered", AllStatic)!;

        Assert.True(field.IsAssembly);
        Assert.True(getMethod.IsAssembly);
        Assert.True(setMethod.IsAssembly);
    }

    [Fact]
    public void GenerateClrAccessorsFalse_GeneratesOnlyThePrivateField()
    {
        var field = typeof(AccessOptionsAttach).GetField("StateProperty", AllStatic)!;

        Assert.True(field.IsPrivate);
        Assert.Null(typeof(AccessOptionsAttach).GetMethod("GetState", AllStatic));
        Assert.Null(typeof(AccessOptionsAttach).GetMethod("SetState", AllStatic));
    }

    [Fact]
    public void GenerateClrAccessorsFalse_ThePropertyIsRegisteredUnderItsName_AndWorksAsElementState()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();
            var field = typeof(AccessOptionsAttach).GetField("StateProperty", AllStatic)!;
            var property = (System.Windows.DependencyProperty)field.GetValue(obj: null)!;

            Assert.Equal("State", property.Name);
            Assert.Null(AccessOptionsAttach.ReadState(button));

            AccessOptionsAttach.WriteState(button, "filtered");

            Assert.Equal("filtered", AccessOptionsAttach.ReadState(button));
        });

    [Fact]
    public void AccessModifier_OnADependencyProperty_AppliesToTheFieldAndTheProperty()
        => StaThreadRunner.Run(() =>
        {
            var field = typeof(AccessOptionsControl).GetField("InternalZoomProperty", AllStatic)!;
            var property = typeof(AccessOptionsControl).GetProperty("InternalZoom", AllInstance)!;

            Assert.True(field.IsPrivate);
            Assert.True(property.GetMethod!.IsPrivate);
            Assert.True(property.SetMethod!.IsPrivate);
            Assert.Equal(2d, new AccessOptionsControl().ZoomIn());
        });
}