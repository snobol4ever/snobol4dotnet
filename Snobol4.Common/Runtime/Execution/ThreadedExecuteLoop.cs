namespace Snobol4.Common;

/// <summary>
/// Threaded execution engine for SNOBOL4.NET.
///
/// Replaces the two-layer dispatch of the C# generation path:
///   Old: ExecuteLoop → Statements[i](x) → InitializeStatement → string dict lookups
///   New: ThreadedExecuteLoop → switch(instr.Op) → direct method calls
///
/// Variables and functions are accessed by pre-resolved array index.
/// Static gotos are pre-resolved instruction indices.
/// Constants are pre-allocated Var objects from the constant pool.
/// </summary>
public partial class Executive
{
    // Saved Failure value during unconditional goto evaluation
    private bool _savedFailure;

    /// <summary>
    /// Execute the compiled Instruction[] thread.
    /// Called in place of the C# generated Run() method when
    /// BuildOptions.UseThreadedExecution is true.
    /// </summary>
    internal void ThreadedExecuteLoop()
    {
        var thread    = Thread!;
        var varSlots  = Parent.VariableSlots;
        var constPool = Parent.Constants.Pool;
        var funcSlots = Parent.FunctionSlots;

        // Honour EntryLabel (same as ExecuteLoop)
        InstructionPointer = 0;
        var entryKey = Parent.FoldCase(Parent.EntryLabel);
        if (!string.IsNullOrEmpty(entryKey) &&
            LabelTable[entryKey] != GotoNotFound)
        {
            // LabelTable maps label → statement index; we need instruction index.
            // StatementStart is recorded in the compiler — use the label table
            // value to index into Parent's statement-start list.
            var stmtIdx = LabelTable[entryKey];
            if (stmtIdx >= 0 && stmtIdx < Parent.VariableSlots.Count)
                InstructionPointer = stmtIdx; // will be overridden below if needed
        }

        var savedFailure = ErrorJump > 0;
        ErrorJump = 0;

        while (InstructionPointer >= 0 && InstructionPointer < thread.Length)
        {
            var instr = thread[InstructionPointer++];

            switch (instr.Op)
            {
                // ---- Statement boundary --------------------------------

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

                // ---- Stack push ----------------------------------------

                case OpCode.PushVar:
                    Identifier(varSlots[instr.IntOperand].Symbol);
                    break;

                case OpCode.PushConst:
                    SystemStack.Push(constPool[instr.IntOperand]);
                    break;

                case OpCode.PushExpr:
                    Constant(StarFunctionList[instr.IntOperand]);
                    break;

                // ---- Function call -------------------------------------

                case OpCode.CallFunc:
                    Function_BySlot(funcSlots[instr.IntOperand], instr.IntOperand2);
                    break;

                // ---- Binary operators ----------------------------------

                case OpCode.OpAdd:       Operator("__+", 2); break;
                case OpCode.OpSubtract:  Operator("__-", 2); break;
                case OpCode.OpMultiply:  Operator("__*", 2); break;
                case OpCode.OpDivide:    Operator("__/", 2); break;
                case OpCode.OpPower:     Operator("__^", 2); break;
                case OpCode.OpConcat:    Operator("___", 2); break;
                case OpCode.OpAlt:       Operator("__|", 2); break;
                case OpCode.OpPeriod:    Operator("__.", 2); break;
                case OpCode.OpDollar:    Operator("__$", 2); break;
                case OpCode.OpQuestion:  Operator("__?", 2); break;
                case OpCode.OpAt:        Operator("__@", 2); break;
                case OpCode.OpAmpersand: Operator("__&", 2); break;
                case OpCode.OpPercent:   Operator("__%", 2); break;
                case OpCode.OpHash:      Operator("__#", 2); break;
                case OpCode.OpTilde:     Operator("__~", 2); break;

                // ---- Unary operators -----------------------------------

                case OpCode.OpUnaryMinus:    Operator("_-", 1); break;
                case OpCode.OpUnaryPlus:     Operator("_+", 1); break;
                case OpCode.OpIndirection:   Operator("_$", 1); break;
                case OpCode.OpKeyword:       Operator("_&", 1); break;
                case OpCode.OpName:          Operator("_.", 1); break;
                case OpCode.OpNegation:      Operator("_~", 0); break;
                case OpCode.OpInterrogation: Operator("_?", 0); break;
                case OpCode.OpUnaryAt:       Operator("_@", 1); break;
                case OpCode.OpUnaryPercent:  Operator("_%", 1); break;
                case OpCode.OpUnaryHash:     Operator("_#", 1); break;
                case OpCode.OpUnarySlash:    Operator("_/", 1); break;

                // ---- Assignment / indexing -----------------------------

                case OpCode.BinaryEquals:
                    _BinaryEquals();
                    break;

                case OpCode.IndexCollection:
                    IndexCollection();
                    break;

                // ---- Choice (comma operator) ---------------------------

                case OpCode.ChoiceStart:
                    if (Failure)
                    {
                        SystemStack.Pop();
                        Failure = false;
                    }
                    break;

                case OpCode.ChoiceEnd:
                    // Close (IntOperand - 1) extra brace levels — mirrors R_PAREN_CHOICE
                    for (var i = 1; i < instr.IntOperand; i++) { /* closing braces */ }
                    break;

                // ---- Control flow -------------------------------------

                case OpCode.Jump:
                    InstructionPointer = instr.IntOperand;
                    break;

                case OpCode.JumpOnSuccess:
                    if (!Failure) InstructionPointer = instr.IntOperand;
                    break;

                case OpCode.JumpOnFailure:
                    if (Failure)  InstructionPointer = instr.IntOperand;
                    break;

                case OpCode.Halt:
                    goto Done;

                // ---- Goto helpers --------------------------------------

                case OpCode.SaveFailure:
                    _savedFailure = Failure;
                    Failure = false;
                    break;

                case OpCode.RestoreFailure:
                    Failure = _savedFailure;
                    break;

                case OpCode.SetFailure:
                    Failure = true;
                    break;

                case OpCode.CheckGotoFailure:
                    if (Failure) LogRuntimeException(20);
                    break;

                case OpCode.GotoIndirect:
                {
                    var sym = SystemStack.Peek().Symbol;
                    var target = LabelTable[sym];
                    if (target != GotoNotFound)
                    {
                        SystemStack.Pop();
                        // target is a statement index — resolve to instruction index
                        InstructionPointer = StatementIndexToInstrIndex(target);
                    }
                    else
                    {
                        SystemStack.Pop();
                        LogRuntimeException(instr.IntOperand); // error 23
                        InstructionPointer = -1;
                    }
                    break;
                }

                case OpCode.GotoIndirectCode:
                {
                    var sym = SystemStack.Peek().Symbol;
                    if (IdentifierTable.ContainsKey(sym) &&
                        IdentifierTable[sym] is CodeVar codeVar)
                    {
                        SystemStack.Pop();
                        InstructionPointer = StatementIndexToInstrIndex(codeVar.StatementNumber);
                    }
                    else
                    {
                        SystemStack.Pop();
                        LogRuntimeException(instr.IntOperand); // error 24
                        InstructionPointer = -1;
                    }
                    break;
                }
            }
        }

        Done:
        Failure = savedFailure;
    }

