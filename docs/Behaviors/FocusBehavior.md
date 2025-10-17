# FocusBehavior

The `FocusBehavior` manages focus for UI elements declaratively through XAML properties, eliminating the need for focus management code in code-behind. It provides two-way binding for focus state, automatic text selection, and trigger-based focus control.

## Overview

Instead of managing focus in code-behind:

```csharp
// ❌ Avoid this
private void SetFocusToTextBox()
{
    textBox.Focus();
    Keyboard.Focus(textBox);
    textBox.SelectAll();
}
```

Use `FocusBehavior` in XAML:

```xml
<!-- ✅ Do this -->
<behaviors:FocusBehavior 
    IsFocused="{Binding IsFieldFocused, Mode=TwoWay}"
    SelectAllOnFocus="True" />
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `HasInitialFocus` | `bool` | If `true`, element receives focus when the behavior is attached/loaded |
| `IsFocused` | `bool` | Two-way bindable property to track and control focus state |
| `SelectAllOnFocus` | `bool` | If `true`, selects all text when element receives focus (TextBox only) |
| `FocusTrigger` | `object` | When this property changes, focus is set to the element |

## Platform-Specific Usage

### WPF

#### WPF Setup

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

#### WPF Example 1: Initial Focus

```xml
<Window>
    <StackPanel Margin="20">
        <TextBlock Text="The TextBox below has initial focus:" 
                   Margin="0,0,0,5" />
        
        <TextBox Width="300" HorizontalAlignment="Left">
            <i:Interaction.Behaviors>
                <behaviors:FocusBehavior HasInitialFocus="True" />
            </i:Interaction.Behaviors>
        </TextBox>
    </StackPanel>
</Window>
```

#### WPF Example 2: Two-Way Focus Binding

```xml
<StackPanel Margin="20">
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="Click to focus">
        <i:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                IsFocused="{Binding IsTextBoxFocused, Mode=TwoWay}" />
        </i:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock Margin="0,10,0,0">
        <Run Text="Is Focused:" />
        <Run Text="{Binding IsTextBoxFocused}" FontWeight="Bold" />
    </TextBlock>
    
    <Button Content="Set Focus from ViewModel" 
            Command="{Binding SetFocusCommand}"
            Margin="0,10,0,0"
            Width="200"
            HorizontalAlignment="Left" />
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isTextBoxFocused;

    [RelayCommand]
    private void SetFocus()
    {
        // Setting this to true will focus the TextBox
        IsTextBoxFocused = true;
    }
}
```

#### WPF Example 3: Select All on Focus

```xml
<StackPanel Margin="20">
    <TextBlock Text="Tab into the TextBox - all text will be selected:" 
               Margin="0,0,0,5" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="This text will be selected on focus">
        <i:Interaction.Behaviors>
            <behaviors:FocusBehavior SelectAllOnFocus="True" />
        </i:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

#### WPF Example 4: Focus Trigger

```xml
<StackPanel Margin="20">
    <Button Content="Trigger Focus" 
            Command="{Binding TriggerFocusCommand}"
            Width="150"
            HorizontalAlignment="Left"
            Margin="0,0,0,10" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="I will receive focus when triggered">
        <i:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                FocusTrigger="{Binding FocusTrigger}" />
        </i:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? focusTrigger;

    [RelayCommand]
    private void TriggerFocus()
    {
        // Changing the trigger value sets focus
        FocusTrigger = new object();
    }
}
```

#### WPF Example 5: Form with Tab Order

```xml
<StackPanel Margin="20">
    <TextBlock Text="Login Form with Focus Management" 
               FontSize="18" 
               FontWeight="Bold"
               Margin="0,0,0,10" />
    
    <TextBlock Text="Username:" Margin="0,0,0,5" />
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
             Margin="0,0,0,10">
        <i:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                HasInitialFocus="True"
                SelectAllOnFocus="True" />
        </i:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock Text="Password:" Margin="0,0,0,5" />
    <PasswordBox Width="300" 
                 HorizontalAlignment="Left"
                 Margin="0,0,0,10">
        <i:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                IsFocused="{Binding IsPasswordFocused, Mode=TwoWay}" />
        </i:Interaction.Behaviors>
    </PasswordBox>
    
    <Button Content="Login" 
            Command="{Binding LoginCommand}"
            Width="100"
            HorizontalAlignment="Left" />
</StackPanel>
```

