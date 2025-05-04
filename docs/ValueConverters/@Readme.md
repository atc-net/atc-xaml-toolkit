# Atc.XamlToolkit - Schemas for ValueConverters

This directory documents the collection of reusable and cross-platform-friendly `ValueConverters` available in **Atc.XamlToolkit**.

These converters are organized by their **input/output types**, making it easy to locate the correct one based on the data transformation needed in your XAML bindings.

---

## 🚀 Quick Start

For WPF:

```powershell
dotnet add package Atc.XamlToolkit.Wpf
```

For Avalonia:

```powershell
 dotnet add package Atc.XamlToolkit.Avalonia
```

## 📜 XML Namespace Mapping


For WPF:

```xml
xmlns:atcToolkitValueConverters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Wpf"
```

For Avalonia:

```xml
xmlns:atcToolkitValueConverters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Avalonia"
```

---

## 🧹 Usage

To use a converter in WPF or Avalonia by ResourceDictionary and and key:

```xml
<UserControl.Resources>
    <ResourceDictionary>
        <atcValueConverters:BoolToVisibilityVisibleValueConverter x:Key="BoolToVisibilityVisibleValueConverter" />
    </ResourceDictionary>
</UserControl.Resources>

<StackPanel Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibilityVisibleValueConverter}}" />
```

Or by the ValueConverter's Instance:

```xml
<StackPanel Visibility="{Binding IsVisible, Converter={x:Static atcToolkitValueConverters:BoolToVisibilityVisibleValueConverter.Instance}}" />
```

---

## #️⃣ ValueConverters - Bool to [...]

| Category                  | Type                                                    | Source → Target                    | WPF Convert Example                                   | Avalonia Convert Example        | ConvertBack   |
|---------------------------|---------------------------------------------------------|------------------------------------|-------------------------------------------------------|---------------------------------|---------------|
| Bool → Bool               | `BoolToInverseBoolValueConverter`                       | bool → bool                        | True → False<br/>False → True                         | True → False<br/>False → True   |               |
| Bool → Visibility         | `BoolToVisibilityCollapsedValueConverter`               | bool → Visibility                  | True → Collapsed<br/>False → Visible                  | —                               |               |
| Bool → Visibility         | `BoolToVisibilityVisibleValueConverter`                 | bool → Visibility                  | True → Visible<br/>False → Collapsed                  | True → True<br/>False → False   |               |
| Bool → Width              | `BoolToWidthValueConverter`                             | bool + param → double/Auto         | True<br/>10 → 10<br/>True<br/>Auto → Auto             | —                               | Not supported |
| Bool[] → Bool             | `MultiBoolToBoolValueConverter`                         | bool[] → bool                      | All True → True (AND)<br/>One True → True (OR)        | —                               | Not supported |
| Bool[] → Visibility       | `MultiBoolToVisibilityVisibleValueConverter`            | bool[] → Visibility                | All True → Visible (AND)<br/>One True → Visible (OR)  | —                               | Not supported |

---