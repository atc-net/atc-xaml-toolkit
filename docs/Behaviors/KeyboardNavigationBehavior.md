# KeyboardNavigationBehavior

The `KeyboardNavigationBehavior` enables custom keyboard navigation for UI elements through declarative command bindings. It supports arrow keys, Enter, Escape, and Tab navigation with customizable commands, making it ideal for grid navigation, custom controls, and accessibility enhancements.

## Overview

Instead of handling keyboard events in code-behind:

```csharp
// ❌ Avoid this
private void Element_KeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Up)
        NavigateUp();
    else if (e.Key == Key.Down)
        NavigateDown();
    // ... more key handling
}
```

Use `KeyboardNavigationBehavior` in XAML:

```xml
<!-- ✅ Do this -->
<behaviors:KeyboardNavigationBehavior 
    UpCommand="{Binding NavigateUpCommand}"
    DownCommand="{Binding NavigateDownCommand}"
    EnterCommand="{Binding SelectCommand}" />
```

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `UpCommand` | `ICommand` | Command executed when Up arrow key is pressed |
| `DownCommand` | `ICommand` | Command executed when Down arrow key is pressed |
| `LeftCommand` | `ICommand` | Command executed when Left arrow key is pressed |
| `RightCommand` | `ICommand` | Command executed when Right arrow key is pressed |
| `EnterCommand` | `ICommand` | Command executed when Enter key is pressed |
| `EscapeCommand` | `ICommand` | Command executed when Escape key is pressed |
| `TabCommand` | `ICommand` | Command executed when Tab key is pressed |
| `IsEnabled` | `bool` | Enable or disable the behavior (default: `true`) |

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

#### WPF Example 1: Grid Navigation

```xml
<Border BorderBrush="Gray" 
        BorderThickness="2" 
        Background="White"
        Width="400" 
        Height="300"
        Focusable="True">
    <i:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding NavigateUpCommand}"
            DownCommand="{Binding NavigateDownCommand}"
            LeftCommand="{Binding NavigateLeftCommand}"
            RightCommand="{Binding NavigateRightCommand}"
            EnterCommand="{Binding SelectCommand}"
            EscapeCommand="{Binding ResetCommand}" />
    </i:Interaction.Behaviors>
    
    <Grid ShowGridLines="True">
        <Grid.RowDefinitions>
            <RowDefinition />
            <RowDefinition />
            <RowDefinition />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition />
            <ColumnDefinition />
            <ColumnDefinition />
        </Grid.ColumnDefinitions>
        
        <!-- Highlight current position -->
        <Ellipse Grid.Row="{Binding CurrentRow}" 
                 Grid.Column="{Binding CurrentColumn}"
                 Fill="LightBlue" 
                 Width="30" 
                 Height="30"
                 HorizontalAlignment="Center" 
                 VerticalAlignment="Center"
                 Opacity="0.5" />
    </Grid>
</Border>
```

```csharp
public partial class GridNavigationViewModel : ViewModelBase
{
    [ObservableProperty]
    private int currentRow;

    [ObservableProperty]
    private int currentColumn;

    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentRow > 0)
            CurrentRow--;
    }

    [RelayCommand]
    private void NavigateDown()
    {
        if (CurrentRow < 2)
            CurrentRow++;
    }

    [RelayCommand]
    private void NavigateLeft()
    {
        if (CurrentColumn > 0)
            CurrentColumn--;
    }

    [RelayCommand]
    private void NavigateRight()
    {
        if (CurrentColumn < 2)
            CurrentColumn++;
    }

    [RelayCommand]
    private void Select()
    {
        // Handle selection
        Debug.WriteLine($"Selected: ({CurrentRow}, {CurrentColumn})");
    }

    [RelayCommand]
    private void Reset()
    {
        CurrentRow = 0;
        CurrentColumn = 0;
    }
}
```

#### WPF Example 2: List Navigation

