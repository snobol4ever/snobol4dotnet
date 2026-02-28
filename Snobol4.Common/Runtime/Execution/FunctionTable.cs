using Snobol4.Common;

public class FunctionTable(Executive exec) : Dictionary<string, FunctionTableEntry>
{
    public Executive Exec { get; } = exec;

    public new FunctionTableEntry? this[string symbol]
    {
        get => TryGetValue(symbol, out var entry) ? entry : null;
        set => base[value!.Symbol] = value;
    }
}