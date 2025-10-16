# EventToCommandBehavior

The `EventToCommandBehavior` enables you to execute commands in response to any UI event without writing code-behind. This is essential for maintaining clean MVVM architecture and keeping your ViewModels testable.

## Overview

Instead of handling events in code-behind:

```csharp
// ❌ Avoid this
private void Button_Click(object sender, RoutedEventArgs e)
{
    viewModel.HandleClick();
}
```

Use `EventToCommandBehavior` in XAML:

```xml
<!-- ✅ Do this -->
<behaviors:EventToCommandBehavior 
    EventName="Click"
    Command="{Binding HandleClickCommand}" />
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `EventName` | `string` | Name of the event to listen to (e.g., "Click", "SelectionChanged") |
| `Command` | `ICommand` | Command to execute when the event is raised |
| `CommandParameter` | `object` | Optional parameter to pass to the command |
| `PassEventArgsToCommand` | `bool` | If `true`, passes the event arguments instead of `CommandParameter` |

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

#### Example 1: Simple Button Click

```xml
<Button Content="Save" Width="100">
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="Click"
            Command="{Binding SaveCommand}" />
    </i:Interaction.Behaviors>
</Button>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void Save()
    {
        // Save logic here
    }
}
```

#### Example 2: ListBox SelectionChanged with Event Args

```xml
<ListBox ItemsSource="{Binding Items}">
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="SelectionChanged"
            Command="{Binding SelectionChangedCommand}"
            PassEventArgsToCommand="True" />
    </i:Interaction.Behaviors>
</ListBox>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void SelectionChanged(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
        {
            var selectedItem = args.AddedItems[0];
            // Handle selection
        }
    }
}
```

#### Example 3: TextBox TextChanged with Custom Parameter

```xml
<TextBox Width="200">
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="TextChanged"
            Command="{Binding TextChangedCommand}"
            CommandParameter="UserInput" />
    </i:Interaction.Behaviors>
</TextBox>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void TextChanged(object parameter)
    {
        // parameter will be "UserInput"
        Debug.WriteLine($"Text changed from: {parameter}");
    }
}
```

#### Example 4: Multiple Behaviors on One Element

```xml
<Border Background="LightGray" Width="300" Height="100">
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="MouseEnter"
            Command="{Binding MouseEnterCommand}" />
        <behaviors:EventToCommandBehavior 
            EventName="MouseLeave"
            Command="{Binding MouseLeaveCommand}" />
    </i:Interaction.Behaviors>
    
    <TextBlock Text="Hover over me!" 
               HorizontalAlignment="Center" 
               VerticalAlignment="Center" />
</Border>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private Brush borderBackground = Brushes.LightGray;

    [RelayCommand]
    private void MouseEnter()
    {
        BorderBackground = Brushes.LightBlue;
    }

    [RelayCommand]
    private void MouseLeave()
    {
        BorderBackground = Brushes.LightGray;
    }
}
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

#### Example 1: Button Click with x:Bind

```xml
<Button Content="Load Data">
    <interactivity:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="Click"
            Command="{x:Bind ViewModel.LoadDataCommand}" />
    </interactivity:Interaction.Behaviors>
</Button>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommandAsync]
    private async Task LoadDataAsync()
    {
        // Load data asynchronously
        await Task.Delay(1000);
    }
}
```

#### Example 2: ListView SelectionChanged

```xml
<ListView ItemsSource="{x:Bind ViewModel.Items}" SelectionMode="Single">
    <interactivity:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="SelectionChanged"
            Command="{x:Bind ViewModel.ItemSelectedCommand}"
            PassEventArgsToCommand="True" />
    </interactivity:Interaction.Behaviors>
</ListView>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> items = new();

    [RelayCommand]
    private void ItemSelected(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
        {
            var item = args.AddedItems[0];
            // Handle selection
        }
    }
}
```

#### Example 3: PointerEntered and PointerExited

```xml
<Border Background="{x:Bind ViewModel.Background, Mode=OneWay}" 
        Width="300" Height="100">
    <interactivity:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="PointerEntered"
            Command="{x:Bind ViewModel.PointerEnteredCommand}" />
        <behaviors:EventToCommandBehavior 
            EventName="PointerExited"
            Command="{x:Bind ViewModel.PointerExitedCommand}" />
    </interactivity:Interaction.Behaviors>
    
    <TextBlock Text="Hover over me!" 
               HorizontalAlignment="Center" 
               VerticalAlignment="Center" />
</Border>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private SolidColorBrush background = new(Colors.LightGray);

    [RelayCommand]
    private void PointerEntered()
    {
        Background = new SolidColorBrush(Colors.LightBlue);
    }

    [RelayCommand]
    private void PointerExited()
    {
        Background = new SolidColorBrush(Colors.LightGray);
    }
}
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

#### Example 1: Button Click

```xml
<Button Content="Execute Action">
    <ia:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="Click"
            Command="{Binding ExecuteActionCommand}" />
    </ia:Interaction.Behaviors>
