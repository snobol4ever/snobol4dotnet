namespace Snobol4.Common;

public abstract class Pattern
{
    internal Pattern? Left = null; // Left child of pattern
    internal Pattern? Right = null; // Right child of pattern
    internal List<AbstractSyntaxTreeNode> Ast = [];
    internal AbstractSyntaxTreeNode? StartNode;

    internal virtual bool IsTerminal()
    {
        return true;
    }

    internal abstract Pattern Clone();
}