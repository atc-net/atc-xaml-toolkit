namespace Atc.XamlToolkit.Tests.WeakDelegates;

/// <summary>
/// A weakly held instance delegate must throw the same exception as calling the
/// delegate directly - not a <see cref="System.Reflection.TargetInvocationException"/> wrapper.
/// </summary>
public sealed class WeakDelegateExceptionTests
{
    [Fact]
    public void WeakAction_Execute_ThrowsTheOriginalException()
    {
        var target = new Thrower("boom");
        var sut = new WeakAction(target.Run);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.Execute());

        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public void WeakActionOfT_Execute_ThrowsTheOriginalException()
    {
        var target = new Thrower("boom");
        var sut = new WeakAction<int>(target.Run);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.Execute(42));

        Assert.Equal("boom 42", exception.Message);
    }

    [Fact]
    public void WeakFunc_Execute_ThrowsTheOriginalException()
    {
        var target = new Thrower("boom");
        var sut = new WeakFunc<bool>(target.Check);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.Execute());

        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public void WeakFuncOfT_Execute_ThrowsTheOriginalException()
    {
        var target = new Thrower("boom");
        var sut = new WeakFunc<int, bool>(target.Check);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.Execute(42));

        Assert.Equal("boom 42", exception.Message);
    }

    private sealed class Thrower(string message)
    {
        public void Run()
            => throw new InvalidOperationException(message);

        public void Run(int value)
            => throw new InvalidOperationException($"{message} {value}");

        public bool Check()
            => throw new InvalidOperationException(message);

        public bool Check(int value)
            => throw new InvalidOperationException($"{message} {value}");
    }
}