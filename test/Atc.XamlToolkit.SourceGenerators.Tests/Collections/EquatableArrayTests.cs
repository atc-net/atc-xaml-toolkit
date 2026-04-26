namespace Atc.XamlToolkit.SourceGenerators.Tests.Collections;

public class EquatableArrayTests
{
    [Fact]
    public void Equals_TwoArraysWithSameContent_ReturnsTrue()
    {
        var a = new EquatableArray<string>(["a", "b", "c"]);
        var b = new EquatableArray<string>(["a", "b", "c"]);

        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Equals_TwoArraysWithDifferentContent_ReturnsFalse()
    {
        var a = new EquatableArray<string>(["a", "b", "c"]);
        var b = new EquatableArray<string>(["a", "b", "d"]);

        a.Equals(b).Should().BeFalse();
        (a == b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_TwoArraysWithDifferentLength_ReturnsFalse()
    {
        var a = new EquatableArray<string>(["a", "b"]);
        var b = new EquatableArray<string>(["a", "b", "c"]);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_TwoArraysWithSameContent_ReturnsSameHash()
    {
        var a = new EquatableArray<string>(["a", "b", "c"]);
        var b = new EquatableArray<string>(["a", "b", "c"]);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_TwoArraysWithDifferentOrder_ReturnsDifferentHash()
    {
        // Order matters — codegen emits items in source order, so reordering
        // the model collection should be observable to the cache.
        var a = new EquatableArray<string>(["a", "b", "c"]);
        var b = new EquatableArray<string>(["c", "b", "a"]);

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void DefaultStruct_IsEmpty()
    {
        EquatableArray<string> empty = default;

        empty.IsEmpty.Should().BeTrue();
        empty.Count.Should().Be(0);
        empty.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void Equals_TwoDefaultStructs_AreEqual()
    {
        EquatableArray<string> a = default;
        EquatableArray<string> b = default;

        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equals_DefaultAndEmpty_AreEqual()
    {
        EquatableArray<string> a = default;
        var b = new EquatableArray<string>([]);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromArray_PreservesContent()
    {
        EquatableArray<string> a = new[] { "x", "y" };

        a.Count.Should().Be(2);
        a[0].Should().Be("x");
        a[1].Should().Be("y");
    }

    [Fact]
    public void Enumeration_IteratesItemsInOrder()
    {
        var a = new EquatableArray<int>([1, 2, 3]);

        a.Should().Equal(1, 2, 3);
    }
}