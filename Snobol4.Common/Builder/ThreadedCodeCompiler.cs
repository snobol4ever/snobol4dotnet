namespace Snobol4.Common;

/// <summary>
/// Compiles parsed SNOBOL4 source lines into a flat Instruction[] array.
///
/// Two-pass:
///   Pass 1 — emit instructions, recording fixups for forward jumps
///   Pass 2 — patch all jump targets once statement start indices are known
/// </summary>
internal sealed class ThreadedCodeCompiler
{
    private readonly Builder _parent;
    private readonly List<Instruction> _thread = new(512);
    private readonly List<int> _statementStart = new(128);
    private readonly List<(int InstrIdx, int StmtIdx)> _jumpFixups = new(128);
    private readonly Stack<string> _pendingFunctions = new();

    internal ThreadedCodeCompiler(Builder parent)
    {
        _parent = parent;
    }

    internal Instruction[] Compile()
    {
        _thread.Clear();
        _statementStart.Clear();
        _jumpFixups.Clear();

        Pass1_Emit();
        return Pass2_Patch();
    }

    // -----------------------------------------------------------------------
    // Pass 1
    // -----------------------------------------------------------------------

    private void Pass1_Emit()
    {
        var lines = _parent.Code.SourceLines;

        for (var si = 0; si < lines.Count; si++)
        {
            var line = lines[si];
            _statementStart.Add(_thread.Count);

            Emit(new Instruction(OpCode.Init, (int)line.LineCountTotal));
            EmitTokenList(line.ParseBody);
            Emit(new Instruction(OpCode.Finalize));
            EmitGotos(line, si);
        }

        Emit(new Instruction(OpCode.Halt));
    }

    // -----------------------------------------------------------------------
    // Goto logic — mirrors GenerateStatementGotos in CodeGenerator
    // -----------------------------------------------------------------------

    private void EmitGotos(SourceLine line, int si)
    {
        int next = si + 1;

        if (line.ParseUnconditionalGoto.Count > 0)
        {
            EmitUnconditionalGoto(line);
            return;
        }

        if (line.ParseFailureGoto.Count == 0 && line.ParseSuccessGoto.Count == 0)
        {
            EmitJumpToStmt(OpCode.Jump, next);
            return;
        }

        if (line.ParseSuccessGoto.Count > 0 && line.ParseFailureGoto.Count > 0)
        {
            EmitBothGotos(line, next);
            return;
        }

        if (line.ParseSuccessGoto.Count > 0)
        {
            EmitSuccessOnly(line, next);
            return;
        }

        EmitFailureOnly(line, next);
    }

    private void EmitUnconditionalGoto(SourceLine line)
    {
        Emit(new Instruction(OpCode.SaveFailure));
        EmitTokenList(line.ParseUnconditionalGoto);
        Emit(new Instruction(OpCode.CheckGotoFailure));
        Emit(new Instruction(OpCode.RestoreFailure));
        EmitDispatch(line.DirectGotoFirst);
    }

    private void EmitBothGotos(SourceLine line, int next)
    {
        if (line.SuccessFirst)
        {
            int fp = EmitPlaceholder(OpCode.JumpOnFailure);
            EmitTokenList(line.ParseSuccessGoto);
            Emit(new Instruction(OpCode.CheckGotoFailure));
            EmitDispatch(line.DirectGotoFirst);
            PatchHere(fp);
            EmitTokenList(line.ParseFailureGoto);
            Emit(new Instruction(OpCode.CheckGotoFailure));
            Emit(new Instruction(OpCode.SetFailure));
            EmitDispatch(line.DirectGotoSecond);
        }
        else
        {
            int sp = EmitPlaceholder(OpCode.JumpOnSuccess);
            EmitTokenList(line.ParseFailureGoto);
            Emit(new Instruction(OpCode.CheckGotoFailure));
            Emit(new Instruction(OpCode.SetFailure));
            EmitDispatch(line.DirectGotoFirst);
            PatchHere(sp);
            EmitTokenList(line.ParseSuccessGoto);
            Emit(new Instruction(OpCode.CheckGotoFailure));
            EmitDispatch(line.DirectGotoSecond);
        }
    }

