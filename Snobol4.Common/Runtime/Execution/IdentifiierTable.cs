using Snobol4.Common;

public class IdentifierTable(Executive exec) : Dictionary<string, Var>
{
    public Executive Exec { get; } = exec;

    public new Var this[string symbol]
    {
        get
        {
            if (!TryGetValue(symbol, out var value))
            {
                base[symbol] = value = StringVar.Null(symbol);
            }

            Exec.TraceIdentifierAccess(symbol);
            return value;
        }
        set
        {
            value.Symbol = symbol;
            base[value.Symbol] = value;
            Exec.SyncVarSlot(symbol, value);   // keep VarSlotArray in sync
        }
    }

    public Var GetValueSafe(string symbol)
    {
        return base[symbol];
    }
}