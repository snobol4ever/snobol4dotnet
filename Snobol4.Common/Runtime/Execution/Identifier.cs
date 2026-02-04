namespace Snobol4.Common;

public partial class Executive
{
    public delegate string? ReadLine();

    public static ReadLine? ReadLineDelegate;

    public void Identifier(string symbol)
    {
        if (Parent.TraceStatements)
            Console.Error.WriteLine($@"Identifier {symbol}");

        // Identifiers cannot be null strings
        if (symbol == "")
        {
            LogRuntimeException(212);
            return;
        }

        // Look up variable associated with the symbol
        // If the identifier is not in the symbol table, add a new entry (StringVar with an empty string)
        var v = IdentifierTable[symbol];
        SystemStack.Push(v);
    }

    // ReSharper disable once UnusedMember.Global
    public void FunctionName(string name)
    {
        using var profiler = Profiler.Start($"FunctionName", ProfileStatements);
        SystemStack.Push(new StringVar(name));
    }
}