    private void EmitSuccessOnly(SourceLine line, int next)
    {
        EmitJumpToStmt(OpCode.JumpOnFailure, next);
        EmitTokenList(line.ParseSuccessGoto);
        Emit(new Instruction(OpCode.CheckGotoFailure));
        EmitDispatch(line.DirectGotoFirst);
    }

    private void EmitFailureOnly(SourceLine line, int next)
    {
        EmitJumpToStmt(OpCode.JumpOnSuccess, next);
        EmitTokenList(line.ParseFailureGoto);
        Emit(new Instruction(OpCode.CheckGotoFailure));
        Emit(new Instruction(OpCode.SetFailure));
        EmitDispatch(line.DirectGotoFirst);
    }

    private void EmitDispatch(bool isDirect)
    {
        Emit(isDirect
            ? new Instruction(OpCode.GotoIndirectCode, 24)
            : new Instruction(OpCode.GotoIndirect,     23));
    }

    // -----------------------------------------------------------------------
    // Token list → instructions
    // -----------------------------------------------------------------------

    private void EmitTokenList(List<Token> tokens)
    {
        foreach (var t in tokens)
        {
            switch (t.TokenType)
            {
                case Token.Type.BINARY_PLUS:      Emit(OpCode.OpAdd);       break;
                case Token.Type.BINARY_MINUS:     Emit(OpCode.OpSubtract);  break;
                case Token.Type.BINARY_STAR:      Emit(OpCode.OpMultiply);  break;
                case Token.Type.BINARY_SLASH:     Emit(OpCode.OpDivide);    break;
                case Token.Type.BINARY_CARET:     Emit(OpCode.OpPower);     break;
                case Token.Type.BINARY_CONCAT:    Emit(OpCode.OpConcat);    break;
                case Token.Type.BINARY_PIPE:      Emit(OpCode.OpAlt);       break;
                case Token.Type.BINARY_PERIOD:    Emit(OpCode.OpPeriod);    break;
                case Token.Type.BINARY_DOLLAR:    Emit(OpCode.OpDollar);    break;
                case Token.Type.BINARY_QUESTION:  Emit(OpCode.OpQuestion);  break;
                case Token.Type.BINARY_AT:        Emit(OpCode.OpAt);        break;
                case Token.Type.BINARY_AMPERSAND: Emit(OpCode.OpAmpersand); break;
                case Token.Type.BINARY_PERCENT:   Emit(OpCode.OpPercent);   break;
                case Token.Type.BINARY_HASH:      Emit(OpCode.OpHash);      break;
                case Token.Type.BINARY_TILDE:     Emit(OpCode.OpTilde);     break;
                case Token.Type.BINARY_EQUAL:     Emit(OpCode.BinaryEquals); break;

                case Token.Type.UNARY_OPERATOR:
                    Emit(t.MatchedString switch
                    {
                        "-" => OpCode.OpUnaryMinus,
                        "+" => OpCode.OpUnaryPlus,
                        "$" => OpCode.OpIndirection,
                        "&" => OpCode.OpKeyword,
                        "." => OpCode.OpName,
                        "~" => OpCode.OpNegation,
                        "?" => OpCode.OpInterrogation,
                        "@" => OpCode.OpUnaryAt,
                        "%" => OpCode.OpUnaryPercent,
                        "#" => OpCode.OpUnaryHash,
                        "/" => OpCode.OpUnarySlash,
                        _   => throw new ApplicationException(
                                   $"ThreadedCodeCompiler: unknown unary '{t.MatchedString}'")
                    });
                    break;

                case Token.Type.IDENTIFIER:
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                {
                    var key = _parent.FoldCase(t.MatchedString);
                    Emit(new Instruction(OpCode.PushVar, _parent.VariableSlotIndex[key]));
                    break;
                }

                case Token.Type.IDENTIFIER_FUNCTION:
                    _pendingFunctions.Push(t.MatchedString);
                    break;

                case Token.Type.R_PAREN_FUNCTION:
                {
                    var name     = _pendingFunctions.Pop();
                    var canon    = _parent.FoldCase(name);
                    var argc     = (int)t.IntegerValue;
                    var slotIdx  = _parent.FunctionSlotIndex[$"{canon}/{argc}"];
                    Emit(new Instruction(OpCode.CallFunc, slotIdx, argc));
                    break;
                }

                case Token.Type.STRING:
                    Emit(new Instruction(OpCode.PushConst,
                        _parent.Constants.GetOrAddString(t.MatchedString)));
                    break;

                case Token.Type.NULL:
                    Emit(new Instruction(OpCode.PushConst,
                        _parent.Constants.GetOrAddString("")));
                    break;

                case Token.Type.INTEGER:
                    Emit(new Instruction(OpCode.PushConst,
                        _parent.Constants.GetOrAddInteger(t.IntegerValue)));
                    break;

                case Token.Type.REAL:
                    Emit(new Instruction(OpCode.PushConst,
                        _parent.Constants.GetOrAddReal(t.DoubleValue)));
                    break;

                case Token.Type.EXPRESSION:
                    Emit(new Instruction(OpCode.PushExpr, (int)t.IntegerValue));
                    break;

                case Token.Type.R_ANGLE:
                case Token.Type.R_SQUARE:
                    Emit(OpCode.IndexCollection);
                    break;

                case Token.Type.COMMA_CHOICE:
                    Emit(OpCode.ChoiceStart);
                    break;

                case Token.Type.R_PAREN_CHOICE:
                    Emit(new Instruction(OpCode.ChoiceEnd, (int)t.IntegerValue));
                    break;

                // Structural tokens — no instruction emitted
                case Token.Type.COLON:
                case Token.Type.COMMA:
                case Token.Type.FAILURE_GOTO:
                case Token.Type.SUCCESS_GOTO:
                case Token.Type.L_ANGLE:
                case Token.Type.L_ANGLE_FAILURE:
                case Token.Type.L_ANGLE_SUCCESS:
                case Token.Type.L_ANGLE_UNCONDITIONAL:
                case Token.Type.L_PAREN_CHOICE:
                case Token.Type.L_PAREN_FAILURE:
                case Token.Type.L_PAREN_FUNCTION:
                case Token.Type.L_PAREN_SUCCESS:
                case Token.Type.L_PAREN_UNCONDITIONAL:
                case Token.Type.L_SQUARE:
                case Token.Type.R_ANGLE_FAILURE:
                case Token.Type.R_ANGLE_SUCCESS:
                case Token.Type.R_ANGLE_UNCONDITIONAL:
                case Token.Type.R_PAREN_SUCCESS:
                case Token.Type.R_PAREN_UNCONDITIONAL:
                case Token.Type.R_PAREN_FAILURE:
                case Token.Type.SPACE:
                case Token.Type.UNARY_STAR:
                    break;

                default:
                    throw new ApplicationException(
                        $"ThreadedCodeCompiler: unhandled token {t.TokenType}");
            }
        }
    }

