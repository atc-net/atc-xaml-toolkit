namespace Atc.XamlToolkit.Tests.Mvvm;

public sealed class ObservableObjectTests
{
    [Theory]
    [InlineData(true, true, null)]
    [InlineData(true, true, "")]
    [InlineData(true, false, "IsBoolProperty")]
    [InlineData(true, false, "IsBoolPropertyWithExpression")]
    [InlineData(true, false, "IsBoolPropertyWithSet")]
    [InlineData(true, false, "IsBoolPropertyWithSetAndExpression")]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    public void RaisePropertyChanged(
        bool expected,
        bool expectedAsEmpty,
        string? propertyName)
    {
        // Arrange
        var sut = new TestObservableObject();
        var actual = false;
        sut.PropertyChanged += (_, e) =>
        {
            actual = TestHelper.HandlePropertyChangedEventArgs(e, expectedAsEmpty, propertyName!);
        };

        // Act
        sut.RaisePropertyChanged(propertyName);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RaisePropertyChanged_ReusesPropertyChangedEventArgs_AcrossRaisesForSamePropertyName()
    {
        // The PropertyChangedEventArgsCache is what makes property-change raises
        // allocation-free after the first hit. Verify the cache by capturing the
        // EventArgs instance from two consecutive raises and checking reference equality.
        var sut = new TestObservableObject();
        PropertyChangedEventArgs? firstArgs = null;
        PropertyChangedEventArgs? secondArgs = null;
        var raises = 0;

        sut.PropertyChanged += (_, e) =>
        {
            raises++;
            if (raises == 1)
            {
                firstArgs = e;
            }
            else
            {
                secondArgs = e;
            }
        };

        sut.RaisePropertyChanged("IsBoolProperty");
        sut.RaisePropertyChanged("IsBoolProperty");

        firstArgs.Should().NotBeNull();
        secondArgs.Should().BeSameAs(firstArgs, "the cache must return the same PropertyChangedEventArgs instance for repeat raises of the same property");
    }
}