### WinUI

#### WinUI Setup

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

#### WinUI Example 1: Initial Focus

```xml
<StackPanel Margin="20">
    <TextBlock Text="The TextBox below has initial focus:" 
               Margin="0,0,0,5" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="{x:Bind ViewModel.InitialFocusText, Mode=TwoWay}">
        <interactivity:Interaction.Behaviors>
            <behaviors:FocusBehavior HasInitialFocus="True" />
        </interactivity:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

#### WinUI Example 2: Two-Way Focus Binding

```xml
<StackPanel Margin="20">
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             PlaceholderText="Click to focus">
        <interactivity:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                IsFocused="{x:Bind ViewModel.IsTextBoxFocused, Mode=TwoWay}" />
        </interactivity:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock Margin="0,10,0,0">
        <Run Text="Is Focused:" />
        <Run Text="{x:Bind ViewModel.IsTextBoxFocused, Mode=OneWay}" 
             FontWeight="Bold" />
    </TextBlock>
    
    <Button Content="Set Focus Programmatically" 
            Command="{x:Bind ViewModel.SetFocusCommand}"
            Margin="0,10,0,0" />
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isTextBoxFocused;

    [RelayCommand]
    private void SetFocus()
    {
        IsTextBoxFocused = true;
    }
}
```

#### WinUI Example 3: Select All on Focus

```xml
<StackPanel Margin="20">
    <TextBlock Text="Click or tab into the TextBox - all text will be selected:" 
               TextWrapping="Wrap"
               Margin="0,0,0,10" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="This text will be selected on focus">
        <interactivity:Interaction.Behaviors>
            <behaviors:FocusBehavior SelectAllOnFocus="True" />
        </interactivity:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

#### WinUI Example 4: Focus Trigger

```xml
<StackPanel Margin="20">
    <Button Content="Trigger Focus" 
            Command="{x:Bind ViewModel.TriggerFocusCommand}"
            Margin="0,0,0,10" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="I will receive focus when triggered">
        <interactivity:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                FocusTrigger="{x:Bind ViewModel.FocusTrigger, Mode=OneWay}" />
        </interactivity:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? focusTrigger;

    [RelayCommand]
    private void TriggerFocus()
    {
        FocusTrigger = new object();
    }
}
```

#### WinUI Example 5: Combined Features

```xml
<StackPanel Margin="20">
    <TextBlock Text="Combined: SelectAllOnFocus + IsFocused binding" 
               FontWeight="Bold"
               Margin="0,0,0,10" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="Combined features example"
             Margin="0,0,0,10">
        <interactivity:Interaction.Behaviors>
            <behaviors:FocusBehavior
                IsFocused="{x:Bind ViewModel.IsCombinedFocused, Mode=TwoWay}"
                SelectAllOnFocus="True" />
        </interactivity:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock>
        <Run Text="Focus State:" />
        <Run Text="{x:Bind ViewModel.IsCombinedFocused, Mode=OneWay}"
             FontWeight="Bold" />
    </TextBlock>
</StackPanel>
```

### Avalonia

#### Avalonia Setup

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

#### Avalonia Example 1: Initial Focus