    // -----------------------------------------------------------------------
    // Pass 2 — patch forward jump targets
    // -----------------------------------------------------------------------

    private Instruction[] Pass2_Patch()
    {
        var arr = _thread.ToArray();

        foreach (var (instrIdx, stmtIdx) in _jumpFixups)
        {
            int target = stmtIdx >= _statementStart.Count
                ? arr.Length - 1          // past last statement → Halt
                : _statementStart[stmtIdx];

            var old = arr[instrIdx];
            arr[instrIdx] = new Instruction(old.Op, target, old.IntOperand2);
        }

        return arr;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void Emit(OpCode op) => _thread.Add(new Instruction(op));
    private void Emit(Instruction i) => _thread.Add(i);

    private void EmitJumpToStmt(OpCode op, int stmtIdx)
    {
        _jumpFixups.Add((_thread.Count, stmtIdx));
        _thread.Add(new Instruction(op, -1));
    }

    private int EmitPlaceholder(OpCode op)
    {
        var idx = _thread.Count;
        _thread.Add(new Instruction(op, -1));
        return idx;
    }

    private void PatchHere(int instrIdx)
    {
        // The target is the *current* instruction count (next to be emitted)
        for (var i = 0; i < _jumpFixups.Count; i++)
        {
            if (_jumpFixups[i].InstrIdx == instrIdx)
            {
                _jumpFixups[i] = (instrIdx, _thread.Count);
                return;
            }
        }
        // Not in fixup list — it's an intra-statement placeholder, patch directly
        var old = _thread[instrIdx];
        _thread[instrIdx] = new Instruction(old.Op, _thread.Count, old.IntOperand2);
    }
}
