using System.Runtime.ExceptionServices;

namespace Snobol4.Common;

public partial class Executive
{
    private bool _inExecuteLoop;

    // How many expressions (EVAL, deferred expressions of patterns) are being evaluated, the functions they call included
    internal int ExpressionDepth;

    // The function level the innermost evaluated expression runs at: an error at that level is in the expression's own code
    internal long ExpressionFunctionLevel = -1;

    // The program runs on a thread with a stack large enough for the depth SPITBOL's stack allows (-s bytes of 72-byte frames
    // at the least) at the .NET frames each level takes; the thread that called goes on afterwards with the program's state
    private int OnLargeStack(Func<int> run)
    {
        var blockCounter = BlockCounterValue;
        var result = 0;
        ExceptionDispatchInfo? failure = null;
        void Body()
        {
            BlockCounterValue = blockCounter;
            try
            {
                result = run();
            }
            catch (Exception e)
            {
                failure = ExceptionDispatchInfo.Capture(e);
            }
            finally
            {
                blockCounter = BlockCounterValue;
            }
        }

        // a system that will not reserve so much (a limit on address space) gets the largest stack it will, and the run goes on
        // this thread's own stack when it will give none
        var stackBytes = (int)Math.Clamp(Parent.BuildOptions.StackSizeBytes / 72L * LargeStackBytesPerLevel, 256L << 20, int.MaxValue);
        var started = false;
        for (; stackBytes >= 16 << 20 && !started; stackBytes /= 4)
        {
            var thread = new Thread(Body, stackBytes);
            try
            {
                thread.Start();
            }
            catch (Exception e) when (e is OutOfMemoryException or ThreadStartException)
            {
                continue;
            }

            started = true;
            thread.Join();
        }

        if (!started)
            Body();
        BlockCounterValue = blockCounter;
        failure?.Throw();
        return result;
    }

    // What one SPITBOL level of recursion may take of the .NET stack: measured at about 1.8 KB for a plain call, and more where a
    // pattern match or a deep expression sits between the levels
    private const int LargeStackBytesPerLevel = 8 * 1024;

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
                    result = OnLargeStack(() => ThreadedExecuteLoop(startInstr));
                }
                catch (CompilerException)
                {
                    // Error already recorded in ErrorCodeHistory by LogRuntimeException.
                    // Halt gracefully rather than propagating through the test harness.
                    return -1;
                }
                catch (ProgramEndException)
                {
                    // a goto to END inside a function body ends the program
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

            var limitPassed = StatementLimitReached();
            if (limitPassed == StatementLimit.PassedForGood)
            {
                LogRuntimeException(244, fatal: true);
                Failure = true;
                break;
            }

            if (limitPassed == StatementLimit.Passed)
                LogRuntimeException(244);

            if (AmpStatementLimit >= 0)
                AmpStatementCount++;

            i = Statements[i](this);
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
        var savedExpressionLevel = ExpressionFunctionLevel;
        ExpressionFunctionLevel = AmpFunctionLevel;
        ExpressionDepth++;
        try
        {
            ThreadedExecuteLoop(0, useFastPath: false, overrideThread: subThread);
        }
        finally
        {
            ExpressionDepth--;
            ExpressionFunctionLevel = savedExpressionLevel;
        }

        var exprFailure = LastExpressionFailure;
        InstructionPointer = savedIP;
        Failure            = exprFailure;
        // the evaluation that an error ended fails as an ordinary failure of its own to the code that asked for it, so
        // (EVAL('1 / N'), 'f') is f, unless a SETEXIT handler waits for the error
        if (exprFailure && OnErrorGoto == 0)
            FailureIsError = false;
    }
}

/// <summary>A goto to END from inside a nested run (a function body): unwinds to the outermost loop.</summary>
internal sealed class ProgramEndException : Exception;