```xml
<StackPanel Margin="20">
    <TextBlock Text="The TextBox below has initial focus:" 
               Margin="0,0,0,5" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="{Binding InitialFocusText}">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior HasInitialFocus="True" />
        </ia:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

#### Avalonia Example 2: Two-Way Focus Binding

```xml
<StackPanel Margin="20">
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Watermark="Click to focus">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                IsFocused="{Binding IsTextBoxFocused, Mode=TwoWay}" />
        </ia:Interaction.Behaviors>
    </TextBox>
    
    <TextBlock Margin="0,10,0,0">
        <Run Text="Is Focused:" />
        <Run Text="{Binding IsTextBoxFocused}" FontWeight="Bold" />
    </TextBlock>
    
    <Button Content="Set Focus from ViewModel" 
            Command="{Binding SetFocusCommand}"
            Margin="0,10,0,0"
            Width="200"
            HorizontalAlignment="Left" />
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isTextBoxFocused;

    [RelayCommand]
    private void SetFocus()
    {
        IsTextBoxFocused = true;
    }
}
```

#### Avalonia Example 3: Select All on Focus

```xml
<StackPanel Margin="20">
    <TextBlock Text="Tab into the TextBox - all text will be selected:" 
               Margin="0,0,0,5" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="This text will be selected on focus">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior SelectAllOnFocus="True" />
        </ia:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

#### Avalonia Example 4: Focus Trigger

```xml
<StackPanel Margin="20">
    <Button Content="Trigger Focus" 
            Command="{Binding TriggerFocusCommand}"
            Width="150"
            HorizontalAlignment="Left"
            Margin="0,0,0,10" />
    
    <TextBox Width="300" 
             HorizontalAlignment="Left"
             Text="I will receive focus when triggered">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                FocusTrigger="{Binding FocusTrigger}" />
        </ia:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? focusTrigger;

    [RelayCommand]
    private void TriggerFocus()
    {
        FocusTrigger = new object();
    }
}
```

#### Avalonia Example 5: Search Box with Focus Management

```xml
<StackPanel Margin="20">
    <TextBlock Text="Search" 
               FontSize="18" 
               FontWeight="Bold"
               Margin="0,0,0,10" />
    
    <TextBox Width="400" 
             HorizontalAlignment="Left"
             Watermark="Type to search..."
             Text="{Binding SearchText, Mode=TwoWay}">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                HasInitialFocus="True"
                IsFocused="{Binding IsSearchFocused, Mode=TwoWay}"
                SelectAllOnFocus="True" />
        </ia:Interaction.Behaviors>
    </TextBox>
    
    <Button Content="Clear and Focus" 
            Command="{Binding ClearAndFocusCommand}"
            Margin="0,10,0,0"
            Width="150"
            HorizontalAlignment="Left" />
</StackPanel>
```

```csharp
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isSearchFocused;

    [RelayCommand]
    private void ClearAndFocus()
    {
        SearchText = string.Empty;
        IsSearchFocused = true;
    }
}
```

## Common Use Cases

### 1. Dialog First Field Focus

```xml
<StackPanel>
    <TextBlock Text="Name:" Margin="0,0,0,5" />
    <TextBox>
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior HasInitialFocus="True" />
        </ia:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

### 2. Search Box Auto-Select

```xml
<TextBox Watermark="Search...">
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior 
            SelectAllOnFocus="True"
            FocusTrigger="{Binding ShowSearchTrigger}" />
    </ia:Interaction.Behaviors>
</TextBox>
```

### 3. Validation Error Focus

```xml
<TextBox Text="{Binding Email, Mode=TwoWay}">
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior 
            IsFocused="{Binding HasEmailError, Mode=TwoWay}" />
    </ia:Interaction.Behaviors>
</TextBox>
```

```csharp
[RelayCommand]
private void Validate()
{
    if (string.IsNullOrEmpty(Email))
    {
        HasEmailError = true; // This will focus the TextBox
    }
}
```

### 4. Wizard Navigation

```xml
<StackPanel>
    <!-- Step 1 -->
    <TextBox Visibility="{Binding IsStep1Visible}">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                FocusTrigger="{Binding Step1FocusTrigger}" />
        </ia:Interaction.Behaviors>
    </TextBox>
    
    <!-- Step 2 -->
    <TextBox Visibility="{Binding IsStep2Visible}">
        <ia:Interaction.Behaviors>
            <behaviors:FocusBehavior 
                FocusTrigger="{Binding Step2FocusTrigger}" />
        </ia:Interaction.Behaviors>
    </TextBox>
