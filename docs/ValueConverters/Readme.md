# Atc.XamlToolkit - Schemas for ValueConverters

This directory documents the collection of reusable and cross-platform-friendly `ValueConverters` available in **Atc.XamlToolkit**.

These converters are organized by their **input/output types**, making it easy to locate the correct one based on the data transformation needed in your XAML bindings.

---

## 🚀 Quick Start

For WPF:

```powershell
dotnet add package Atc.XamlToolkit.Wpf
```

For WinUI:

```powershell
dotnet add package Atc.XamlToolkit.WinUI
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

For WinUI:

```xml
xmlns:atcToolkitValueConverters="using:Atc.XamlToolkit.ValueConverters"
```

For Avalonia:

```xml
xmlns:atcToolkitValueConverters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Avalonia"
```

---

## 🧹 Usage

To use a converter in WPF, WinUI, or Avalonia by ResourceDictionary and key:

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

## #️⃣ ValueConverters - Null to [...]

| Category                  | Type                                                    | Source → Target                    | WPF Convert Example                                   | Avalonia Convert Example                              | ConvertBack   |
|---------------------------|---------------------------------------------------------|------------------------------------|-------------------------------------------------------|-------------------------------------------------------|---------------|
| Null → Visibility         | `NullToVisibilityCollapsedValueConverter`               | object? → Visibility               | null → Collapsed<br/>value → Visible                  | null → False<br/>value → True                         | ✅            |
| Null → Visibility         | `NullToVisibilityVisibleValueConverter`                 | object? → Visibility               | null → Visible<br/>value → Collapsed                  | null → True<br/>value → False                         | ✅            |

### Null Converter Usage Examples

#### Example 1: Show loading indicator when data is null

**WPF:**

```xml
<!-- Show loading spinner only when CurrentUser is null -->
<ProgressRing
    IsActive="True"
    Visibility="{Binding CurrentUser, Converter={x:Static atcToolkitValueConverters:NullToVisibilityVisibleValueConverter.Instance}}" />

<!-- Show user details only when CurrentUser is NOT null -->
<StackPanel
    Visibility="{Binding CurrentUser, Converter={x:Static atcToolkitValueConverters:NullToVisibilityCollapsedValueConverter.Instance}}">
    <TextBlock Text="{Binding CurrentUser.Name}" />
    <TextBlock Text="{Binding CurrentUser.Email}" />
</StackPanel>
```

**Avalonia:**

```xml
<!-- Show loading spinner only when CurrentUser is null -->
<ProgressBar
    IsIndeterminate="True"
    IsVisible="{Binding CurrentUser, Converter={x:Static atcToolkitValueConverters:NullToVisibilityVisibleValueConverter.Instance}}" />

<!-- Show user details only when CurrentUser is NOT null -->
<StackPanel
    IsVisible="{Binding CurrentUser, Converter={x:Static atcToolkitValueConverters:NullToVisibilityCollapsedValueConverter.Instance}}">
    <TextBlock Text="{Binding CurrentUser.Name}" />
    <TextBlock Text="{Binding CurrentUser.Email}" />
</StackPanel>
```

#### Example 2: Show placeholder when item is not selected

**WPF:**

```xml
<!-- Show "No selection" message when SelectedItem is null -->
<TextBlock
    Text="No item selected. Please select an item from the list."
    Visibility="{Binding SelectedItem, Converter={x:Static atcToolkitValueConverters:NullToVisibilityVisibleValueConverter.Instance}}" />
```

**Avalonia:**

```xml
<!-- Show "No selection" message when SelectedItem is null -->
<TextBlock
    Text="No item selected. Please select an item from the list."
    IsVisible="{Binding SelectedItem, Converter={x:Static atcToolkitValueConverters:NullToVisibilityVisibleValueConverter.Instance}}" />
```

#### Example 3: Conditional UI based on optional data

**WPF:**

```xml
<!-- Show edit button only when EditableDocument is loaded (not null) -->
<Button
    Content="Edit Document"
    Visibility="{Binding EditableDocument, Converter={x:Static atcToolkitValueConverters:NullToVisibilityCollapsedValueConverter.Instance}}" />
```

**WinUI:**

```xml
<!-- Show edit button only when EditableDocument is loaded (not null) -->
<Button
    Content="Edit Document"
    Visibility="{Binding EditableDocument, Converter={x:Static atcToolkitValueConverters:NullToVisibilityCollapsedValueConverter.Instance}}" />
```

---