```xml
<ListBox ItemsSource="{Binding Items}"
         SelectedIndex="{Binding SelectedIndex}"
         Focusable="True">
    <i:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding MoveUpCommand}"
            DownCommand="{Binding MoveDownCommand}"
            EnterCommand="{Binding SelectItemCommand}"
            EscapeCommand="{Binding ClearSelectionCommand}" />
    </i:Interaction.Behaviors>
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding}" Padding="5" />
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

```csharp
public partial class ListNavigationViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> items = new()
    {
        "Item 1", "Item 2", "Item 3", "Item 4", "Item 5"
    };

    [ObservableProperty]
    private int selectedIndex;

    [RelayCommand]
    private void MoveUp()
    {
        if (SelectedIndex > 0)
            SelectedIndex--;
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (SelectedIndex < Items.Count - 1)
            SelectedIndex++;
    }

    [RelayCommand]
    private void SelectItem()
    {
        if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            var item = Items[SelectedIndex];
            Debug.WriteLine($"Selected: {item}");
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedIndex = -1;
    }
}
```

#### WPF Example 3: Custom Control Navigation

```xml
<Canvas Width="500" Height="400" Background="WhiteSmoke" Focusable="True">
    <i:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding MoveUpCommand}"
            DownCommand="{Binding MoveDownCommand}"
            LeftCommand="{Binding MoveLeftCommand}"
            RightCommand="{Binding MoveRightCommand}" />
    </i:Interaction.Behaviors>
    
    <Ellipse Canvas.Left="{Binding X}" 
             Canvas.Top="{Binding Y}"
             Width="50" 
             Height="50"
             Fill="DodgerBlue" />
</Canvas>
```

```csharp
public partial class CanvasNavigationViewModel : ViewModelBase
{
    private const int Step = 10;

    [ObservableProperty]
    private double x = 100;

    [ObservableProperty]
    private double y = 100;

    [RelayCommand]
    private void MoveUp() => Y = Math.Max(0, Y - Step);

    [RelayCommand]
    private void MoveDown() => Y = Math.Min(350, Y + Step);

    [RelayCommand]
    private void MoveLeft() => X = Math.Max(0, X - Step);

    [RelayCommand]
    private void MoveRight() => X = Math.Min(450, X + Step);
}
```

#### WPF Example 4: Conditional Navigation

```xml
<StackPanel Focusable="True">
    <i:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            IsEnabled="{Binding IsNavigationEnabled}"
            UpCommand="{Binding NavigateUpCommand}"
            DownCommand="{Binding NavigateDownCommand}" />
    </i:Interaction.Behaviors>
    
    <CheckBox Content="Enable Keyboard Navigation" 
              IsChecked="{Binding IsNavigationEnabled}"
              Margin="0,0,0,10" />
    
    <TextBlock Text="{Binding CurrentIndex, StringFormat='Current Index: {0}'}" />
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

#### WinUI Example 1: Grid Navigation

```xml
<Border BorderBrush="Gray" 
        BorderThickness="2" 
        Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
        Width="400" 
        Height="300">
    <interactivity:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{x:Bind ViewModel.NavigateUpCommand}"
            DownCommand="{x:Bind ViewModel.NavigateDownCommand}"
            LeftCommand="{x:Bind ViewModel.NavigateLeftCommand}"
            RightCommand="{x:Bind ViewModel.NavigateRightCommand}"
            EnterCommand="{x:Bind ViewModel.HandleEnterCommand}"
            EscapeCommand="{x:Bind ViewModel.HandleEscapeCommand}" />
    </interactivity:Interaction.Behaviors>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition />
            <RowDefinition />
            <RowDefinition />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition />
            <ColumnDefinition />
            <ColumnDefinition />
        </Grid.ColumnDefinitions>
        
        <!-- Highlight current position -->
        <Ellipse Grid.Row="{x:Bind ViewModel.CurrentRow, Mode=OneWay}" 
                 Grid.Column="{x:Bind ViewModel.CurrentColumn, Mode=OneWay}"
                 Fill="LightBlue" 
                 Width="30" 
                 Height="30"
                 HorizontalAlignment="Center" 
                 VerticalAlignment="Center"
                 Opacity="0.5" />
    </Grid>
</Border>
```

#### WinUI Example 2: Menu Navigation

