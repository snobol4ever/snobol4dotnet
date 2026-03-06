namespace Snobol4.Common;

public partial class Executive
{
    public int ExecuteLoop(int i)
    {
        var starts = Parent.StatementInstructionStarts;
        int startInstr = (i >= 0 && starts != null && i < starts.Length)
            ? starts[i]
            : 0;
        InitExecutionCache();   // no-op after first call
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
        InitExecutionCache();   // no-op after first call
        ThreadedExecuteLoop(0);
        var exprFailure    = LastExpressionFailure;
        Thread             = savedThread;
        InstructionPointer = savedIP;
        Failure            = exprFailure;
    }
}
