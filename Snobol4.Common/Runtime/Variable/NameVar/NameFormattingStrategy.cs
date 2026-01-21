using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Formatting strategy for name variables
/// </summary>
public class NameFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        var nameSelf = (NameVar)self;
        return nameSelf.Collection is null ? nameSelf.Pointer : "name";
    }

    public string DumpString(Var self)
    {
        var nameSelf = (NameVar)self;

        if (nameSelf.Collection is not null && nameSelf.Key is not null)
        {
            // Format as collection reference
            var keyStr = nameSelf.Key switch
            {
                long l => l.ToString(),
                double d => d.ToString(),
                string s => $"'{s}'",
                _ => nameSelf.Key.ToString()
            };
            return $".{nameSelf.Collection.Symbol}[{keyStr}]";
        }

        return $".{nameSelf.Pointer}";
    }

    public string DebugVar(Var self)
    {
        var nameSelf = (NameVar)self;
        var symbol = string.IsNullOrEmpty(nameSelf.Symbol) ? "<no name>" : nameSelf.Symbol;

        if (nameSelf.Collection is not null && nameSelf.Key is not null)
        {
            return $"NAME Symbol: {symbol}  Collection: {nameSelf.Collection.Symbol}  Key: {nameSelf.Key}  Succeeded: {nameSelf.Succeeded}";
        }

        return $"NAME Symbol: {symbol}  Pointer: '{nameSelf.Pointer}'  Succeeded: {nameSelf.Succeeded}";
    }
}