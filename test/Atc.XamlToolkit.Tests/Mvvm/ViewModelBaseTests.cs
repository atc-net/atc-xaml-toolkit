namespace Atc.XamlToolkit.Tests.Mvvm;

public sealed class ViewModelBaseTests
{
    [Theory]
    [InlineData(true, true, null)]
    [InlineData(true, true, "")]
    [InlineData(true, false, "IsBoolProperty")]
    [InlineData(true, false, "IsBoolPropertyWithExpression")]
    [InlineData(true, false, "IsEnabled")]
    [InlineData(true, false, "IsVisible")]
    [InlineData(true, false, "IsBusy")]
    [InlineData(true, false, "IsDirty")]
    [InlineData(true, false, "IsSelected")]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    public void RaisePropertyChanged(
        bool expected,
        bool expectedAsEmpty,
        string? propertyName)
    {
        // Arrange
        var sut = new TestViewModel();
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

    [Theory]
    [InlineData(nameof(ViewModelBase.IsEnabled))]
    [InlineData(nameof(ViewModelBase.IsVisible))]
    [InlineData(nameof(ViewModelBase.IsBusy))]
    [InlineData(nameof(ViewModelBase.IsDirty))]
    [InlineData(nameof(ViewModelBase.IsSelected))]
    [InlineData(nameof(ViewModelBase.HasErrors))]
    public void ShouldSkipValidationOnPropertyChanged_SkipsUIStateAndValidationProperties(
        string propertyName)
    {
        // ViewModelBase overrides ShouldSkipValidationOnPropertyChanged to skip
        // its own UI-state members (and HasErrors via the ObservableValidator
        // base impl). Pin the membership so any future addition / removal is
        // intentional and visible in a diff.
        var sut = new SkipListProbeViewModel();

        sut.ShouldSkip(propertyName).Should().BeTrue();
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Address")]
    [InlineData("Anything")]
    public void ShouldSkipValidationOnPropertyChanged_DoesNotSkipDomainProperties(
        string propertyName)
    {
        var sut = new SkipListProbeViewModel();

        sut.ShouldSkip(propertyName).Should().BeFalse();
    }

    private sealed class SkipListProbeViewModel : ViewModelBase
    {
        public bool ShouldSkip(string propertyName)
            => ShouldSkipValidationOnPropertyChanged(propertyName);
    }
}