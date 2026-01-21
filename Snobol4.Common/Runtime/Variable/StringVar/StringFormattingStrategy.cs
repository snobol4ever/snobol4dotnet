using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for string variables
/// </summary>
public class StringFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        var stringSelf = (StringVar)self;
        return stringSelf.Data;
    }


    public string DumpString(Var self)
    {
        var stringSelf = (StringVar)self;
        return $"'{stringSelf.Data}'";
    }

    public string DebugVar(Var self)
    {
        var stringSelf = (StringVar)self;
        var symbol = stringSelf.Symbol.Length == 0 ? "<no name>" : stringSelf.Symbol;
        var data = stringSelf.Data.Length == 0 ? "<no data>" : stringSelf.Data;
        return $"STRING Symbol: {symbol}  Data: '{data}'  Succeeded: {stringSelf.Succeeded}  Input channel: '{stringSelf.InputChannel}'  Output channel: '{stringSelf.OutputChannel}'";
    }
}