namespace Snobol4.Common;

/// <summary>
/// Threaded execution engine for SNOBOL4.NET.
/// Accepts a starting instruction index and returns an exit code matching
/// ExecuteLoop's contract: -1 = normal end, -2 = RETURN, -3 = FRETURN, -4 = NRETURN.
/// Safe for recursive calls: all mutable state that must survive a nested
/// call is saved on the C# call stack and restored on return.
/// </summary>
public partial class Executive
{
    internal int ThreadedExecuteLoop(int startAt = 0)
    {
        var thread    = Thread!;
        var varSlots  = Parent.VariableSlots;
        var constPool = Parent.Constants.Pool;
        var funcSlots = Parent.FunctionSlots;

        // Save all fields that recursive calls may overwrite
        var savedIP         = InstructionPointer;
        var savedFailure    = Failure;
        var savedErrorJump  = ErrorJump;
        ErrorJump = 0;

        // Set the instruction pointer for this call
        InstructionPointer = startAt;
        if (startAt == 0)
        {
            var entryKey = Parent.FoldCase(Parent.EntryLabel);
            if (!string.IsNullOrEmpty(entryKey))
            {
                var entryStmt = LabelTable[entryKey];
                if (entryStmt >= 0)
                {
                    var starts = Parent.StatementInstructionStarts;
                    if (starts != null && entryStmt < starts.Length)
                        InstructionPointer = starts[entryStmt];
                }
            }
        }

        int  exitCode        = -1;
        bool localSavedFailure = false; // local SaveFailure/RestoreFailure state

        while (InstructionPointer >= 0 && InstructionPointer < thread.Length)
        {
            var instr = thread[InstructionPointer++];

            switch (instr.Op)
            {
                case OpCode.Init:
                    AmpCurrentLineNumber = instr.IntOperand;
                    Failure = false;
                    AlphaStack.Clear();
                    BetaStack.Clear();
                    SystemStack.Push(new StatementSeparator());
                    if (AmpStatementLimit >= 0) AmpStatementCount++;
                    if (AmpStatementLimit > 0 && AmpStatementCount >= AmpStatementLimit)
                    {
                        LogRuntimeException(244);
                        Failure = true;
                        exitCode = -1;
                        goto Done;
                    }
                    break;

                case OpCode.Finalize:
                    while (SystemStack.Peek() is not StatementSeparator)
                        SystemStack.Pop();
                    SystemStack.Pop();
                    AmpLastLineNumber = AmpCurrentLineNumber;
                    if (ErrorJump > 0) ProcessTrappedErrorThreaded();
                    break;

                case OpCode.PushVar:
                {
                    // Direct array access — no string hash, no dict lookup.
                    // Expand if BuildEval/BuildCode added slots after init.
                    var slotIdx = instr.IntOperand;
                    if (slotIdx >= VarSlotArray.Length) ExpandVarSlotArray();
                    SystemStack.Push(VarSlotArray[slotIdx]);
                    break;
                }

                case OpCode.PushConst:
                    // Clone to prevent runtime mutation of the shared pool object
                    SystemStack.Push(constPool[instr.IntOperand].Clone());
                    break;

                case OpCode.PushExpr:
                    Constant(StarFunctionList[instr.IntOperand]);
                    break;

                case OpCode.CallFunc:
                    // Function name StringVar was pushed before the args by PushConst.
                    // Function() pops args then the name.
                    Function(instr.IntOperand2);
                    break;

                case OpCode.OpAdd:       OperatorFast(OpCode.OpAdd,       2); break;
                case OpCode.OpSubtract:  OperatorFast(OpCode.OpSubtract,  2); break;
                case OpCode.OpMultiply:  OperatorFast(OpCode.OpMultiply,  2); break;
                case OpCode.OpDivide:    OperatorFast(OpCode.OpDivide,    2); break;
                case OpCode.OpPower:     OperatorFast(OpCode.OpPower,     2); break;
                case OpCode.OpConcat:    OperatorFast(OpCode.OpConcat,    2); break;
                case OpCode.OpAlt:       OperatorFast(OpCode.OpAlt,       2); break;
                case OpCode.OpPeriod:    OperatorFast(OpCode.OpPeriod,    2); break;
                case OpCode.OpDollar:    OperatorFast(OpCode.OpDollar,    2); break;
                case OpCode.OpQuestion:  OperatorFast(OpCode.OpQuestion,  2); break;
                case OpCode.OpAt:        OperatorFast(OpCode.OpAt,        2); break;
                case OpCode.OpAmpersand: OperatorFast(OpCode.OpAmpersand, 2); break;
                case OpCode.OpPercent:   OperatorFast(OpCode.OpPercent,   2); break;
                case OpCode.OpHash:      OperatorFast(OpCode.OpHash,      2); break;
                case OpCode.OpTilde:     OperatorFast(OpCode.OpTilde,     2); break;

                case OpCode.OpUnaryMinus:    OperatorFast(OpCode.OpUnaryMinus,    1); break;
                case OpCode.OpUnaryPlus:     OperatorFast(OpCode.OpUnaryPlus,     1); break;
                case OpCode.OpIndirection:   OperatorFast(OpCode.OpIndirection,   1); break;
                case OpCode.OpKeyword:       OperatorFast(OpCode.OpKeyword,       1); break;
                case OpCode.OpName:          OperatorFast(OpCode.OpName,          1); break;
                case OpCode.OpNegation:      OperatorFast(OpCode.OpNegation,      0); break;
                case OpCode.OpInterrogation: OperatorFast(OpCode.OpInterrogation, 0); break;
                case OpCode.OpUnaryAt:       OperatorFast(OpCode.OpUnaryAt,       1); break;
                case OpCode.OpUnaryPercent:  OperatorFast(OpCode.OpUnaryPercent,  1); break;
                case OpCode.OpUnaryHash:     OperatorFast(OpCode.OpUnaryHash,     1); break;
                case OpCode.OpUnarySlash:    OperatorFast(OpCode.OpUnarySlash,    1); break;
                case OpCode.OpUnaryOpsyn:    Operator(((StringVar)constPool[instr.IntOperand]).Data, 1); break;

                case OpCode.BinaryEquals:    _BinaryEquals();   break;
                case OpCode.IndexCollection: IndexCollection(); break;

                case OpCode.ChoiceStart:
                    if (Failure) { SystemStack.Pop(); Failure = false; }
                    break;

                case OpCode.Jump:          InstructionPointer = instr.IntOperand;                     break;
                case OpCode.JumpOnSuccess: if (!Failure) InstructionPointer = instr.IntOperand;       break;
                case OpCode.JumpOnFailure: if  (Failure) InstructionPointer = instr.IntOperand;       break;

                case OpCode.Halt:
                    exitCode = -1;
                    goto Done;

                case OpCode.SaveFailure:
                    localSavedFailure = Failure;
                    Failure = false;
                    break;

                case OpCode.RestoreFailure:
                    Failure = localSavedFailure;
                    break;

                case OpCode.SetFailure:
                    Failure = true;
                    break;

                case OpCode.ClearFailure:
                    Failure = false;
                    break;

                case OpCode.CheckGotoFailure:
                    if (Failure)
                    {
                        LogRuntimeException(20);
                        // LogRuntimeException pushed a StringVar(false) on top of the
                        // label var that GotoIndirect needs. Pop it so dispatch works.
                        SystemStack.Pop();
                    }
                    break;

                case OpCode.GotoIndirect:
                {
                    var sym    = SystemStack.Peek().Symbol;
                    var target = LabelTable[sym];
                    SystemStack.Pop();
                    if (target == -1)                 { exitCode = -1; goto Done; }
                    if (target <= -2 && target >= -7) { exitCode = target; goto Done; }
                    if (target >= 0)
                    {
                        var instrIdx = StatementIndexToInstrIndex(target);
                        if (instrIdx >= 0)
                        {
                            thread = Thread!;  // AppendCompile may have extended Thread
                            InstructionPointer = instrIdx;
                        }
                        else { exitCode = target; goto Done; }
                    }
                    else { LogRuntimeException(instr.IntOperand); InstructionPointer = -1; }
                    break;
                }

                case OpCode.GotoIndirectCode:
                {
                    var sym = SystemStack.Peek().Symbol;
                    SystemStack.Pop();
                    if (IdentifierTable.ContainsKey(sym) && IdentifierTable[sym] is CodeVar cv)
                    {
                        var target = cv.StatementNumber;
                        if (target == -1)                 { exitCode = -1; goto Done; }
                        if (target <= -2 && target >= -7) { exitCode = target; goto Done; }
                        var instrIdx = StatementIndexToInstrIndex(target);
                        if (instrIdx >= 0)
                        {
                            thread = Thread!;  // AppendCompile may have extended Thread
                            InstructionPointer = instrIdx;
                        }
                        else { exitCode = target; goto Done; } // CODE'd stmt in Statements[]
                    }
                    else { LogRuntimeException(instr.IntOperand); InstructionPointer = -1; }
                    break;
                }
            }
        }

        Done:
        // Restore all fields saved at entry so the caller's context is intact.
        // Save the expression's final Failure state first so RunExpressionThread
        // can re-apply it after the restore.
        LastExpressionFailure = Failure;
        InstructionPointer    = savedIP;
        ErrorJump             = savedErrorJump;
        Failure               = savedFailure;
        return exitCode;
    }

    private int StatementIndexToInstrIndex(int stmtIdx)
    {
        if (stmtIdx < 0) return stmtIdx;
        var starts = Parent.StatementInstructionStarts;
        return (starts != null && stmtIdx < starts.Length) ? starts[stmtIdx] : -1;
    }

    private void ProcessTrappedErrorThreaded()
    {
        var target = ErrorJump;
        ErrorJump  = 0;
        InstructionPointer = StatementIndexToInstrIndex(target);
    }
}
