namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that provides simple animations for UI elements in WPF applications.
/// Supports fade, slide, and scale animations triggered by property changes.
/// </summary>
public class AnimationBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
{
    /// <summary>
    /// Dependency property for the animation trigger.
    /// </summary>
    public static readonly DependencyProperty AnimationTriggerProperty = DependencyProperty.Register(
        nameof(AnimationTrigger),
        typeof(object),
        typeof(AnimationBehavior),
        new PropertyMetadata(defaultValue: null, OnAnimationTriggerChanged));

    /// <summary>
    /// Dependency property for the animation type.
    /// </summary>
    public static readonly DependencyProperty AnimationTypeProperty = DependencyProperty.Register(
        nameof(AnimationType),
        typeof(AnimationType),
        typeof(AnimationBehavior),
        new PropertyMetadata(AnimationType.FadeIn));

    /// <summary>
    /// Dependency property for the animation duration in milliseconds.
    /// </summary>
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        nameof(Duration),
        typeof(int),
        typeof(AnimationBehavior),
        new PropertyMetadata(300));

    /// <summary>
    /// Dependency property to auto-start animation on load.
    /// </summary>
    public static readonly DependencyProperty AutoStartProperty = DependencyProperty.Register(
        nameof(AutoStart),
        typeof(bool),
        typeof(AnimationBehavior),
        new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets or sets the trigger value. When changed, the animation will play.
    /// </summary>
    public object? AnimationTrigger
    {
        get => GetValue(AnimationTriggerProperty);
        set => SetValue(AnimationTriggerProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of animation to play.
    /// </summary>
    public AnimationType AnimationType
    {
        get => (AnimationType)GetValue(AnimationTypeProperty);
        set => SetValue(AnimationTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of the animation in milliseconds.
    /// </summary>
    public int Duration
    {
        get => (int)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation should start automatically when loaded.
    /// </summary>
    public bool AutoStart
    {
        get => (bool)GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, BooleanBoxes.Box(value));
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
        {
            AssociatedObject.Loaded += OnLoaded;
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.Loaded -= OnLoaded;
        }

        base.OnDetaching();
    }

    private static void OnAnimationTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimationBehavior behavior && e.NewValue is not null)
        {
            behavior.PlayAnimation();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (AutoStart)
        {
            PlayAnimation();
        }
    }

    private void PlayAnimation()
    {
        if (AssociatedObject is null)
        {
            return;
        }

        var duration = TimeSpan.FromMilliseconds(Duration);

        switch (AnimationType)
        {
            case AnimationType.FadeIn:
                AnimateFadeIn(duration);
                break;
            case AnimationType.FadeOut:
                AnimateFadeOut(duration);
                break;
            case AnimationType.SlideInFromLeft:
                AnimateSlideIn(duration, -100, 0);
                break;
            case AnimationType.SlideInFromRight:
                AnimateSlideIn(duration, 100, 0);
                break;
            case AnimationType.SlideInFromTop:
                AnimateSlideIn(duration, 0, -100);
                break;
            case AnimationType.SlideInFromBottom:
                AnimateSlideIn(duration, 0, 100);
                break;
            case AnimationType.ScaleIn:
                AnimateScale(duration, 0, 1);
                break;
            case AnimationType.ScaleOut:
                AnimateScale(duration, 1, 0);
                break;
        }
    }

    private void AnimateFadeIn(TimeSpan duration)
    {
        var animation = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = duration,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        };

        AssociatedObject.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    private void AnimateFadeOut(TimeSpan duration)
    {
        var animation = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = duration,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseIn },
        };

        AssociatedObject.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    private void AnimateSlideIn(TimeSpan duration, double fromX, double fromY)
    {
        if (AssociatedObject.RenderTransform is not System.Windows.Media.TranslateTransform)
        {
            AssociatedObject.RenderTransform = new System.Windows.Media.TranslateTransform();
        }

        var transform = (System.Windows.Media.TranslateTransform)AssociatedObject.RenderTransform;

        if (System.Math.Abs(fromX) > 0.01)
        {
            var animationX = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = fromX,
                To = 0,
                Duration = duration,
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
            };
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, animationX);
        }

        if (System.Math.Abs(fromY) > 0.01)
        {
            var animationY = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = fromY,
                To = 0,
                Duration = duration,
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
            };
            transform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, animationY);
        }
    }

    private void AnimateScale(TimeSpan duration, double from, double to)
    {
        if (AssociatedObject.RenderTransform is not System.Windows.Media.ScaleTransform)
        {
            AssociatedObject.RenderTransform = new System.Windows.Media.ScaleTransform();
            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        var transform = (System.Windows.Media.ScaleTransform)AssociatedObject.RenderTransform;

        var animationX = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        };

        var animationY = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut },
        };

        transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, animationX);
        transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, animationY);
    }
}