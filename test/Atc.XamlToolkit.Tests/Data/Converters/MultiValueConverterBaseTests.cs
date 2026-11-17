namespace Atc.XamlToolkit.Tests.Data.Converters;

public sealed class MultiValueConverterBaseTests
{
    [Fact]
    public void Convert_ThrowsTheConvertersOwnException()
    {
        IMultiValueConverter sut = new ThrowingConverter();

        var exception = Assert.Throws<InvalidOperationException>(
            () => sut.Convert(["a", "b"], typeof(string), parameter: null, System.Globalization.CultureInfo.InvariantCulture));

        Assert.Equal("converter failed", exception.Message);
    }

    [Fact]
    public void Convert_AcceptsNullInput_WhenTheInputTypeIsNullable()
    {
        IMultiValueConverter sut = new JoinConverter();

        var actual = sut.Convert(["a", null, "c"], typeof(string), parameter: null, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal("a,<null>,c", actual);
    }

    [Fact]
    public void Convert_ThrowsUnexpectedTypeException_WhenAnInputHasTheWrongType()
    {
        IMultiValueConverter sut = new JoinConverter();

        Assert.Throws<UnexpectedTypeException>(
            () => sut.Convert(["a", 42], typeof(string), parameter: null, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ConvertBack_AcceptsNull_WhenTheOutputTypeIsNullable()
    {
        IMultiValueConverter sut = new JoinConverter();

        var actual = sut.ConvertBack(value: null, [], parameter: null, System.Globalization.CultureInfo.InvariantCulture);

        Assert.NotNull(actual);
        Assert.Empty(actual);
    }

    private sealed class JoinConverter : MultiValueConverterBase<string?, string?>
    {
        public override string Convert(
            string?[] values,
            object? parameter,
            System.Globalization.CultureInfo culture)
            => string.Join(',', values.Select(x => x ?? "<null>"));

        public override object?[] ConvertBack(
            string? value,
            object? parameter,
            System.Globalization.CultureInfo culture)
            => value?.Split(',') ?? [];
    }

    private sealed class ThrowingConverter : MultiValueConverterBase<string, string>
    {
        public override string Convert(
            string[] values,
            object? parameter,
            System.Globalization.CultureInfo culture)
            => throw new InvalidOperationException("converter failed");

        public override object?[] ConvertBack(
            string value,
            object? parameter,
            System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }
}