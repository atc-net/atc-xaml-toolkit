namespace Atc.XamlToolkit.Behaviors;

/// <summary>
/// A behavior that provides simple animations for UI elements in Avalonia applications.
/// Supports fade, slide, and scale animations triggered by property changes.
/// </summary>
public class AnimationBehavior : Avalonia.Xaml.Interactivity.Behavior<Control>
{
    /// <summary>
    /// Styled property for the animation trigger.
    /// </summary>
    public static readonly StyledProperty<object?> AnimationTriggerProperty =
        AvaloniaProperty.Register<AnimationBehavior, object?>(nameof(AnimationTrigger));

    /// <summary>
    /// Styled property for the animation type.
    /// </summary>
    public static readonly StyledProperty<AnimationType> AnimationTypeProperty =
        AvaloniaProperty.Register<AnimationBehavior, AnimationType>(nameof(AnimationType));

    /// <summary>
    /// Styled property for the animation duration in milliseconds.
    /// </summary>
    public static readonly StyledProperty<int> DurationProperty =
        AvaloniaProperty.Register<AnimationBehavior, int>(nameof(Duration), 300);

    /// <summary>
    /// Styled property to auto-start animation on load.
    /// </summary>
    public static readonly StyledProperty<bool> AutoStartProperty =
        AvaloniaProperty.Register<AnimationBehavior, bool>(nameof(AutoStart));

    private object? lastTrigger;
    private IDisposable? subscription;

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
        get => GetValue(AnimationTypeProperty);
        set => SetValue(AnimationTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the duration of the animation in milliseconds.
    /// </summary>
    public int Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the animation should start automatically when loaded.
    /// </summary>
    public bool AutoStart
    {
        get => GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        base.OnAttached();

        AssociatedObject.AttachedToVisualTree += OnAttachedToVisualTree;

        // Subscribe to property changes on the behavior itself
        subscription = AnimationTriggerProperty.Changed.Subscribe(new AnimationTriggerObserver(this));
    }

    private sealed class AnimationTriggerObserver : IObserver<AvaloniaPropertyChangedEventArgs<object?>>
    {
        private readonly AnimationBehavior behavior;

        public AnimationTriggerObserver(AnimationBehavior behavior)
            => this.behavior = behavior;

        public void OnNext(AvaloniaPropertyChangedEventArgs<object?> e)
        {
            if (ReferenceEquals(e.Sender, behavior))
            {
                behavior.OnAnimationTriggerChanged(e.NewValue.GetValueOrDefault());
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.AttachedToVisualTree -= OnAttachedToVisualTree;
        }

        subscription?.Dispose();
        subscription = null;

        base.OnDetaching();
    }

    private void OnAnimationTriggerChanged(object? newTrigger)
    {
        if (newTrigger is not null && !Equals(newTrigger, lastTrigger))
        {
            lastTrigger = newTrigger;
            PlayAnimation();
        }
    }

    private void OnAttachedToVisualTree(object? sender, EventArgs e)
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
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        var animation = new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Avalonia.Styling.Setter(Visual.OpacityProperty, 0.0),
                    },
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Avalonia.Styling.Setter(Visual.OpacityProperty, 1.0),
                    },
                },
            },
        };

        _ = animation.RunAsync(AssociatedObject);
    }

    private void AnimateFadeOut(TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        var animation = new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Avalonia.Styling.Setter(Visual.OpacityProperty, AssociatedObject.Opacity),
                    },
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Avalonia.Styling.Setter(Visual.OpacityProperty, 0.0),
                    },
                },
            },
        };

        _ = animation.RunAsync(AssociatedObject);
    }

    private void AnimateSlideIn(TimeSpan duration, double fromX, double fromY)
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        if (AssociatedObject.RenderTransform is not TranslateTransform)
        {
            AssociatedObject.RenderTransform = new TranslateTransform();
        }

        var transform = (TranslateTransform)AssociatedObject.RenderTransform;

        // Set initial values
        transform.X = fromX;
        transform.Y = fromY;

        var animation = new Animation
        {
            Duration = duration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
        };

        if (System.Math.Abs(fromX) > 0.01)
        {
            AddTranslateKeyFrames(animation, TranslateTransform.XProperty, fromX, 0.0);
        }

        if (System.Math.Abs(fromY) > 0.01)
        {
            AddTranslateKeyFrames(animation, TranslateTransform.YProperty, fromY, 0.0);
        }

        // Run animation on the control, targeting the RenderTransform properties
        _ = animation.RunAsync(AssociatedObject);
    }

    private static void AddTranslateKeyFrames(
        Animation animation,
        AvaloniaProperty property,
        double from,
        double to)
    {
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0),
            Setters =
            {
                new Avalonia.Styling.Setter
                {
                    Property = property,
                    Value = from,
                },
            },
        });
        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Avalonia.Styling.Setter
                {
                    Property = property,
                    Value = to,
                },
            },
        });
    }

    private void AnimateScale(TimeSpan duration, double from, double to)
    {
        ArgumentNullException.ThrowIfNull(AssociatedObject);

        if (AssociatedObject.RenderTransform is not ScaleTransform)
        {
            AssociatedObject.RenderTransform = new ScaleTransform();
            AssociatedObject.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        }

        var transform = (ScaleTransform)AssociatedObject.RenderTransform;

        // Set initial values
        transform.ScaleX = from;
        transform.ScaleY = from;

        var animation = CreateScaleAnimation(duration, from, to);

        // Run animation on the control, targeting the RenderTransform properties
        _ = animation.RunAsync(AssociatedObject);
    }

    private static Animation CreateScaleAnimation(TimeSpan duration, double from, double to)
    {
        return new Animation
        {
            Duration = duration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = ScaleTransform.ScaleXProperty,
                            Value = from,
                        },
                        new Avalonia.Styling.Setter
                        {
                            Property = ScaleTransform.ScaleYProperty,
                            Value = from,
                        },
                    },
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = ScaleTransform.ScaleXProperty,
                            Value = to,
                        },
                        new Avalonia.Styling.Setter
                        {
                            Property = ScaleTransform.ScaleYProperty,
                            Value = to,
                        },
                    },
                },
            },
        };
    }
}