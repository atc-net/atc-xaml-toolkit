# AnimationBehavior

The `AnimationBehavior` provides simple, declarative animations for UI elements without writing code-behind. It supports fade, slide, and scale animations that can be triggered by property changes or automatically on load.

## Overview

Instead of writing animation code in code-behind:

```csharp
// ❌ Avoid this
private void AnimateElement()
{
    var animation = new DoubleAnimation
    {
        From = 0,
        To = 1,
        Duration = TimeSpan.FromMilliseconds(300)
    };
    element.BeginAnimation(OpacityProperty, animation);
}
```

Use `AnimationBehavior` in XAML:

```xml
<!-- ✅ Do this -->
<behaviors:AnimationBehavior 
    AnimationType="FadeIn"
    Duration="300"
    AutoStart="True" />
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `AnimationType` | `AnimationType` | The type of animation to play (FadeIn, FadeOut, SlideInFromLeft, etc.) |
| `Duration` | `int` | Duration of the animation in milliseconds (default: 300) |
| `AnimationTrigger` | `object` | When this property changes, the animation plays |
| `AutoStart` | `bool` | If `true`, animation plays automatically when the element loads |

## Animation Types

| AnimationType | Description |
|---------------|-------------|
| `FadeIn` | Fades element from transparent to opaque |
| `FadeOut` | Fades element from opaque to transparent |
| `SlideInFromLeft` | Slides element in from the left side |
| `SlideInFromRight` | Slides element in from the right side |
| `SlideInFromTop` | Slides element in from the top |
| `SlideInFromBottom` | Slides element in from the bottom |
| `ScaleIn` | Scales element from 0 to full size |
| `ScaleOut` | Scales element from full size to 0 |

## Platform-Specific Usage

### WPF

#### Setup

1. Install the required packages:

```powershell
dotnet add package Atc.XamlToolkit.Wpf
dotnet add package Microsoft.Xaml.Behaviors.Wpf
```

2. Add namespace declarations to your XAML:

```xml
<Window xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
        xmlns:behaviors="clr-namespace:Atc.XamlToolkit.Behaviors;assembly=Atc.XamlToolkit.Wpf">
```

#### Example 1: Auto-Start Fade In

```xml
<Border Background="LightBlue" Padding="20">
    <i:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="FadeIn"
            Duration="1000"
            AutoStart="True" />
    </i:Interaction.Behaviors>
    <TextBlock Text="I fade in automatically when loaded!" 
               FontSize="16" />
</Border>
```

#### Example 2: Trigger-Based Animation

```xml
<StackPanel>
    <Button Content="Trigger Animation" 
            Command="{Binding TriggerAnimationCommand}"
            Margin="0,0,0,10" />
    
    <Border Background="LightGreen" Padding="20">
        <i:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="SlideInFromLeft"
                Duration="600"
                AnimationTrigger="{Binding AnimationTrigger}" />
        </i:Interaction.Behaviors>
        <TextBlock Text="Watch me slide!" FontSize="16" />
    </Border>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? animationTrigger;

    [RelayCommand]
    private void TriggerAnimation()
    {
        // Change the trigger value to start animation
        AnimationTrigger = new object();
    }
}
```

#### Example 3: Multiple Fade Animations

```xml
<StackPanel Orientation="Horizontal">
    <Button Content="Fade In" 
            Command="{Binding FadeInCommand}"
            Margin="0,0,5,0" />
    <Button Content="Fade Out" 
            Command="{Binding FadeOutCommand}" />
</StackPanel>

<Border Background="LightCoral" Padding="20" Margin="0,10,0,0">
    <StackPanel>
        <TextBlock Text="Fading In" FontSize="16">
            <i:Interaction.Behaviors>
                <behaviors:AnimationBehavior 
                    AnimationType="FadeIn"
                    Duration="500"
                    AnimationTrigger="{Binding FadeInTrigger}" />
            </i:Interaction.Behaviors>
        </TextBlock>
        
        <TextBlock Text="Fading Out" FontSize="16" Margin="0,10,0,0">
            <i:Interaction.Behaviors>
                <behaviors:AnimationBehavior 
                    AnimationType="FadeOut"
                    Duration="500"
                    AnimationTrigger="{Binding FadeOutTrigger}" />
            </i:Interaction.Behaviors>
        </TextBlock>
    </StackPanel>
