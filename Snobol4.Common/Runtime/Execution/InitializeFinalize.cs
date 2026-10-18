namespace Snobol4.Common;

public partial class Executive
{
    // ReSharper disable once UnusedMember.Global
    public void InitializeStatement(int lineNumber)
    {
        using var profiler1 = Profiler.Start3("InitializeStatement", this);


        if (Parent.BuildOptions.TraceStatements)
            Console.Error.WriteLine($"""

                               InitializeStatement {lineNumber}

                               """);

        BeginStatement(lineNumber);
        Failure = false;
        AlphaStack.Clear(); // Used for conditional variable association
        BetaStack.Clear();  // Used for conditional variable association
        SystemStack.Push(new StatementSeparator());

        // Monitor bridge — LABEL event (SN-26-bridge-coverage-f).
        // Use precomputed Execute.SourceStno — Parent.Code is replaced by
        // EVAL/CODE at runtime; a BlankLineCount lookup against the
        // swapped SourceLines silently returns 0 and emits a wrong stno.
        // See GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan, session #58.
        {
            long stno = (lineNumber >= 0 && lineNumber < SourceStno.Count)
                ? SourceStno[lineNumber]
                : (lineNumber + 1);
            MonitorIpc.EmitLabel(stno);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public void FinalizeStatement()
    {
        using var profiler1 = Profiler.Start3($"FinalizeStatement", this);

        if (Parent.BuildOptions.TraceStatements)
            Console.Error.WriteLine("""

                              FinalizeStatement

                              """);

        while (SystemStack.Peek() is not StatementSeparator)
            SystemStack.Pop();

        SystemStack.Pop();
    }

    // A statement begins: the one that began before it becomes &LASTNO, as in SPITBOL, where a
    // function body's first statement reads the calling statement as its last
    internal void BeginStatement(int index)
    {
        AmpLastLineNumber = _statementBegun ? AmpCurrentLineNumber : -1;
        AmpCurrentLineNumber = index;
        _statementBegun = true;
    }

    private bool _statementBegun;
}