    // -----------------------------------------------------------------------
    // Helper: resolve a statement index to an instruction index
    // The compiler stored statement starts in Builder; we need them at runtime.
    // -----------------------------------------------------------------------
    private int StatementIndexToInstrIndex(int stmtIdx)
    {
        // Special SNOBOL4 return labels
        if (stmtIdx == -1) return -1; // end of program
        if (stmtIdx < 0)  return stmtIdx; // RETURN, FRETURN etc. — handled by caller

        var starts = Parent.StatementInstructionStarts;
        if (starts != null && stmtIdx < starts.Length)
            return starts[stmtIdx];

        return -1; // safety fallback
    }

    // -----------------------------------------------------------------------
    // Function call by pre-resolved FunctionSlot
    // Avoids the FunctionName push + Function() string lookup path
    // -----------------------------------------------------------------------
    private void Function_BySlot(FunctionSlot slot, int argCount)
    {
        if (Failure) return;

        List<Var> arguments = [];
        if (SystemStack.ExtractArguments(argCount, arguments, this))
            return;

        var entry = FunctionTable[slot.Symbol];
        if (entry == null)
        {
            LogRuntimeException(22);
            return;
        }

        for (var i = arguments.Count; i < entry.ArgumentCount; i++)
            arguments.Add(StringVar.Null());

        InputArguments(arguments);
        arguments.Add(new StringVar(slot.Symbol));
        entry.Handler(arguments);
    }

    // -----------------------------------------------------------------------
    // Trapped error handling in threaded mode
    // -----------------------------------------------------------------------
    private void ProcessTrappedErrorThreaded()
    {
        var target = ErrorJump;
        ErrorJump = 0;
        InstructionPointer = StatementIndexToInstrIndex(target);
    }
}
