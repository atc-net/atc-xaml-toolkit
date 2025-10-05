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

## #️⃣ ValueConverters - String to [...]

| Category                  | Type                                                    | Source → Target                    | WPF Convert Example                                   | Avalonia Convert Example                              | ConvertBack   |
|---------------------------|---------------------------------------------------------|------------------------------------|-------------------------------------------------------|-------------------------------------------------------|---------------|
| String → Bool             | `StringNullOrEmptyToBoolValueConverter`                 | string → bool                      | null/empty → True<br/>"text" → False                  | null/empty → True<br/>"text" → False                  | Not supported |
| String → Bool             | `StringNullOrEmptyToInverseBoolValueConverter`          | string → bool                      | null/empty → False<br/>"text" → True                  | null/empty → False<br/>"text" → True                  | Not supported |
| String → Visibility       | `StringNullOrEmptyToVisibilityVisibleValueConverter`    | string → Visibility                | null/empty → Visible<br/>"text" → Collapsed           | null/empty → True<br/>"text" → False                  | Not supported |
| String → Visibility       | `StringNullOrEmptyToVisibilityCollapsedValueConverter`  | string → Visibility                | null/empty → Collapsed<br/>"text" → Visible           | null/empty → False<br/>"text" → True                  | Not supported |
| String → String           | `ToLowerValueConverter`                                 | string → string                    | "HELLO" → "hello"<br/>"World" → "world"               | "HELLO" → "hello"<br/>"World" → "world"               | Not supported |
| String → String           | `ToUpperValueConverter`                                 | string → string                    | "hello" → "HELLO"<br/>"World" → "WORLD"               | "hello" → "HELLO"<br/>"World" → "WORLD"               | Not supported |

### String Converter Usage Examples

#### Example 1: Show/Hide UI based on string content

**WPF:**

```xml
<!-- Show TextBlock only if Name is not empty (using Visibility) -->
<TextBlock 
    Text="Name is provided" 
    Visibility="{Binding Name, Converter={x:Static atcToolkitValueConverters:StringNullOrEmptyToVisibilityCollapsedValueConverter.Instance}}" />
```

**Avalonia:**

```xml
<!-- Show TextBlock only if Name is not empty (using IsVisible) -->
<TextBlock 
    Text="Name is provided" 
    IsVisible="{Binding Name, Converter={x:Static atcToolkitValueConverters:StringNullOrEmptyToInverseBoolValueConverter.Instance}}" />
```

#### Example 2: Convert text to uppercase in binding

**WPF and Avalonia:**

```xml
<!-- Display username in uppercase -->
<TextBlock 
    Text="{Binding UserName, Converter={x:Static atcToolkitValueConverters:ToUpperValueConverter.Instance}}" />
```

#### Example 3: Show placeholder when string is empty

**WPF:**

```xml
<!-- Show placeholder text when input is empty -->
<TextBlock 
    Text="Enter your name..." 
    Visibility="{Binding UserInput, Converter={x:Static atcToolkitValueConverters:StringNullOrEmptyToVisibilityVisibleValueConverter.Instance}}" />
```

**Avalonia:**

```xml
<!-- Show placeholder text when input is empty -->
<TextBlock 
    Text="Enter your name..." 
    IsVisible="{Binding UserInput, Converter={x:Static atcToolkitValueConverters:StringNullOrEmptyToBoolValueConverter.Instance}}" />
```

---