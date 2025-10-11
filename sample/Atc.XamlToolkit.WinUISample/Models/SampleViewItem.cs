namespace Atc.XamlToolkit.WinUISample.Models;

public class SampleViewItem
{
    public SampleViewItem(
        string name,
        string path)
    {
        Name = name;
        Path = path;
        Children = [];
    }

    public SampleViewItem(
        string name,
        Type viewType)
    {
        Name = name;
        ViewType = viewType;
        Children = [];
    }

    public string Name { get; }

    public string? Path { get; }

    public Type? ViewType { get; }

    public IList<SampleViewItem> Children { get; }

    public bool IsFolder => ViewType is null;
}