```xml
<StackPanel>
    <TextBlock Text="Use arrow keys to navigate menu" 
               Margin="0,0,0,10" />
    
    <Border BorderBrush="Gray" BorderThickness="1" Padding="10">
        <interactivity:Interaction.Behaviors>
            <behaviors:KeyboardNavigationBehavior
                UpCommand="{x:Bind ViewModel.PreviousMenuItemCommand}"
                DownCommand="{x:Bind ViewModel.NextMenuItemCommand}"
                EnterCommand="{x:Bind ViewModel.SelectMenuItemCommand}"
                EscapeCommand="{x:Bind ViewModel.CloseMenuCommand}" />
        </interactivity:Interaction.Behaviors>
        
        <ItemsControl ItemsSource="{x:Bind ViewModel.MenuItems}">
            <ItemsControl.ItemTemplate>
                <DataTemplate x:DataType="x:String">
                    <Border Background="{Binding IsSelected, Converter={StaticResource SelectedToBrushConverter}}"
                            Padding="10,5">
                        <TextBlock Text="{x:Bind}" />
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Border>
</StackPanel>
```

#### WinUI Example 3: Tab Navigation

```xml
<StackPanel>
    <Border Padding="10" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}">
        <interactivity:Interaction.Behaviors>
            <behaviors:KeyboardNavigationBehavior
                TabCommand="{x:Bind ViewModel.NextFieldCommand}"
                EnterCommand="{x:Bind ViewModel.SubmitCommand}" />
        </interactivity:Interaction.Behaviors>
        
        <StackPanel Spacing="10">
            <TextBox Header="Name" 
                     Text="{x:Bind ViewModel.Name, Mode=TwoWay}" />
            <TextBox Header="Email" 
                     Text="{x:Bind ViewModel.Email, Mode=TwoWay}" />
            <TextBox Header="Phone" 
                     Text="{x:Bind ViewModel.Phone, Mode=TwoWay}" />
        </StackPanel>
    </Border>
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

#### Avalonia Example 1: Grid Navigation

```xml
<Border BorderBrush="Gray" 
        BorderThickness="2"
        Background="White"
        Width="400" 
        Height="300"
        Focusable="True">
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding NavigateUpCommand}"
            DownCommand="{Binding NavigateDownCommand}"
            LeftCommand="{Binding NavigateLeftCommand}"
            RightCommand="{Binding NavigateRightCommand}"
            EnterCommand="{Binding HandleEnterCommand}"
            EscapeCommand="{Binding HandleEscapeCommand}" />
    </ia:Interaction.Behaviors>
    
    <Grid ShowGridLines="True">
        <Grid.RowDefinitions>
            <RowDefinition />
            <RowDefinition />
            <RowDefinition />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition />
            <ColumnDefinition />
            <ColumnDefinition />
        </Grid.ColumnDefinitions>
        
        <!-- Grid cell labels -->
        <TextBlock Grid.Row="0" Grid.Column="0" 
                   Text="(0,0)" 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center" />
        <TextBlock Grid.Row="0" Grid.Column="1" 
                   Text="(0,1)" 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center" />
        <!-- ... more cells ... -->
        
        <!-- Highlight current position -->
        <Ellipse Grid.Row="{Binding CurrentRow}" 
                 Grid.Column="{Binding CurrentColumn}"
                 Fill="LightBlue" 
                 Width="30" 
                 Height="30"
                 HorizontalAlignment="Center" 
                 VerticalAlignment="Center"
                 Opacity="0.5" />
    </Grid>
</Border>
```

```csharp
public partial class GridNavigationViewModel : ViewModelBase
{
    [ObservableProperty]
    private int currentRow;

    [ObservableProperty]
    private int currentColumn;

    [ObservableProperty]
    private string navigationLog = string.Empty;

    [RelayCommand]
    private void NavigateUp()
    {
        if (CurrentRow > 0)
        {
            CurrentRow--;
            LogNavigation("Up");
        }
    }

    [RelayCommand]
    private void NavigateDown()
    {
        if (CurrentRow < 2)
        {
            CurrentRow++;
            LogNavigation("Down");
        }
    }

    [RelayCommand]
    private void NavigateLeft()
    {
        if (CurrentColumn > 0)
        {
            CurrentColumn--;
            LogNavigation("Left");
        }
    }

    [RelayCommand]
    private void NavigateRight()
    {
        if (CurrentColumn < 2)
        {
            CurrentColumn++;
            LogNavigation("Right");
        }
    }

    [RelayCommand]
    private void HandleEnter()
    {
        LogNavigation($"Selected ({CurrentRow}, {CurrentColumn})");
    }

