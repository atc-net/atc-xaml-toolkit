namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class XamlStylerServiceTests : IDisposable
{
    private const string UnformattedXaml = """
        <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Title="Test" Height="450" Width="800">
            <Grid>
                <TextBlock Text="Hello" Margin="10" />
            </Grid>
        </Window>
        """;

    private readonly string tempDir;

    public XamlStylerServiceTests()
    {
        tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void StyleDocument_EmptyString_ThrowsXmlException()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var act = () => service.StyleDocument(string.Empty);

        // Assert
        act.Should().Throw<System.Xml.XmlException>();
    }

    [Fact]
    public void StyleDocument_ValidXaml_ReturnsFormattedXaml()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(UnformattedXaml);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().NotBe(UnformattedXaml);
    }

    [Fact]
    public void StyleDocument_AlreadyFormatted_ReturnsIdentical()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // First, format the XAML
        var formatted = service.StyleDocument(UnformattedXaml);

        // Act - format again
        var result = service.StyleDocument(formatted);

        // Assert
        result.Should().Be(formatted);
    }

    [Fact]
    public void StyleDocument_SuppressProcessing_ReturnsOriginal()
    {
        // Arrange
        var options = new XamlStylerOptions { SuppressProcessing = true };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(UnformattedXaml);

        // Assert
        result.Should().Be(UnformattedXaml);
    }

    [Fact]
    public void StyleFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);
        var nonExistentPath = Path.Combine(tempDir, "does_not_exist.xaml");

        // Act
        var act = () => service.StyleFile(nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void StyleFile_AlreadyFormatted_ReturnsFalse()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Pre-format the XAML and write to file
        var formatted = service.StyleDocument(UnformattedXaml);
        var filePath = CreateTempFile(
            "formatted.xaml",
            formatted);

        // Act
        var result = service.StyleFile(filePath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void StyleFile_NeedsFormatting_ReturnsTrueAndModifiesFile()
    {
        // Arrange
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);
        var filePath = CreateTempFile(
            "unformatted.xaml",
            UnformattedXaml);

        // Act
        var result = service.StyleFile(filePath);

        // Assert
        result.Should().BeTrue();

        var fileContent = File.ReadAllText(filePath);
        fileContent.Should().NotBe(UnformattedXaml);
    }

    [Fact]
    public void StyleDocument_NestedMarkupExtension_PreservesBinding()
    {
        // Arrange
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock Text="{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=Name}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("RelativeSource={RelativeSource AncestorType=Control}");
        result.Should().NotContain("Atc.XamlToolkit.XamlStyler");
    }

    [Fact]
    public void StyleDocument_DesignTimeReferences_PreservedByDefault()
    {
        // Arrange
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                    mc:Ignorable="d"
                    d:DesignWidth="800">
                <Grid />
            </Window>
            """;
        var options = new XamlStylerOptions { RemoveDesignTimeReferences = false };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("xmlns:d=");
        result.Should().Contain("d:DesignWidth=");
        result.Should().Contain("xmlns:mc=");
        result.Should().Contain("mc:Ignorable=");
    }

    [Fact]
    public void StyleDocument_DesignTimeReferences_RemovedWhenEnabled()
    {
        // Arrange
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                    mc:Ignorable="d"
                    d:DesignWidth="800">
                <Grid />
            </Window>
            """;
        var options = new XamlStylerOptions { RemoveDesignTimeReferences = true };
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().NotContain("xmlns:d=");
        result.Should().NotContain("d:DesignWidth=");
    }

    [Fact]
    public void StyleDocument_NestedRelativeSourceBinding_Preserved()
    {
        // Arrange — real pattern from atc-wpf MainWindow.xaml / CheckBox.xaml
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <TextBlock TextElement.FontFamily="{Binding RelativeSource={RelativeSource AncestorType=ContentControlEx}, Path=FontFamily}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("RelativeSource={RelativeSource AncestorType=ContentControlEx}");
        result.Should().Contain("Path=FontFamily");
        result.Should().NotContainAny("Atc.XamlToolkit", "XamlStyler");
    }

    [Fact]
    public void StyleDocument_NestedStaticResourceConverter_Preserved()
    {
        // Arrange — real pattern from atc-wpf CheckBox.xaml: two nested markup extensions
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:atc="http://schemas.atc-net.org/2023/wpf">
                <Rectangle RadiusX="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=(atc:CheckBoxHelper.CheckCornerRadius), Mode=OneWay, Converter={StaticResource CornerRadiusTopLeftConverter}}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("RelativeSource={RelativeSource TemplatedParent}");
        result.Should().Contain("Converter={StaticResource CornerRadiusTopLeftConverter}");
        result.Should().NotContainAny("Atc.XamlToolkit", "XamlStyler");
    }

    [Fact]
    public void StyleDocument_NestedXStaticConverter_Preserved()
    {
        // Arrange — real pattern from atc-wpf ClipboardServiceView.xaml
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:atcValueConverters="clr-namespace:Atc.Wpf.ValueConverters;assembly=Atc.Wpf">
                <Image Visibility="{Binding PastedImage, Converter={x:Static atcValueConverters:ObjectNotNullToVisibilityVisibleValueConverter.Instance}}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("Converter={x:Static atcValueConverters:ObjectNotNullToVisibilityVisibleValueConverter.Instance}");
    }

    [Fact]
    public void StyleDocument_QuotedStringFormat_Preserved()
    {
        // Arrange — real pattern from atc-wpf ClipboardServiceView.xaml
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock Text="{Binding StatusText, StringFormat='Status: {0}'}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("StringFormat='Status: {0}'");
    }

    [Fact]
    public void StyleDocument_QuotedStringFormatWithEscapedBraces_Preserved()
    {
        // Arrange — real pattern from atc-wpf ClipboardServiceView.xaml
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                <TextBlock Text="{Binding Timestamp, StringFormat='{}{0:HH:mm:ss}', Mode=OneWay}" />
            </Window>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert
        result.Should().Contain("StringFormat='{}{0:HH:mm:ss}'");
        result.Should().Contain("Mode=OneWay");
    }

    [Fact]
    public void StyleDocument_DesignTimeReferences_DefaultPreserved()
    {
        // Arrange — real pattern from nearly every UserControl in atc-wpf
        var xaml = """
            <UserControl xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                         xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                         d:DesignHeight="900"
                         d:DesignWidth="900"
                         mc:Ignorable="d">
                <Grid />
            </UserControl>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert — all design-time attributes and namespaces preserved by default
        result.Should().Contain("xmlns:d=");
        result.Should().Contain("xmlns:mc=");
        result.Should().Contain("d:DesignHeight=\"900\"");
        result.Should().Contain("d:DesignWidth=\"900\"");
        result.Should().Contain("mc:Ignorable=\"d\"");
    }

    [Fact]
    public void StyleDocument_AlreadyFormattedThemeXaml_Idempotent()
    {
        // Arrange — real-world snippet resembling atc-wpf CheckBox.xaml ResourceDictionary
        var xaml = """
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:atc="http://schemas.atc-net.org/2023/wpf">
                <ControlTemplate x:Key="CheckBoxTemplate" TargetType="{x:Type CheckBox}">
                    <Grid>
                        <Border
                            Background="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=(atc:CheckBoxHelper.CheckBackgroundBrush)}"
                            BorderBrush="{Binding RelativeSource={RelativeSource TemplatedParent}, Path=(atc:CheckBoxHelper.CheckBorderBrush)}"
                            BorderThickness="1"
                            CornerRadius="2" />
                        <ContentPresenter
                            Margin="{TemplateBinding Padding}"
                            HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                            VerticalAlignment="{TemplateBinding VerticalContentAlignment}"
                            RecognizesAccessKey="True" />
                    </Grid>
                </ControlTemplate>
            </ResourceDictionary>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act — format once, then format again
        var firstPass = service.StyleDocument(xaml);
        var secondPass = service.StyleDocument(firstPass);

        // Assert — idempotent: second format produces identical output
        secondPass.Should().Be(firstPass);
    }

    [Fact]
    public void StyleFile_NonBomFile_NoBomAdded()
    {
        // Arrange — write XAML file without BOM
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Title="Test" Height="450" Width="800">
                <Grid />
            </Window>
            """;
        var filePath = Path.Combine(tempDir, "no_bom.xaml");
        File.WriteAllBytes(filePath, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(xaml));

        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        service.StyleFile(filePath);

        // Assert — file must not start with BOM bytes (0xEF 0xBB 0xBF)
        var bytes = File.ReadAllBytes(filePath);
        bytes.Should().HaveCountGreaterThan(3);
        var hasBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        hasBom.Should().BeFalse("file was written without BOM and should stay without BOM");
    }

    [Fact]
    public void StyleFile_BomFile_BomPreserved()
    {
        // Arrange — write XAML file with BOM
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Title="Test" Height="450" Width="800">
                <Grid />
            </Window>
            """;
        var filePath = Path.Combine(tempDir, "with_bom.xaml");
        var encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bom = encoding.GetPreamble();
        var content = encoding.GetBytes(xaml);
        var bytesWithBom = new byte[bom.Length + content.Length];
        bom.CopyTo(bytesWithBom, 0);
        content.CopyTo(bytesWithBom, bom.Length);
        File.WriteAllBytes(filePath, bytesWithBom);

        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        service.StyleFile(filePath);

        // Assert — file must still start with BOM bytes
        var bytes = File.ReadAllBytes(filePath);
        bytes.Should().HaveCountGreaterThan(3);
        var hasBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        hasBom.Should().BeTrue("file was written with BOM and should keep its BOM");
    }

    [Fact]
    public void StyleDocument_DoubleBraceTemplateTokens_Preserved()
    {
        // Arrange — real pattern from atc-wpf Theme.Template.xaml
        // {{placeholder}} are template substitution tokens, not markup extensions
        var xaml = """
            <ResourceDictionary
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:options="http://schemas.microsoft.com/winfx/2006/xaml/presentation/options">
                <SolidColorBrush
                    x:Key="AtcApps.Brushes.SystemControlHighlightAltListAccentHigh"
                    options:Freeze="True"
                    Opacity="{{AtcApps.Brushes.SystemControlHighlightAltListAccentHigh.Opacity}}"
                    Color="{StaticResource AtcApps.Colors.SystemAccent}" />
                <SolidColorBrush
                    x:Key="AtcApps.Brushes.SemiTransparent"
                    options:Freeze="True"
                    Color="{{AtcApps.Colors.SemiTransparent}}" />
            </ResourceDictionary>
            """;
        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        var result = service.StyleDocument(xaml);

        // Assert — double-brace tokens must be preserved exactly
        result.Should().Contain("Opacity=\"{{AtcApps.Brushes.SystemControlHighlightAltListAccentHigh.Opacity}}\"");
        result.Should().Contain("Color=\"{{AtcApps.Colors.SemiTransparent}}\"");
    }

    [Fact]
    public void StyleFile_ForceUtf8Bom_AddsBomToNonBomFile()
    {
        // Arrange — write XAML file without BOM
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Title="Test" Height="450" Width="800">
                <Grid />
            </Window>
            """;
        var filePath = Path.Combine(tempDir, "no_bom_force.xaml");
        File.WriteAllBytes(filePath, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(xaml));

        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        service.StyleFile(filePath, forceUtf8Bom: true);

        // Assert — file must now start with BOM bytes (0xEF 0xBB 0xBF)
        var bytes = File.ReadAllBytes(filePath);
        bytes.Should().HaveCountGreaterThan(3);
        var hasBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        hasBom.Should().BeTrue("forceUtf8Bom should add BOM to a non-BOM file");
    }

    [Fact]
    public void StyleFile_ForceUtf8Bom_PreservesBomOnBomFile()
    {
        // Arrange — write XAML file with BOM
        var xaml = """
            <Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Title="Test" Height="450" Width="800">
                <Grid />
            </Window>
            """;
        var filePath = Path.Combine(tempDir, "with_bom_force.xaml");
        var encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bom = encoding.GetPreamble();
        var content = encoding.GetBytes(xaml);
        var bytesWithBom = new byte[bom.Length + content.Length];
        bom.CopyTo(bytesWithBom, 0);
        content.CopyTo(bytesWithBom, bom.Length);
        File.WriteAllBytes(filePath, bytesWithBom);

        var options = new XamlStylerOptions();
        var service = new XamlStylerService(options);

        // Act
        service.StyleFile(filePath, forceUtf8Bom: true);

        // Assert — file must still start with BOM bytes
        var bytes = File.ReadAllBytes(filePath);
        bytes.Should().HaveCountGreaterThan(3);
        var hasBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        hasBom.Should().BeTrue("forceUtf8Bom should preserve BOM on a file that already had BOM");
    }

    private string CreateTempFile(
        string name,
        string content)
    {
        var path = Path.Combine(tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }
}