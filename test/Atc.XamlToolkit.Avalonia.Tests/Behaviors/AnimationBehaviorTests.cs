namespace Atc.XamlToolkit.Avalonia.Tests.Behaviors;

public sealed class AnimationBehaviorTests
{
    [Fact]
    [SuppressMessage("Critical Code Smell", "S1215:\"GC.Collect\" should not be called", Justification = "The test verifies that the behavior can be garbage collected.")]
    public void AttachedBehavior_IsCollectable_WhenNoLongerReferenced()
    {
        // Arrange
        var behaviorReference = CreateAttachedBehavior();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert - nothing static (such as a class-level property observable) may keep the behavior alive
        Assert.False(behaviorReference.IsAlive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AutoStart_PlaysAnimation_WhenAttachedToAnElementThatIsAlreadyInTheVisualTree(
        bool autoStart)
        => HeadlessRunner.Run(() =>
        {
            // Arrange - a scale animation gives the element a ScaleTransform when it plays
            var border = new global::Avalonia.Controls.Border();
            var window = HeadlessRunner.ShowInWindow(border);
            try
            {
                var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior
                {
                    AnimationType = Atc.XamlToolkit.Behaviors.AnimationType.ScaleIn,
                    AutoStart = autoStart,
                };

                // Act
                behavior.Attach(border);

                // Assert
                Assert.Equal(autoStart, border.RenderTransform is global::Avalonia.Media.ScaleTransform);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void AutoStart_PlaysAnimation_WhenTheElementIsAttachedToTheVisualTree()
        => HeadlessRunner.Run(() =>
        {
            // Arrange
            var border = new global::Avalonia.Controls.Border();
            var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior
            {
                AnimationType = Atc.XamlToolkit.Behaviors.AnimationType.ScaleIn,
                AutoStart = true,
            };

            behavior.Attach(border);
            Assert.Null(border.RenderTransform);

            // Act
            var window = HeadlessRunner.ShowInWindow(border);
            try
            {
                // Assert
                Assert.IsType<global::Avalonia.Media.ScaleTransform>(border.RenderTransform);
            }
            finally
            {
                window.Close();
            }
        });

    [Fact]
    public void AnimationTrigger_PlaysAgain_WhenSetToTheSameValueAfterNull()
        => HeadlessRunner.Run(() =>
        {
            // Arrange - a scale animation gives the element a ScaleTransform when it plays
            var trigger = new object();
            var border = new global::Avalonia.Controls.Border();
            var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior
            {
                AnimationType = Atc.XamlToolkit.Behaviors.AnimationType.ScaleIn,
            };

            behavior.Attach(border);
            behavior.AnimationTrigger = trigger;
            Assert.IsType<global::Avalonia.Media.ScaleTransform>(border.RenderTransform);
            border.RenderTransform = null;

            // Act
            behavior.AnimationTrigger = null;
            behavior.AnimationTrigger = trigger;

            // Assert
            Assert.IsType<global::Avalonia.Media.ScaleTransform>(border.RenderTransform);
        });

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference CreateAttachedBehavior()
    {
        var behavior = new Atc.XamlToolkit.Behaviors.AnimationBehavior();
        behavior.Attach(new global::Avalonia.Controls.Border());
        return new WeakReference(behavior);
    }
}