    [RelayCommand]
    private void HandleEscape()
    {
        CurrentRow = 0;
        CurrentColumn = 0;
        NavigationLog = string.Empty;
    }

    private void LogNavigation(string action)
    {
        NavigationLog += $"{DateTime.Now:HH:mm:ss} - {action} -> ({CurrentRow}, {CurrentColumn})\n";
    }
}
```

#### Avalonia Example 2: Image Gallery Navigation

```xml
<Border BorderBrush="Gray" BorderThickness="1" Padding="10">
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            LeftCommand="{Binding PreviousImageCommand}"
            RightCommand="{Binding NextImageCommand}"
            EscapeCommand="{Binding CloseGalleryCommand}" />
    </ia:Interaction.Behaviors>
    
    <StackPanel>
        <Image Source="{Binding CurrentImage}" 
               Width="600" 
               Height="400"
               Stretch="Uniform" />
        
        <StackPanel Orientation="Horizontal" 
                    HorizontalAlignment="Center"
                    Margin="0,10,0,0">
            <TextBlock Text="{Binding CurrentImageIndex, StringFormat='Image {0}'}" />
            <TextBlock Text=" of " />
            <TextBlock Text="{Binding TotalImages}" />
        </StackPanel>
        
        <TextBlock Text="Use Left/Right arrows to navigate, Escape to close"
                   HorizontalAlignment="Center"
                   FontSize="12"
                   Foreground="Gray"
                   Margin="0,5,0,0" />
    </StackPanel>
</Border>
```

```csharp
public partial class GalleryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> images = new()
    {
        "image1.jpg", "image2.jpg", "image3.jpg"
    };

    [ObservableProperty]
    private int currentImageIndex;

    public string CurrentImage => 
        CurrentImageIndex >= 0 && CurrentImageIndex < Images.Count 
            ? Images[CurrentImageIndex] 
            : string.Empty;

    public int TotalImages => Images.Count;

    [RelayCommand]
    private void PreviousImage()
    {
        if (CurrentImageIndex > 0)
            CurrentImageIndex--;
    }

    [RelayCommand]
    private void NextImage()
    {
        if (CurrentImageIndex < Images.Count - 1)
            CurrentImageIndex++;
    }

    [RelayCommand]
    private void CloseGallery()
    {
        // Close gallery logic
    }
}
```

#### Avalonia Example 3: Custom Spreadsheet Navigation

```xml
<DataGrid ItemsSource="{Binding Cells}"
          AutoGenerateColumns="False"
          GridLinesVisibility="All">
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding MoveCellUpCommand}"
            DownCommand="{Binding MoveCellDownCommand}"
            LeftCommand="{Binding MoveCellLeftCommand}"
            RightCommand="{Binding MoveCellRightCommand}"
            EnterCommand="{Binding EditCellCommand}"
            TabCommand="{Binding NextCellCommand}" />
    </ia:Interaction.Behaviors>
    
    <DataGrid.Columns>
        <DataGridTextColumn Header="A" Binding="{Binding ColumnA}" />
        <DataGridTextColumn Header="B" Binding="{Binding ColumnB}" />
        <DataGridTextColumn Header="C" Binding="{Binding ColumnC}" />
    </DataGrid.Columns>
</DataGrid>
```

## Common Use Cases

### 1. Calendar Navigation

```xml
<Calendar>
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            LeftCommand="{Binding PreviousDayCommand}"
            RightCommand="{Binding NextDayCommand}"
            UpCommand="{Binding PreviousWeekCommand}"
            DownCommand="{Binding NextWeekCommand}"
            EnterCommand="{Binding SelectDateCommand}" />
    </ia:Interaction.Behaviors>
</Calendar>
```

### 2. Game Controls

```xml
<Canvas>
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding MovePlayerUpCommand}"
            DownCommand="{Binding MovePlayerDownCommand}"
            LeftCommand="{Binding MovePlayerLeftCommand}"
            RightCommand="{Binding MovePlayerRightCommand}"
            EnterCommand="{Binding PlayerActionCommand}"
            EscapeCommand="{Binding PauseGameCommand}" />
    </ia:Interaction.Behaviors>
</Canvas>
```

### 3. Dropdown Menu Navigation

```xml
<ListBox>
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            UpCommand="{Binding PreviousItemCommand}"
            DownCommand="{Binding NextItemCommand}"
            EnterCommand="{Binding SelectItemCommand}"
            EscapeCommand="{Binding CloseMenuCommand}" />
    </ia:Interaction.Behaviors>
