namespace Snobol4.Common;

internal class AbstractSyntaxTree
{
    private readonly List<AbstractSyntaxTreeNode> _nodes = [];
    private AbstractSyntaxTreeNode? _startNode;

    // The start node of each pattern entered by *X or ARBNO during this match, by the pattern's identity: its nodes are
    // built once and every activation of it runs over the same nodes, as SPITBOL runs one pattern structure (see Enter)
    private Dictionary<Pattern, int>? _entries;

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
        // Always rebuild fresh: do NOT reuse cached AST nodes.
        // Cached AbstractSyntaxTreeNode objects hold a _tree reference to the list
        // they were created in. If reused via AddRange into a new list (e.g. inside
        // UnevaluatedPattern's child Scanner), GetSubsequent/GetAlternate follow
        // _tree[index] back into the original list — not the new copy — causing the
        // child scanner to jump to wrong nodes or fail entirely.
        // Rebuilding is slightly more work but is correct and re-entrant safe.
        BuildNodeList(rootPattern);
        LinkParentChildren();
        ComputeSubsequentsAndAlternates();
        FindStartNode();
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

            // A node after the first of a concatenation starts no enclosing alternative. The
            // sequence's first node saved that alternative at the cursor the alternation began
            // at; saving it again here would retry it from wherever this arm had got to.
            if (!concatenate && currentNode.IsRightChild() && parentNode.Self is ConcatenatePattern)
                return -1;

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

    /// <summary>
    /// The start node of <paramref name="subPattern"/>'s nodes in this AST, appending them the first time the pattern is
    /// entered during the match.  A node whose Subsequent is -1 ends the pattern: the scanner returns from it to the
    /// node its caller's frame names (Scanner.Call), so the nodes need no successor of their own and one copy serves
    /// every activation, at any depth of recursion, as SPITBOL's one pattern structure does.  (Each entry was a fresh
    /// copy wired to its own successor: the copies grew with the work, 2 GB for a 32 KB calculator input.)
    /// </summary>
    internal int Enter(Pattern subPattern)
    {
        _entries ??= new Dictionary<Pattern, int>(ReferenceEqualityComparer.Instance);
        if (_entries.TryGetValue(subPattern, out var known))
            return known;

        var subAst = new AbstractSyntaxTree();
        subAst.BuildNodeList(subPattern);
        subAst.LinkParentChildren();
        subAst.ComputeSubsequentsAndAlternates();
        subAst.FindStartNode();

        var offset = _nodes.Count;
        foreach (var n in subAst._nodes)
        {
            _nodes.Add(new AbstractSyntaxTreeNode(n.Self, n.SelfIndex + offset, n.ChildType,
                n.ParentIndex >= 0 ? n.ParentIndex + offset : n.ParentIndex, _nodes)
            {
                LeftChild  = n.LeftChild  >= 0 ? n.LeftChild  + offset : n.LeftChild,
                RightChild = n.RightChild >= 0 ? n.RightChild + offset : n.RightChild,
                Subsequent = n.Subsequent >= 0 ? n.Subsequent + offset : -1,
                Alternate  = n.Alternate  >= 0 ? n.Alternate  + offset : -1
            });
        }

        var start = subAst.StartNode.SelfIndex + offset;
        _entries[subPattern] = start;
        return start;
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