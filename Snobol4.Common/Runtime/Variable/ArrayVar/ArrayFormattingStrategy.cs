namespace Snobol4.Common;

public class ArrayFormattingStrategy : IFormattingStrategy
{
    private const string _arrayTypeName = "array";
    private const string _anonymousSymbol = "<anonymous>";


    public string ToString(Var self)
    {
        // Simple representation for general use
        return _arrayTypeName;
    }


    public string DumpString(Var self)
    {
        // Detailed representation showing prototype
        var arraySelf = (ArrayVar)self;
        return $"{_arrayTypeName}({arraySelf.Prototype})";
    }

    public string DebugVar(Var self)
    {
        // Comprehensive debug information
        var arraySelf = (ArrayVar)self;
        var symbol = string.IsNullOrEmpty(arraySelf.Symbol) ? _anonymousSymbol : arraySelf.Symbol;
        var fill = arraySelf.Fill.DumpString();
        var dataCount = arraySelf.Data.Count;
        
        return $"ARRAY Symbol: {symbol}  Prototype: {arraySelf.Prototype}  " +
               $"Dimensions: {arraySelf.Dimensions}  TotalSize: {arraySelf.TotalSize}  " +
               $"Fill: {fill}  " +
               $"Elements: {dataCount}  Succeeded: {arraySelf.Succeeded}";
    }
}