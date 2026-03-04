namespace Snobol4.Common;

internal class AbstractSyntaxTree
{
    private readonly List<AbstractSyntaxTreeNode> _nodes = [];
    private AbstractSyntaxTreeNode? _startNode;

    public AbstractSyntaxTreeNode StartNode => _startNode 
        ?? throw new InvalidOperationException("AST not built");

    public int Count => _nodes.Count;

    public AbstractSyntaxTreeNode this[int index] => _nodes[index];

    public static AbstractSyntaxTree Build(Pattern rootPattern)
    {
        var ast = new AbstractSyntaxTree();
        ast.BuildFromPattern(rootPattern);
        return ast;
    }

    private void BuildFromPattern(Pattern rootPattern)
    {
        if (rootPattern.Ast.Count != 0)
        {
            // Pattern already has AST, reuse it
            _nodes.AddRange(rootPattern.Ast);
            _startNode = rootPattern.StartNode;
            return;
        }

        BuildNodeList(rootPattern);
        LinkParentChildren();
        ComputeSubsequentsAndAlternates();
        FindStartNode();

        // Cache in pattern
        rootPattern.Ast = _nodes;
        rootPattern.StartNode = _startNode;
    }

    private void BuildNodeList(Pattern rootPattern)
    {
        var nodeStack = new Stack<AbstractSyntaxTreeNode>();
        var currentIndex = 0;
        nodeStack.Push(new AbstractSyntaxTreeNode(rootPattern, 0, AbstractSyntaxTreeNode.NodeType.NONE, -1, _nodes));

        while (nodeStack.Count > 0)
        {
            var currentNode = nodeStack.Pop();
            var parentIndex = currentIndex;
            currentNode.SelfIndex = currentIndex++;
            _nodes.Add(currentNode);

            if (currentNode.IsTerminal())
                continue;

            nodeStack.Push(new AbstractSyntaxTreeNode(currentNode.Self.RightPattern!, -99, AbstractSyntaxTreeNode.NodeType.RIGHT, parentIndex, _nodes));
            nodeStack.Push(new AbstractSyntaxTreeNode(currentNode.Self.LeftPattern!, -99, AbstractSyntaxTreeNode.NodeType.LEFT, parentIndex, _nodes));
        }
    }

    private void LinkParentChildren()
    {
        for (var i = 1; i < _nodes.Count; i++)
        {
            var currentNode = _nodes[i];
            var parent = _nodes[currentNode.ParentIndex];

            switch (currentNode.ChildType)
            {
                case AbstractSyntaxTreeNode.NodeType.LEFT:
                    parent.LeftChild = i;
                    break;
                case AbstractSyntaxTreeNode.NodeType.RIGHT:
                    parent.RightChild = i;
                    break;
            }
        }
    }

    private void ComputeSubsequentsAndAlternates()
    {
        for (var i = 0; i < _nodes.Count; ++i)
        {
            if (!_nodes[i].IsTerminal())
                continue;
            _nodes[i].Subsequent = ComputeSubsequent(i);
            _nodes[i].Alternate = ComputeAlternate(i);
        }
    }

    private void FindStartNode()
    {
        var node = _nodes[0];
        while (!node.IsTerminal())
            node = node.GetLeftChild()!;
        _startNode = node;
    }

    private int ComputeSubsequent(int index) => ComputeNext(index, true);

    private int ComputeAlternate(int index) => ComputeNext(index, false);

    private int ComputeNext(int index, bool concatenate)
    {
        if (_nodes.Count == 1)
            return -1;

        var currentIndex = index;

        while (true)
        {
            var currentNode = _nodes[currentIndex];
            var parentNode = _nodes[currentNode.ParentIndex];

            if (currentNode.IsLeftChild() && 
                (concatenate ? parentNode.Self is ConcatenatePattern : parentNode.Self is AlternatePattern))
                break;

            if (parentNode.SelfIndex == 0)
                return -1;

            currentIndex = currentNode.ParentIndex;
        }

        var parentNode2 = _nodes[_nodes[currentIndex].ParentIndex];
        var currentNode2 = _nodes[parentNode2.RightChild];

        while (!currentNode2.IsTerminal())
            currentNode2 = currentNode2.GetLeftChild()!;

        return currentNode2.SelfIndex;
    }

    public void Dump()
    {
        Console.Error.WriteLine(@"=============================================================");
        Console.Error.WriteLine(@"Root:");
        if (_startNode is not null)
            Console.Error.WriteLine(_startNode.DebugAst());
        Console.Error.WriteLine(@"-------------------------------------------------------------");
        foreach (var node in _nodes)
        {
            Console.Error.WriteLine(node.DebugAst());
        }
        Console.Error.WriteLine(@"=============================================================");
    }
}