namespace Atc.XamlToolkit.SourceGenerators.Factories;

internal static class SimpleTypeFactory
{
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    internal static string? CreateDefaultValueAsStrForType(
        string type,
        XamlPlatform xamlPlatform = XamlPlatform.Wpf)
        => type switch
        {
            // Primitive types
            "bool" => xamlPlatform == XamlPlatform.Avalonia ? "false" : "Atc.XamlToolkit.BooleanBoxes.FalseBox",
            "byte" => "(byte)0",
            "sbyte" => "(sbyte)0",
            "char" => "'\\0'",
            "decimal" => "0m",
            "double" => "0d",
            "float" => "0f",
            "int" => "0",
            "uint" => "0U",
            "short" => "(short)0",
            "ushort" => "(ushort)0",
            "long" => "0",
            "ulong" => "0UL",
            "nint" => "nint.Zero",
            "nuint" => "nuint.Zero",

            // System structs
            "Guid" => "Guid.Empty",
            "DateTime" => "DateTime.MinValue",
            "DateTimeOffset" => "DateTimeOffset.MinValue",
            "TimeSpan" => "TimeSpan.Zero",

            // WPF / WinUI core structs
            "Color" => "Colors.Transparent",
            "Point" => "new Point(0, 0)",
            "Rect" => "Rect.Empty",
            "Size" => "Size.Empty",
            "Thickness" => "new Thickness(0)",
            "Vector" => "new Vector(0, 0)",
            "CornerRadius" => "new CornerRadius(0)",
            "Matrix" => "Matrix.Identity",
            "Matrix3D" => "Matrix3D.Identity",
            "Point3D" => "new Point3D(0, 0, 0)",
            "Vector3D" => "new Vector3D(0, 0, 0)",
            "Quaternion" => "Quaternion.Identity",
            "Rect3D" => "Rect3D.Empty",
            "Int32Rect" => "Int32Rect.Empty",
            "Duration" => "Duration.Automatic",
            "GridLength" => "GridLength.Auto",
            "FontStretch" => "FontStretches.Normal",
            "FontStyle" => "FontStyles.Normal",
            "FontWeight" => "FontWeights.Normal",

            // Common enums (layout / visibility / flow)
            "HorizontalAlignment" => "HorizontalAlignment.Left",
            "VerticalAlignment" => "VerticalAlignment.Top",
            "FlowDirection" => "FlowDirection.LeftToRight",
            "Orientation" => "Orientation.Horizontal",
            "TextAlignment" => "TextAlignment.Left",
            "TextWrapping" => "TextWrapping.NoWrap",
            "Dock" => "Dock.Left",
            "Visibility" => "Visibility.Visible",
            "ScrollBarVisibility" => "ScrollBarVisibility.Auto",
            "ResizeMode" => "ResizeMode.CanResize",
            "WindowState" => "WindowState.Normal",

            // Media / drawing enums
            "Stretch" => "Stretch.None",
            "AlignmentX" => "AlignmentX.Center",
            "AlignmentY" => "AlignmentY.Center",
            "BitmapScalingMode" => "BitmapScalingMode.Unspecified",
            "PenLineCap" => "PenLineCap.Flat",
            "PenLineJoin" => "PenLineJoin.Miter",
            "FillRule" => "FillRule.EvenOdd",
            "SweepDirection" => "SweepDirection.Counterclockwise",

            // Custom enums
            "ImageLocation" => "ImageLocation.None",

            _ => null,
        };
}