</Border>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? fadeInTrigger;

    [ObservableProperty]
    private object? fadeOutTrigger;

    [RelayCommand]
    private void FadeIn() => FadeInTrigger = new object();

    [RelayCommand]
    private void FadeOut() => FadeOutTrigger = new object();
}
```

#### Example 4: Scale Animations

```xml
<UniformGrid Columns="2">
    <Border Background="Lavender" Padding="20" Margin="0,0,5,0">
        <i:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="ScaleIn"
                Duration="400"
                AnimationTrigger="{Binding ScaleInTrigger}" />
        </i:Interaction.Behaviors>
        <TextBlock Text="Scaling In!" 
                   FontSize="16" 
                   HorizontalAlignment="Center" />
    </Border>
    
    <Border Background="PeachPuff" Padding="20" Margin="5,0,0,0">
        <i:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="ScaleOut"
                Duration="400"
                AnimationTrigger="{Binding ScaleOutTrigger}" />
        </i:Interaction.Behaviors>
        <TextBlock Text="Scaling Out!" 
                   FontSize="16" 
                   HorizontalAlignment="Center" />
    </Border>
</UniformGrid>
```

### WinUI

#### Setup

1. Install the required package:

```powershell
dotnet add package Atc.XamlToolkit.WinUI
# Microsoft.Xaml.Behaviors.WinUI.Managed is automatically included
```

2. Add namespace declarations to your XAML:

```xml
<Page xmlns:interactivity="using:Microsoft.Xaml.Interactivity"
      xmlns:behaviors="using:Atc.XamlToolkit.Behaviors">
```

#### Example 1: Auto-Start Animation

```xml
<Border Background="LightBlue" Padding="20">
    <interactivity:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="FadeIn"
            Duration="1000"
            AutoStart="True" />
    </interactivity:Interaction.Behaviors>
    <TextBlock Text="I fade in automatically!" 
               FontSize="16" />
</Border>
```

#### Example 2: Slide Animations

```xml
<StackPanel>
    <StackPanel Orientation="Horizontal" Spacing="10" Margin="0,0,0,10">
        <Button Content="Slide from Left" 
                Command="{x:Bind ViewModel.SlideLeftCommand}" />
        <Button Content="Slide from Right" 
                Command="{x:Bind ViewModel.SlideRightCommand}" />
        <Button Content="Slide from Top" 
                Command="{x:Bind ViewModel.SlideTopCommand}" />
        <Button Content="Slide from Bottom" 
                Command="{x:Bind ViewModel.SlideBottomCommand}" />
    </StackPanel>
    
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition />
            <ColumnDefinition />
        </Grid.ColumnDefinitions>
        
        <Border Grid.Column="0" Background="LightGreen" 
                Padding="20" Margin="0,0,5,0">
            <interactivity:Interaction.Behaviors>
                <behaviors:AnimationBehavior 
                    AnimationType="SlideInFromLeft"
                    Duration="600"
                    AnimationTrigger="{x:Bind ViewModel.SlideLeftTrigger, Mode=OneWay}" />
            </interactivity:Interaction.Behaviors>
            <TextBlock Text="From Left" 
                       HorizontalAlignment="Center" />
        </Border>
        
        <Border Grid.Column="1" Background="LightCoral" 
                Padding="20" Margin="5,0,0,0">
            <interactivity:Interaction.Behaviors>
                <behaviors:AnimationBehavior 
                    AnimationType="SlideInFromRight"
                    Duration="600"
                    AnimationTrigger="{x:Bind ViewModel.SlideRightTrigger, Mode=OneWay}" />
            </interactivity:Interaction.Behaviors>
            <TextBlock Text="From Right" 
                       HorizontalAlignment="Center" />
        </Border>
    </Grid>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? slideLeftTrigger;

    [ObservableProperty]
    private object? slideRightTrigger;

    [RelayCommand]
    private void SlideLeft() => SlideLeftTrigger = new object();

    [RelayCommand]
    private void SlideRight() => SlideRightTrigger = new object();
}
```

#### Example 3: List Item Animations

```xml
<ListView ItemsSource="{x:Bind ViewModel.Items}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="x:String">
            <Border Background="AliceBlue" Padding="10" Margin="0,2">
                <interactivity:Interaction.Behaviors>
                    <behaviors:AnimationBehavior 
                        AnimationType="FadeIn"
                        Duration="400"
                        AutoStart="True" />
                </interactivity:Interaction.Behaviors>
                <TextBlock Text="{x:Bind}" />
            </Border>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