</Button>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void ExecuteAction()
    {
        // Action logic
    }
}
```

#### Example 2: ListBox SelectionChanged

```xml
<ListBox ItemsSource="{Binding Items}" SelectionMode="Single">
    <ia:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="SelectionChanged"
            Command="{Binding SelectionChangedCommand}"
            PassEventArgsToCommand="True" />
    </ia:Interaction.Behaviors>
</ListBox>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> items = new();

    [RelayCommand]
    private void SelectionChanged(SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
        {
            var item = args.AddedItems[0];
            // Handle selection
        }
    }
}
```

#### Example 3: TextBox with Custom Parameter

```xml
<TextBox Watermark="Enter text...">
    <ia:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="TextChanged"
            Command="{Binding TextChangedCommand}"
            CommandParameter="UserInput" />
    </ia:Interaction.Behaviors>
</TextBox>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void TextChanged(object parameter)
    {
        Debug.WriteLine($"Text changed from: {parameter}");
    }
}
```

#### Example 4: PointerEntered and PointerExited

```xml
<Border Background="{Binding Background}" Width="300" Height="100">
    <ia:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="PointerEntered"
            Command="{Binding PointerEnteredCommand}" />
        <behaviors:EventToCommandBehavior 
            EventName="PointerExited"
            Command="{Binding PointerExitedCommand}" />
    </ia:Interaction.Behaviors>
    
    <TextBlock Text="Hover over me!" 
               HorizontalAlignment="Center" 
               VerticalAlignment="Center" />
</Border>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private IBrush background = Brushes.LightGray;

    [RelayCommand]
    private void PointerEntered()
    {
        Background = Brushes.LightBlue;
    }

    [RelayCommand]
    private void PointerExited()
    {
        Background = Brushes.LightGray;
    }
}
```

## Common Use Cases

### 1. Form Validation on Lost Focus

```xml
<TextBox>
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="LostFocus"
            Command="{Binding ValidateFieldCommand}" />
    </i:Interaction.Behaviors>
</TextBox>
```

### 2. Drag and Drop

```xml
<ListBox AllowDrop="True">
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="Drop"
            Command="{Binding HandleDropCommand}"
            PassEventArgsToCommand="True" />
    </i:Interaction.Behaviors>
</ListBox>
```

### 3. Window Loaded Event

```xml
<Window>
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="Loaded"
            Command="{Binding InitializeCommand}" />
    </i:Interaction.Behaviors>
</Window>
```

### 4. Data Grid Cell Editing

```xml
<DataGrid>
    <i:Interaction.Behaviors>
        <behaviors:EventToCommandBehavior 
            EventName="CellEditEnding"
            Command="{Binding CellEditedCommand}"
            PassEventArgsToCommand="True" />
    </i:Interaction.Behaviors>
</DataGrid>
```

## Tips and Best Practices

### ✅ Do's

- **Use for any event** - Works with any routed event or standard .NET event
- **Combine with source generators** - Use `[RelayCommand]` for clean command generation
- **Pass event args when needed** - Set `PassEventArgsToCommand="True"` to access event details
- **Multiple behaviors** - Attach multiple behaviors to handle different events

### ❌ Don'ts

- **Don't use for complex logic** - Keep command methods focused and simple
- **Don't forget async commands** - Use `[RelayCommandAsync]` for async operations
- **Don't ignore CanExecute** - Implement CanExecute logic when appropriate

## Comparison with Code-Behind

| Aspect | Code-Behind | EventToCommandBehavior |
|--------|-------------|----------------------|
| Testability | ❌ Hard to test | ✅ Easy to test ViewModels |
| MVVM Compliance | ❌ Violates pattern | ✅ Pure MVVM |
| Reusability | ❌ Tied to View | ✅ Reusable commands |
| Maintainability | ❌ Scattered logic | ✅ Centralized in ViewModel |
| Design-time support | ✅ Good | ✅ Good with proper bindings |

## Troubleshooting

### Event Name Not Found

**Problem:** Runtime exception "Event 'X' not found"

**Solution:** Check the exact event name on the control. Event names are case-sensitive.

```xml
<!-- ❌ Wrong -->
<behaviors:EventToCommandBehavior EventName="click" />

<!-- ✅ Correct -->
<behaviors:EventToCommandBehavior EventName="Click" />
```

### Command Not Executing

**Problem:** Command doesn't execute when event fires

**Solution:** 
1. Verify command is not null in ViewModel
2. Check CanExecute returns true
3. Ensure binding path is correct

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save()
{
    // Save logic
}

private bool CanSave() => !string.IsNullOrEmpty(UserName);
```

### Event Args Type Mismatch

**Problem:** InvalidCastException when using PassEventArgsToCommand

**Solution:** Ensure command parameter type matches event args type

```csharp
// ❌ Wrong - parameter type doesn't match
[RelayCommand]
private void SelectionChanged(EventArgs args) { }

// ✅ Correct - parameter type matches
[RelayCommand]
private void SelectionChanged(SelectionChangedEventArgs args) { }
```

## See Also

- [Behaviors Overview](Readme.md)
- [MVVM Framework](../Mvvm/Readme.md)
- [Commands](../Mvvm/Readme.md#commands)
- [Source Generators](../SourceGenerators/ViewModel.md)
