namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.Mvvm.GeneralAttributes.Azure;

public enum AppEnvironmentType
{
    [Description("Local")]
    Local,

    [Description("Development")]
    Dev,

    [Description("UAT")]
    Uat,

    [Description("Production")]
    Prod,
}