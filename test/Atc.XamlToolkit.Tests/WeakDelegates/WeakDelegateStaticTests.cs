namespace Atc.XamlToolkit.Tests.WeakDelegates;

public sealed class WeakDelegateStaticTests
{
    [Fact]
    public void WeakActionOfT_IsStatic_IsTrue_ForStaticMethod()
    {
        var sut = new WeakAction<int>(StaticAction);

        Assert.True(sut.IsStatic);
    }

    [Fact]
    public void WeakFuncOfT_IsStatic_IsTrue_ForStaticMethod()
    {
        var sut = new WeakFunc<int, bool>(StaticFunc);

        Assert.True(sut.IsStatic);
    }

    [Fact]
    public void WeakActionOfT_MarkForDeletion_ThroughBaseReference_MakesItNotAlive()
    {
        // The Messenger holds its registrations as the non-generic base type.
        WeakAction sut = new WeakAction<int>(StaticAction);

        sut.MarkForDeletion();

        Assert.False(sut.IsAlive);
    }

    private static void StaticAction(int value)
    {
        // Intentionally empty - only the delegate kind matters.
    }

    private static bool StaticFunc(int value)
        => value > 0;
}