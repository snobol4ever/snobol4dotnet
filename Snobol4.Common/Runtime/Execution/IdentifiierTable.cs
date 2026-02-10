using Snobol4.Common;
using System.Globalization;

public class IdentifierTable(Executive exec) : Dictionary<string, Var>
{
    public Executive Exec { get; } = exec;

    public new Var this[string symbol]
    {
        get
        {
            if (Exec.Parent.CaseFolding)
            {
                symbol = symbol.ToUpper(CultureInfo.CurrentCulture);
            }

            if (!TryGetValue(symbol, out Var value))
            {
                value = StringVar.Null(symbol);
                base[symbol] = value;
            }

            Exec.TraceIdentifierAccess(symbol);
            Exec.TraceIdentifierValue(symbol);
            return value;
        }
        set
        {
            if (Exec.Parent.CaseFolding)
            {
                symbol = symbol.ToUpper(CultureInfo.CurrentCulture);
            }

            value.Symbol = symbol;
            base[value.Symbol] = value;
            Exec.TraceIdentifierValue(symbol);
        }
    }

    public Var GetValueSafe(string symbol)
    {
        return base[symbol];
    }
}