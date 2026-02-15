using Snobol4.Common;


public class LabelTable(Executive exec) : Dictionary<string, int>
{
    public Executive Exec { get; } = exec;

    public new int this[string symbol]
    {
        get
        {
            return TryGetValue(symbol, out var entry) ? entry : Executive.GotoNotFound;
        }

        set
        {
            base[symbol] = value;
        }
    }
}