</StackPanel>
```

## Tips and Best Practices

### ✅ Do's

- **Use HasInitialFocus for dialogs** - Improve user experience by focusing the first input
- **Use SelectAllOnFocus for editable fields** - Makes editing existing values easier
- **Bind IsFocused for validation** - Automatically focus fields with errors
- **Use FocusTrigger for dynamic focus** - Control focus based on application state

### ❌ Don'ts

- **Don't fight the platform** - Respect default focus behavior when appropriate
- **Don't create focus loops** - Be careful with IsFocused two-way binding
- **Don't force focus too aggressively** - Can be disruptive to user workflow
- **Don't forget keyboard users** - Ensure tab order is logical

## Focus Management Patterns

### Pattern 1: Form Validation

```csharp
public partial class FormViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isNameFocused;

    [RelayCommand]
    private void Submit()
    {
        if (string.IsNullOrEmpty(Name))
        {
            // Focus the field with error
            IsNameFocused = true;
            return;
        }
        
        // Submit form...
    }
}
```

### Pattern 2: Multi-Step Flow

```csharp
public partial class WizardViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? currentStepFocus;

    [RelayCommand]
    private void NextStep()
    {
        CurrentStep++;
        // Trigger focus on next step's first field
        CurrentStepFocus = new object();
    }
}
```

### Pattern 3: Search Box

```csharp
public partial class SearchViewModel : ViewModelBase
{
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isSearchFocused;

    [RelayCommand]
    private void ShowSearch()
    {
        IsSearchPanelVisible = true;
        IsSearchFocused = true;
    }
}
```

## Troubleshooting

### Focus Not Setting

**Problem:** Setting `IsFocused = true` doesn't focus the element

**Solution:** Ensure the element is visible and enabled:

```xml
<!-- ❌ Won't work if collapsed -->
<TextBox Visibility="Collapsed">
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior IsFocused="{Binding IsFocused}" />
    </ia:Interaction.Behaviors>
</TextBox>

<!-- ✅ Works correctly -->
<TextBox Visibility="Visible" IsEnabled="True">
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior IsFocused="{Binding IsFocused}" />
    </ia:Interaction.Behaviors>
</TextBox>
```

### SelectAllOnFocus Not Working

**Problem:** Text is not selected when TextBox receives focus

**Solution:** `SelectAllOnFocus` only works with `TextBox` controls, not other controls:

```xml
<!-- ❌ Won't work on other controls -->
<ComboBox>
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior SelectAllOnFocus="True" />
    </ia:Interaction.Behaviors>
</ComboBox>

<!-- ✅ Works on TextBox -->
<TextBox>
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior SelectAllOnFocus="True" />
    </ia:Interaction.Behaviors>
</TextBox>
```

### Focus Loop

**Problem:** Focus keeps jumping between fields

**Solution:** Be careful with IsFocused bindings on multiple fields:

```csharp
// ❌ Wrong - can create focus loop
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsField2Focused))]
private bool isField1Focused;

public bool IsField2Focused => !IsField1Focused;

// ✅ Correct - explicit control
[ObservableProperty]
private bool isField1Focused;

[ObservableProperty]
private bool isField2Focused;
```

## Accessibility Considerations

- **Respect focus order** - Ensure logical tab order for keyboard navigation
- **Provide focus indicators** - Make it clear which element has focus
- **Don't steal focus** - Only set focus in response to user actions
- **Support keyboard shortcuts** - Allow users to quickly navigate to important fields

## See Also

- [Behaviors Overview](Readme.md)
- [EventToCommandBehavior](EventToCommandBehavior.md)
- [KeyboardNavigationBehavior](KeyboardNavigationBehavior.md)
- [MVVM Framework](../Mvvm/Readme.md)