</ListBox>
```

### 4. Form Field Navigation

```xml
<StackPanel>
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            TabCommand="{Binding NextFieldCommand}"
            EnterCommand="{Binding SubmitFormCommand}"
            EscapeCommand="{Binding CancelFormCommand}" />
    </ia:Interaction.Behaviors>
</StackPanel>
```

## Tips and Best Practices

### ✅ Do's

- **Make elements focusable** - Set `Focusable="True"` on the element with the behavior
- **Provide visual feedback** - Show users which element or cell is currently selected
- **Use boundary checking** - Prevent navigation beyond valid ranges
- **Support Escape for cancellation** - Give users a way to exit or reset
- **Log actions for debugging** - Track navigation events during development

### ❌ Don'ts

- **Don't override standard controls** - Respect default keyboard behavior when appropriate
- **Don't forget accessibility** - Ensure keyboard navigation is logical and intuitive
- **Don't create navigation traps** - Always provide a way to exit focus
- **Don't ignore CanExecute** - Disable commands when navigation is not valid

## Command Parameter Usage

The behavior passes `KeyEventArgs` to commands, allowing access to modifier keys:

```csharp
[RelayCommand]
private void NavigateRight(object parameter)
{
    if (parameter is KeyEventArgs e)
    {
        // Check for Shift+Right (faster navigation)
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            CurrentColumn += 5;
        }
        else
        {
            CurrentColumn++;
        }
    }
}
```

## Enabling/Disabling Navigation

Dynamically control when keyboard navigation is active:

```csharp
public partial class ViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isNavigationEnabled = true;

    [RelayCommand]
    private void ToggleNavigation()
    {
        IsNavigationEnabled = !IsNavigationEnabled;
    }
}
```

```xml
<Border>
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior
            IsEnabled="{Binding IsNavigationEnabled}"
            UpCommand="{Binding NavigateUpCommand}"
            DownCommand="{Binding NavigateDownCommand}" />
    </ia:Interaction.Behaviors>
</Border>
```

## Troubleshooting

### Commands Not Executing

**Problem:** Key presses don't execute commands

**Solution:** Ensure the element is focusable and has focus:

```xml
<!-- ❌ Won't receive keyboard events -->
<Border Focusable="False">
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior ... />
    </ia:Interaction.Behaviors>
</Border>

<!-- ✅ Can receive keyboard events -->
<Border Focusable="True">
    <ia:Interaction.Behaviors>
        <behaviors:KeyboardNavigationBehavior ... />
    </ia:Interaction.Behaviors>
</Border>
```

### Focus Issues

**Problem:** Element doesn't receive focus on load

**Solution:** Set focus programmatically or use FocusBehavior:

```xml
<Border Focusable="True">
    <ia:Interaction.Behaviors>
        <behaviors:FocusBehavior HasInitialFocus="True" />
        <behaviors:KeyboardNavigationBehavior ... />
    </ia:Interaction.Behaviors>
</Border>
```

### Key Events Bubbling

**Problem:** Parent controls also handle key events

**Solution:** Mark events as handled in your commands:

```csharp
[RelayCommand]
private void NavigateUp(object parameter)
{
    if (parameter is KeyEventArgs e)
    {
        // Handle navigation
        CurrentRow--;
        
        // Mark event as handled to prevent bubbling
        e.Handled = true;
    }
}
```

## Performance Considerations

- **Lightweight behavior** - Minimal overhead on UI thread
- **Event handling** - Uses PreviewKeyDown for early interception
- **Command execution** - Only executes when CanExecute returns true
- **Works with virtualization** - Compatible with virtualized controls

## Accessibility

- **Standard keyboard support** - Works with screen readers
- **Focus management** - Respects focus order and keyboard focus
- **Visual feedback** - Ensure visible focus indicators
- **Logical navigation** - Follow platform conventions

## See Also

- [Behaviors Overview](Readme.md)
- [EventToCommandBehavior](EventToCommandBehavior.md)
- [FocusBehavior](FocusBehavior.md)
- [MVVM Framework](../Mvvm/Readme.md)
- [Commands](../Mvvm/Readme.md#commands)
