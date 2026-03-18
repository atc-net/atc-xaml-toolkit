namespace Atc.XamlToolkit.XamlStyler.Model;

internal sealed class ElementProcessContext
{
    private readonly Stack<ElementProcessStatus> stack;

    public int Count => stack.Count;

    public ElementProcessStatus Current => stack.Peek();

    public ElementProcessContext()
    {
        stack = new Stack<ElementProcessStatus>();
        stack.Push(new ElementProcessStatus());
    }

    public void Push(ElementProcessStatus status)
    {
        stack.Push(status);
    }

    public ElementProcessStatus Pop() =>
        stack.Pop();

    public void UpdateParentElementProcessStatus(ContentTypes contentType)
    {
        var parent = stack.Peek();
        parent.ContentType |= contentType;
    }
}