namespace Snobol4.Common;

public partial class Executive
{
    public int ExecuteLoop(int i)
    {
        var starts = Parent.StatementInstructionStarts;
        int startInstr = (i >= 0 && starts != null && i < starts.Length)
            ? starts[i]
            : 0;
        return ThreadedExecuteLoop(startInstr);
    }

    // ReSharper disable once UnusedMember.Global
    public static void BreakPoint()
    {
    }

    /// <summary>
    /// Executes a standalone threaded sub-program (compiled star function).
    /// </summary>
    internal void RunExpressionThread(Instruction[] subThread)
    {
        var savedIP        = InstructionPointer;
        var savedThread    = Thread;
        Thread             = subThread;
        InstructionPointer = 0;
        ThreadedExecuteLoop(0);
        var exprFailure    = LastExpressionFailure;
        Thread             = savedThread;
        InstructionPointer = savedIP;
        Failure            = exprFailure;
    }

    // =========================================================================
    // DEAD CODE — Roslyn Statements[] execution loop, kept for reference only.
    // =========================================================================
#pragma warning disable CS0162
    private int _Dead_ExecuteLoop_Roslyn(int i)
    {
        throw new NotImplementedException("Roslyn path removed");

        if (LabelTable[Parent.FoldCase(Parent.EntryLabel)] != GotoNotFound)
            i = LabelTable[Parent.FoldCase(Parent.EntryLabel)];

        var failure = ErrorJump > 0;
        ErrorJump = 0;

        while (i >= 0)
        {
            using var profiler1 = Profiler.Start1($"Statement{AmpCurrentLineNumber:000000}", this);
            if (Parent.BuildOptions.TraceStatements)
                Console.Error.WriteLine(@$"{i} {SourceCode[i]}");
            if (AmpStatementLimit >= 0) AmpStatementCount++;
            i = Statements[i](this);
            if (AmpStatementLimit <= 0) continue;
            if (AmpStatementCount < AmpStatementLimit) continue;
            LogRuntimeException(244);
            Failure = true;
            break;
        }

        Failure = failure;
        return i;
    }
#pragma warning restore CS0162
    // =========================================================================
}
