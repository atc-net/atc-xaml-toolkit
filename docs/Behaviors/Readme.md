# Behaviors

Behaviors provide a way to add interactivity to UI elements without code-behind. The Atc.XamlToolkit provides powerful behaviors that work across WPF, WinUI, and Avalonia platforms.

## Available Behaviors

### EventToCommandBehavior

Execute commands in response to any UI event, eliminating the need for code-behind event handlers.

[Learn more about EventToCommandBehavior](EventToCommandBehavior.md)

## Platform Support

| Behavior | WPF | WinUI | Avalonia |
|----------|-----|-------|----------|
| EventToCommandBehavior | ✅ | ✅ | ✅ |

## Getting Started

### Installation

Behaviors are included in the platform-specific packages:

**For WPF:**
```powershell
dotnet add package Atc.XamlToolkit.Wpf
dotnet add package Microsoft.Xaml.Behaviors.Wpf
```

**For WinUI:**
```powershell
dotnet add package Atc.XamlToolkit.WinUI
# Microsoft.Xaml.Behaviors.WinUI.Managed is automatically included
```

**For Avalonia:**
```powershell
dotnet add package Atc.XamlToolkit.Avalonia
# Avalonia.Xaml.Behaviors is automatically included
```

### Basic Usage

#### WPF

```xml
<Window xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
        xmlns:behaviors="clr-namespace:Atc.XamlToolkit.Behaviors;assembly=Atc.XamlToolkit.Wpf">
    
    <Button Content="Click Me">
        <i:Interaction.Behaviors>
            <behaviors:EventToCommandBehavior 
                EventName="Click"
                Command="{Binding MyCommand}" />
        </i:Interaction.Behaviors>
    </Button>
</Window>
```

#### WinUI

```xml
<Page xmlns:interactivity="using:Microsoft.Xaml.Interactivity"
      xmlns:behaviors="using:Atc.XamlToolkit.Behaviors">
    
    <Button Content="Click Me">
        <interactivity:Interaction.Behaviors>
            <behaviors:EventToCommandBehavior 
                EventName="Click"
                Command="{x:Bind ViewModel.MyCommand}" />
        </interactivity:Interaction.Behaviors>
    </Button>
</Page>
```

#### Avalonia

```xml
<Window xmlns:ia="using:Avalonia.Xaml.Interactivity"
        xmlns:behaviors="using:Atc.XamlToolkit.Behaviors">
    
    <Button Content="Click Me">
        <ia:Interaction.Behaviors>
            <behaviors:EventToCommandBehavior 
                EventName="Click"
                Command="{Binding MyCommand}" />
        </ia:Interaction.Behaviors>
    </Button>
</Window>
```

## Benefits

- **No Code-Behind** - Keep your Views clean and testable
- **MVVM Friendly** - Commands stay in your ViewModels
- **Cross-Platform** - Same API across WPF, WinUI, and Avalonia
- **Reusable** - Apply behaviors to any control supporting the event
- **Type-Safe** - Compile-time checking of event names

## See Also

- [MVVM Framework](../Mvvm/Readme.md)
- [Commands](../Mvvm/Readme.md#commands)
- [Getting Started Guide](../GettingStarted.md)