### Avalonia

#### Setup

1. Install the required packages:

```powershell
dotnet add package Atc.XamlToolkit.Avalonia
# Avalonia.Xaml.Behaviors is automatically included
```

2. Add namespace declarations to your XAML:

```xml
<Window xmlns:ia="using:Avalonia.Xaml.Interactivity"
        xmlns:behaviors="using:Atc.XamlToolkit.Behaviors">
```

#### Example 1: Auto-Start Fade In

```xml
<Border Background="LightBlue" Padding="20">
    <ia:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="FadeIn"
            Duration="1000"
            AutoStart="True" />
    </ia:Interaction.Behaviors>
    <TextBlock Text="I fade in automatically when loaded!" 
               FontSize="16" />
</Border>
```

#### Example 2: Triggered Animations

```xml
<StackPanel>
    <StackPanel Orientation="Horizontal" Spacing="10" Margin="0,0,0,10">
        <Button Content="Fade In" Command="{Binding FadeInCommand}" />
        <Button Content="Fade Out" Command="{Binding FadeOutCommand}" />
    </StackPanel>
    
    <Border Background="LightCoral" Padding="20">
        <StackPanel>
            <TextBlock Text="Fading In" FontSize="16">
                <ia:Interaction.Behaviors>
                    <behaviors:AnimationBehavior 
                        AnimationType="FadeIn"
                        Duration="500"
                        AnimationTrigger="{Binding FadeInTrigger}" />
                </ia:Interaction.Behaviors>
            </TextBlock>
            
            <TextBlock Text="Fading Out" FontSize="16" Margin="0,10,0,0">
                <ia:Interaction.Behaviors>
                    <behaviors:AnimationBehavior 
                        AnimationType="FadeOut"
                        Duration="500"
                        AnimationTrigger="{Binding FadeOutTrigger}" />
                </ia:Interaction.Behaviors>
            </TextBlock>
        </StackPanel>
    </Border>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? fadeInTrigger;

    [ObservableProperty]
    private object? fadeOutTrigger;

    [RelayCommand]
    private void FadeIn() => FadeInTrigger = new object();

    [RelayCommand]
    private void FadeOut() => FadeOutTrigger = new object();
}
```

#### Example 3: Slide Animations

```xml
<StackPanel>
    <Button Content="Slide from Top" 
            Command="{Binding SlideTopCommand}"
            Margin="0,0,0,10" />
    
    <Border Background="LightYellow" Padding="20">
        <ia:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="SlideInFromTop"
                Duration="600"
                AnimationTrigger="{Binding SlideTopTrigger}" />
        </ia:Interaction.Behaviors>
        <TextBlock Text="Sliding from top!" 
                   FontSize="16" 
                   HorizontalAlignment="Center" />
    </Border>
</StackPanel>
```

#### Example 4: Scale Animations

```xml
<Grid ColumnDefinitions="*,*">
    <Border Grid.Column="0" Background="Lavender" 
            Padding="20" Margin="0,0,5,0">
        <ia:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="ScaleIn"
                Duration="400"
                AnimationTrigger="{Binding ScaleInTrigger}" />
        </ia:Interaction.Behaviors>
        <TextBlock Text="Scaling In!" 
                   FontSize="16" 
                   HorizontalAlignment="Center" />
    </Border>
    
    <Border Grid.Column="1" Background="PeachPuff" 
            Padding="20" Margin="5,0,0,0">
        <ia:Interaction.Behaviors>
            <behaviors:AnimationBehavior 
                AnimationType="ScaleOut"
                Duration="400"
                AnimationTrigger="{Binding ScaleOutTrigger}" />
        </ia:Interaction.Behaviors>
        <TextBlock Text="Scaling Out!" 
                   FontSize="16" 
                   HorizontalAlignment="Center" />
    </Border>
</Grid>
```

