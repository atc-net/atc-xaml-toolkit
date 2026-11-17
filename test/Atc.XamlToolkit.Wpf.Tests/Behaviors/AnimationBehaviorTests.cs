namespace Atc.XamlToolkit.Wpf.Tests.Behaviors;

public sealed class AnimationBehaviorTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AutoStart_PlaysAnimation_WhenAttachedToAnElementThatIsAlreadyLoaded(
        bool autoStart)
        => StaThreadRunner.Run(() =>
        {
            // Arrange - a scale animation gives the element a ScaleTransform when it plays
            var border = new System.Windows.Controls.Border();
            var window = StaThreadRunner.ShowInWindow(border);
            try
            {
                Assert.True(border.IsLoaded);
                var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior
                {
                    AnimationType = Atc.XamlToolkit.Behaviors.AnimationType.ScaleIn,
                    AutoStart = autoStart,
                };

                // Act
                behavior.Attach(border);

                // Assert
                Assert.Equal(autoStart, border.RenderTransform is System.Windows.Media.ScaleTransform);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void AutoStart_PlaysAnimation_WhenTheElementIsLoaded()
        => StaThreadRunner.Run(() =>
        {
            // Arrange
            var border = new System.Windows.Controls.Border();
            var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior
            {
                AnimationType = Atc.XamlToolkit.Behaviors.AnimationType.ScaleIn,
                AutoStart = true,
            };

            behavior.Attach(border);
            Assert.IsNotType<System.Windows.Media.ScaleTransform>(border.RenderTransform);

            // Act
            var window = StaThreadRunner.ShowInWindow(border);
            try
            {
                // Assert
                Assert.IsType<System.Windows.Media.ScaleTransform>(border.RenderTransform);
            }
            finally
            {
                window.Close();
            }
        });
}