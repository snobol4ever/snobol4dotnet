namespace Snobol4.Common;

public sealed class StatementSeparatorFormattingStrategy : IFormattingStrategy
{
    private const string _toStringValue = "<statement-separator>";
    private const string _dumpStringValue = "───";
    private const string _debugStringValue = "STATEMENT SEPARATOR";


    public string ToString(Var self)
    {
        return _toStringValue;
    }


    public string DumpString(Var self)
    {
        return _dumpStringValue;
    }


    public string DebugVar(Var self)
    {
        return _debugStringValue;
    }
}