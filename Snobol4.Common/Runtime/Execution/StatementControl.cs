namespace Snobol4.Common;

public partial class Executive
{
    public int ExecuteLoop(int i)
    {
        // Threaded path: delegate to ThreadedExecuteLoop starting at statement i
        if (Thread != null)
        {
            var starts = Parent.StatementInstructionStarts;
            int startInstr = (i >= 0 && starts != null && i < starts.Length)
                ? starts[i]
                : 0;
            int result = ThreadedExecuteLoop(startInstr);
            // Non-negative: threaded loop exited to a CODE'd Statements[] index — run it.
            if (result < 0) return result;
            i = result;
            // fall through to Roslyn Statements[] loop below
        }

        if (LabelTable[Parent.FoldCase(Parent.EntryLabel)] != GotoNotFound)
            i = LabelTable[Parent.FoldCase(Parent.EntryLabel)];

        var failure = ErrorJump > 0;
        ErrorJump = 0;

        while (i >= 0)
        {
            using var profiler1 = Profiler.Start1($"Statement{AmpCurrentLineNumber:000000}", this);

            if (Parent.BuildOptions.TraceStatements)
                Console.Error.WriteLine(@$"{i} {SourceCode[i]}");

            if (AmpStatementLimit >= 0)
                AmpStatementCount++;

            //Console.WriteLine(SourceCode[i]);
            //Console.Write($"<{i}>");
            i = Statements[i](this);

            if (AmpStatementLimit <= 0)
                continue;

            if (AmpStatementCount < AmpStatementLimit)
                continue;

            LogRuntimeException(244);
            Failure = true;
            break;
        }

        Failure = failure;
        return i;
    }

    // ReSharper disable once UnusedMember.Global
    public static void BreakPoint()
    {
    }
}