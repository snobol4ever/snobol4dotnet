namespace Snobol4.Common;

public partial class Executive
{
    // ReSharper disable once UnusedMember.Global
    public void InitializeStatement(int lineNumber)
    {
        using var profiler1 = Profiler.Start($"InitializeStatement", ProfileStatements);

        if (Parent.TraceStatements)
            Console.Error.WriteLine($"""

                               InitializeStatement {lineNumber}

                               """);
        ((StringVar)IdentifierTable["&file"]).Data = SourceFiles[lineNumber - 1];
        ((IntegerVar)IdentifierTable["&stno"]).Data = lineNumber;
        ((IntegerVar)IdentifierTable["&line"]).Data = SourceLineNumbers[lineNumber - 1] - 1;
        ((IntegerVar)IdentifierTable["&stcount"]).Data++;
        Failure = false;
        AlphaStack.Clear(); // Used for conditional variable association
        BetaStack.Clear();  // Used for conditional variable association
        SystemStack.Push(new StatementSeparator());
    }

    // ReSharper disable once UnusedMember.Global
    public void FinalizeStatement()
    {
        using var profiler1 = Profiler.Start($"FinalizeStatement", ProfileStatements);

        if (Parent.TraceStatements)
            Console.Error.WriteLine("""

                              FinalizeStatement

                              """);

        while (SystemStack.Peek() is not StatementSeparator)
            SystemStack.Pop();

        SystemStack.Pop();

        ((StringVar)IdentifierTable["&lastfile"]).Data = ((StringVar)IdentifierTable["&file"]).Data;
        ((IntegerVar)IdentifierTable["&lastno"]).Data = ((IntegerVar)IdentifierTable["&stno"]).Data;
        ((IntegerVar)IdentifierTable["&lastline"]).Data = ((IntegerVar)IdentifierTable["&line"]).Data;
    }
}