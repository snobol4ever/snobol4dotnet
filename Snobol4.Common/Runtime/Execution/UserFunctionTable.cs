using Snobol4.Common;

public class UserFunctionTable(Executive exec) : Dictionary<string, UserFunctionTableEntry>
{
    public Executive Exec { get; } = exec;

    public new UserFunctionTableEntry? this[string symbol]
    {
        get
        {
            return TryGetValue(symbol, out UserFunctionTableEntry? entry) ? entry : null;
        }

        set
        {
            base[symbol] = value;
        }
    }
}