namespace Atc.XamlToolkit.Avalonia.Tests.Mvvm;

public sealed class MainWindowViewModelBaseTests
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
    [InlineData(true, false, "WindowState")]
    [SuppressMessage("Design", "CA1030:Use events where appropriate", Justification = "OK.")]
    public void RaisePropertyChanged(
        bool expected,
        bool expectedAsEmpty,
        string? propertyName)
    {
        // Arrange
        var sut = new TestMainWindowViewModelBase();
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
    [InlineData(1.0, 0.5, global::Avalonia.Controls.WindowState.Normal)]
    [InlineData(1.0, 1.0, global::Avalonia.Controls.WindowState.Maximized)]
    [InlineData(1.5, 0.5, global::Avalonia.Controls.WindowState.Normal)]
    [InlineData(1.5, 1.0, global::Avalonia.Controls.WindowState.Maximized)]
    public void OnLoaded_MaximizesWindow_WhenItCoversTheWorkingAreaInPixels(
        double renderScaling,
        double fractionOfWorkingAreaWidth,
        global::Avalonia.Controls.WindowState expected)
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var window = new global::Avalonia.Controls.Window();
            window.Show();
            try
            {
                HeadlessTestApplication.Lifetime.MainWindow.Returns(window);

                // The working area is in physical pixels, the size of the window in device-independent pixels
                var workingArea = window.Screens.Primary!.WorkingArea;
                global::Avalonia.Headless.HeadlessWindowExtensions.SetRenderScaling(window, renderScaling);
                window.Width = workingArea.Width * fractionOfWorkingAreaWidth / renderScaling;
                window.Height = 100;
                HeadlessRunner.PumpDispatcher();

                var sut = new TestMainWindowViewModelBase();

                // Act
                sut.OnLoaded(window, EventArgs.Empty);

                // Assert
                Assert.Equal(expected, sut.WindowState);
            }
            finally
            {
                HeadlessTestApplication.Lifetime.MainWindow.Returns((global::Avalonia.Controls.Window?)null);
                window.Close();
            }
        });
}