namespace Snobol4.Common;

public class NameFormattingStrategy : IFormattingStrategy
{

    public string ToString(Var self)
    {
        var nameSelf = (NameVar)self;
        return nameSelf.TargetCollection is null && nameSelf.HasText ? nameSelf.Pointer : "NAME";
    }

    public string DumpString(Var self)
    {
        var nameSelf = (NameVar)self;

        if (nameSelf.TargetCollection is not null && nameSelf.TargetKey is not null)
        {
            // Format as collection reference
            var keyStr = nameSelf.TargetKey switch
            {
                long l => l.ToString(),
                double d => d.ToString(),
                string s => $"'{s}'",
                _ => nameSelf.TargetKey.ToString()
            };
            return $".{nameSelf.TargetCollection.Symbol}[{keyStr}]";
        }

        return $".{nameSelf.Pointer}";
    }

    public string DebugVar(Var self)
    {
        var nameSelf = (NameVar)self;
        var symbol = string.IsNullOrEmpty(nameSelf.Symbol) ? "<no name>" : nameSelf.Symbol;

        if (nameSelf.TargetCollection is not null && nameSelf.TargetKey is not null)
        {
            return $"NAME Symbol: {symbol}  Collection: {nameSelf.TargetCollection.Symbol}  Key: {nameSelf.TargetKey}  Succeeded: {nameSelf.Succeeded}";
        }

        return $"NAME Symbol: {symbol}  Pointer: '{nameSelf.Pointer}'  Succeeded: {nameSelf.Succeeded}";
    }
}