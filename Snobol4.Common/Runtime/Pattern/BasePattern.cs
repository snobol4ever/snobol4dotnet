namespace Snobol4.Common;

public abstract class Pattern
{
                                    internal Pattern? LeftPattern = null;

                                    internal Pattern? RightPattern = null;

                                                            internal List<AbstractSyntaxTreeNode> Ast = [];

                                    internal AbstractSyntaxTreeNode? StartNode;

                                                                                                            internal virtual bool IsTerminal()
    {
        return true;
    }

                                                                                                                                                        internal abstract Pattern Clone();

    #region Debugging

                    public abstract string DebugPattern();

    #endregion
}