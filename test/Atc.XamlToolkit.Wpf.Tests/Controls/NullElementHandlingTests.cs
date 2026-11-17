namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the accessors the source generator emits for <c>NullElementHandling</c> against real WPF.
/// </summary>
public sealed class NullElementHandlingTests
{
    [Fact]
    public void Default_SetIgnoresANullElement_AndGetThrowsNullReference()
    {
        NullElementAttach.SetLenient(null!, 1);

        Assert.Throws<NullReferenceException>(() => NullElementAttach.GetLenient(null!));
    }

    [Fact]
    public void ThrowArgumentNullException_BothAccessorsNameTheParameter()
    {
        var fromGet = Assert.Throws<ArgumentNullException>(() => NullElementAttach.GetStrict(null!));
        var fromSet = Assert.Throws<ArgumentNullException>(() => NullElementAttach.SetStrict(null!, 1));
        var fromBoolSet = Assert.Throws<ArgumentNullException>(() => NullElementAttach.SetStrictFlag(null!, value: true));

        Assert.Equal("element", fromGet.ParamName);
        Assert.Equal("element", fromSet.ParamName);
        Assert.Equal("element", fromBoolSet.ParamName);
    }

    [Fact]
    public void None_BothAccessorsThrowNullReference()
    {
        Assert.Throws<NullReferenceException>(() => NullElementAttach.GetPlain(null!));
        Assert.Throws<NullReferenceException>(() => NullElementAttach.SetPlain(null!, 1));
    }

    [Fact]
    public void EveryMode_ReadsAndWritesTheProperty()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();

            NullElementAttach.SetLenient(button, 1);
            NullElementAttach.SetStrict(button, 2);
            NullElementAttach.SetStrictFlag(button, value: true);
            NullElementAttach.SetPlain(button, 3);

            Assert.Equal(1d, NullElementAttach.GetLenient(button));
            Assert.Equal(2d, NullElementAttach.GetStrict(button));
            Assert.True(NullElementAttach.GetStrictFlag(button));
            Assert.Equal(3d, NullElementAttach.GetPlain(button));
        });
}