namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class SetterAccessViewModel : Atc.XamlToolkit.Mvvm.ObservableObject
{
    [Atc.XamlToolkit.Mvvm.ObservableProperty(SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Internal)]
    private string name = string.Empty;

    [Atc.XamlToolkit.Mvvm.ObservableProperty(SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.Private)]
    private int count;

    [Atc.XamlToolkit.Mvvm.ObservableProperty]
    private bool isActive;

    public void Increase()
        => Count++;
}