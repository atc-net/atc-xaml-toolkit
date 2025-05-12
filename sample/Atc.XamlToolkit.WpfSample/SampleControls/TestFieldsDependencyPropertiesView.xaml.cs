namespace Atc.XamlToolkit.WpfSample.SampleControls;

public partial class TestFieldsDependencyPropertiesView
{
    public TestFieldsDependencyPropertiesView()
    {
        InitializeComponent();

        IsRunning = false;
    }

    [DependencyProperty(DefaultValue = true)]
    private bool isRunning;

    [DependencyProperty(DefaultValue = 1.1)]
    private decimal decimalValue;

    [DependencyProperty(DefaultValue = 1.1)]
    private float floatValue;

    [DependencyProperty(DefaultValue = 1)]
    private int intValue;

    [DependencyProperty]
    private LogItem logItem;

    [DependencyProperty(DefaultValue = LogCategoryType.Debug)]
    private LogCategoryType logCategory;

    [DependencyProperty(DefaultValue = "Hello world")]
    private string stringValue;

    [DependencyProperty(DefaultValue = "error;err:")]
    private IList<string> errorTerms;

    [DependencyProperty]
    private IList<string>? otherTerms;

    [DependencyProperty(DefaultValue = "Red")]
    private Color errorTextColor;

    [DependencyProperty(DefaultValue = "Red")]
    private Brush errorTextBrush;

    [DependencyProperty(DefaultValue = "Consolas")]
    private FontFamily myFontFamily;

    [DependencyProperty(DefaultValue = 12.2)]
    private double myFontSize;

    [DependencyProperty(DefaultValue = "Red")]
    private Brush? background;

    [DependencyProperty(DefaultValue = "double.NaN")]
    private double spacing;

    [DependencyProperty(DefaultValue = "decimal.MinValue")]
    private decimal minimum1;

    [DependencyProperty(DefaultValue = PropertyDefaultValueConstants.MinValue)]
    private decimal minimum2;

    [DependencyProperty(DefaultValue = double.MinValue)]
    private double minimum3Double;

    [DependencyProperty(DefaultValue = float.MinValue)]
    private float minimum3Float;

    [DependencyProperty(DefaultValue = "decimal.MaxValue")]
    private decimal maximum1;

    [DependencyProperty(DefaultValue = PropertyDefaultValueConstants.MaxValue)]
    private decimal maximum2;

    [DependencyProperty(DefaultValue = double.MaxValue)]
    private double maximum3Double;

    [DependencyProperty(DefaultValue = float.MaxValue)]
    private float maximum3Float;
}