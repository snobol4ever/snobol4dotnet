namespace Snobol4.Common;

public class FunctionTableEntry
{
    #region Members

    public delegate void FunctionHandler(List<Var> list);

    internal FunctionHandler Handler;
    internal int ArgumentCount;
    internal bool IsProtected;
    internal Stack<List<Var>> StateStack;
    internal string Symbol;

    #endregion

    #region Constructors

    public FunctionTableEntry(Executive exec, string symbol, FunctionHandler handler, int argumentCount,  bool isProtected)    
    {
        Handler = handler;
        ArgumentCount = argumentCount;
        IsProtected = isProtected;
        StateStack = new Stack<List<Var>>();
        Symbol = symbol;
    }

    #endregion
}