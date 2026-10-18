namespace Snobol4.Common;

public partial class Executive
{
    private bool _inExecuteLoop;

    public int ExecuteLoop(int i)
    {
        // Threaded path (default): delegate to ThreadedExecuteLoop.
        if (Thread != null)
        {
            var starts = Parent.StatementInstructionStarts;
            int startInstr = (i >= 0 && starts != null && i < starts.Length)
                ? starts[i]
                : 0;
            InitExecutionCache();   // no-op after first call
            int result;
            // A CompilerException from LogRuntimeException is fatal to the whole
            // program, so only the outermost ExecuteLoop catches it and halts.
            // ExecuteLoop also runs function bodies, CODE gotos and SETEXIT
            // handlers; catching in one of those returned -1 to its caller, which
            // resumed after the error (an error inside a function looped or
            // overflowed the stack).
            if (_inExecuteLoop)
            {
                result = ThreadedExecuteLoop(startInstr);
            }
            else
            {
                _inExecuteLoop = true;
                try
                {
                    result = ThreadedExecuteLoop(startInstr);
                }
                catch (CompilerException)
                {
                    // Error already recorded in ErrorCodeHistory by LogRuntimeException.
                    // Halt gracefully rather than propagating through the test harness.
                    return -1;
                }
                finally
                {
                    _inExecuteLoop = false;
                }
            }
            // Non-negative: threaded loop exited to a CODE'd Statements[] index — run it.
            if (result < 0) return result;
            i = result;
        }

        if (LabelTable[Parent.FoldCase(Parent.EntryLabel)] != GotoNotFound)
            i = LabelTable[Parent.FoldCase(Parent.EntryLabel)];

        var failure = OnErrorGoto > 0;
        OnErrorGoto = 0;

        while (i >= 0)
        {
            using var profiler1 = Profiler.Start1($"Statement{AmpCurrentLineNumber:000000}", this);

            if (Parent.BuildOptions.TraceStatements)
                Console.Error.WriteLine(@$"{i} {SourceCode[i]}");

            if (AmpStatementLimit >= 0)
                AmpStatementCount++;

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

    /// <summary>
    /// Executes a standalone threaded sub-program (compiled star function).
    ///
    /// Root cause of the star-function side-effect bug (S-9 / S-8B):
    ///   Setting Thread = subThread before calling ThreadedExecuteLoop caused
    ///   ExecuteProgramDefinedFunction → ExecuteLoop → ThreadedExecuteLoop to
    ///   capture Thread = subThread and run the tiny sub-expression Instruction[]
    ///   from a statement-index IP that is valid only in the main program thread,
    ///   re-entering the main program from its beginning and never executing the
    ///   user-defined function body.
    ///
    /// Fix: leave Thread pointing at the main program thread throughout.
    ///   Pass subThread as overrideThread to ThreadedExecuteLoop so it uses the
    ///   sub-expression instructions for its own loop — while any nested
    ///   ExecuteLoop / ThreadedExecuteLoop calls (for user-defined functions) still
    ///   see Thread = main thread and execute correctly via StatementInstructionStarts.
    ///
    ///   The entry-label override (which would redirect IP=0 into the main program)
    ///   is suppressed when overrideThread != null inside ThreadedExecuteLoop.
    ///
    ///   Failure is cleared on entry so Function() does not bail out when the outer
    ///   pattern-match context has Failure=true.
    /// </summary>
    internal void RunExpressionThread(Instruction[] subThread)
    {
        var savedIP    = InstructionPointer;
        Failure        = false;   // sub-expression always starts clean
        InitExecutionCache();     // no-op after first call
        ThreadedExecuteLoop(0, useFastPath: false, overrideThread: subThread);
        var exprFailure = LastExpressionFailure;
        InstructionPointer = savedIP;
        Failure            = exprFailure;
    }
}