## Common Use Cases

### 1. Welcome Screen Animation

```xml
<StackPanel>
    <ia:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="FadeIn"
            Duration="1500"
            AutoStart="True" />
    </ia:Interaction.Behaviors>
    
    <TextBlock Text="Welcome!" 
               FontSize="32" 
               HorizontalAlignment="Center" />
</StackPanel>
```

### 2. Notification Banner

```xml
<Border Background="Yellow" Padding="10">
    <ia:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="SlideInFromTop"
            Duration="500"
            AnimationTrigger="{Binding ShowNotification}" />
    </ia:Interaction.Behaviors>
    <TextBlock Text="{Binding NotificationMessage}" />
</Border>
```

### 3. Loading Indicator

```xml
<Border Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}">
    <ia:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="FadeIn"
            Duration="300"
            AutoStart="True" />
    </ia:Interaction.Behaviors>
    <ProgressRing IsActive="True" />
</Border>
```

### 4. Dialog Appearance

```xml
<Border Background="White" Padding="20" BorderBrush="Gray" BorderThickness="2">
    <ia:Interaction.Behaviors>
        <behaviors:AnimationBehavior 
            AnimationType="ScaleIn"
            Duration="300"
            AutoStart="True" />
    </ia:Interaction.Behaviors>
    <StackPanel>
        <TextBlock Text="Confirm Action" FontSize="18" FontWeight="Bold" />
        <TextBlock Text="Are you sure?" Margin="0,10,0,0" />
    </StackPanel>
</Border>
```

## Tips and Best Practices

### ✅ Do's

- **Use AutoStart for initial animations** - Great for loading screens and welcome messages
- **Keep durations reasonable** - 200-600ms feels responsive, 1000ms+ can feel sluggish
- **Combine with visibility** - Animate when showing/hiding elements
- **Use triggers for user interactions** - Provide visual feedback for user actions

### ❌ Don'ts

- **Don't overuse animations** - Too many animations can be distracting
- **Don't animate every element** - Reserve for important UI changes
- **Don't use very long durations** - Users don't want to wait for animations
- **Don't forget accessibility** - Some users may prefer reduced motion

## Animation Duration Guidelines

| Use Case | Recommended Duration |
|----------|---------------------|
| Quick feedback | 100-200ms |
| Standard transitions | 300-500ms |
| Emphasis animations | 500-800ms |
| Special effects | 800-1500ms |

## Troubleshooting

### Animation Not Playing

**Problem:** Animation doesn't start when trigger changes

**Solution:** Ensure the trigger value actually changes. Use `new object()` to guarantee a new value:

```csharp
// ❌ Wrong - same value
AnimationTrigger = true;

// ✅ Correct - new object instance
AnimationTrigger = new object();
```

### Animation Plays Only Once

**Problem:** Animation only works the first time

**Solution:** Each time you want to replay the animation, you must change the AnimationTrigger value:

```csharp
[RelayCommand]
private void Replay()
{
    // Create a new object each time
    AnimationTrigger = new object();
}
```

### Slide Animation Not Visible

**Problem:** Slide animation doesn't appear to work

**Solution:** Ensure the element has proper layout constraints. The element needs space to slide into:

```xml
<!-- ❌ Wrong - stretched to fill -->
<Border HorizontalAlignment="Stretch" />

<!-- ✅ Correct - has defined size -->
<Border Width="300" HorizontalAlignment="Left" />
```

## Performance Considerations

- **Animations are GPU-accelerated** when possible for smooth performance
- **Multiple simultaneous animations** are supported
- **Memory usage** is minimal - behaviors are lightweight
- **Works well with ItemsControl** for list animations

## See Also

- [Behaviors Overview](Readme.md)
- [EventToCommandBehavior](EventToCommandBehavior.md)
- [FocusBehavior](FocusBehavior.md)
- [MVVM Framework](../Mvvm/Readme.md)
