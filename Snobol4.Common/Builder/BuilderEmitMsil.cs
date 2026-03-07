using System.Reflection;
using System.Reflection.Emit;

namespace Snobol4.Common;

/// <summary>
/// JIT-compiles each statement's expression token list into a
/// <c>DynamicMethod</c> delegate (<c>Action&lt;Executive&gt;</c>) at program
/// load time.  The delegate is cached in <see cref="MsilCache"/> keyed by
/// the <c>List&lt;Token&gt;</c> reference (object identity).
///
/// At runtime, one <c>CallMsil</c> opcode invokes the cached delegate,
/// replacing the individual PushVar / PushConst / CallFunc / operator
/// opcodes with a straight-line native call sequence.
///
/// Control-flow opcodes (Init, Finalize, Jump, JumpOnSuccess, JumpOnFailure,
/// Halt, GotoIndirect) are NOT emitted here — they stay in ThreadedExecuteLoop.
/// Only the expression body between Init and Finalize is JIT-compiled.
/// </summary>
public partial class Builder
{
    // -----------------------------------------------------------------------
    // Public cache — ThreadedCodeCompiler reads this to decide whether to
    // emit CallMsil instead of the individual expression opcodes.
    // Key   = List<Token> reference (object identity, not content equality).
    // Value = index into MsilDelegates.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps a token-list reference to its compiled delegate index.
    /// Populated by <see cref="EmitMsilForAllStatements"/>.
    /// </summary>
    internal Dictionary<List<Token>, int> MsilCache =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Compiled <c>Action&lt;Executive&gt;</c> delegates, indexed by the
    /// value stored in <see cref="MsilCache"/>.
    /// Grows as statements are compiled; never shrinks.
    /// </summary>
    internal List<Action<Executive>> MsilDelegates = new();

    // -----------------------------------------------------------------------
    // Pre-reflected MethodInfo / FieldInfo — resolved once, reused per emit
    // -----------------------------------------------------------------------

    private static readonly MethodInfo _pushVarBySlot =
        typeof(Executive).GetMethod("PushVarBySlot",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "PushVarBySlot");

    private static readonly MethodInfo _pushConstByIndex =
        typeof(Executive).GetMethod("PushConstByIndex",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "PushConstByIndex");

    private static readonly MethodInfo _callFuncBySlot =
        typeof(Executive).GetMethod("CallFuncBySlot",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int), typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "CallFuncBySlot");

    private static readonly MethodInfo _pushExprByIndex =
        typeof(Executive).GetMethod("PushExprByIndex",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "PushExprByIndex");

    private static readonly MethodInfo _operatorFast =
        typeof(Executive).GetMethod("OperatorFast",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(OpCode), typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "OperatorFast");

    private static readonly MethodInfo _binaryEquals =
        typeof(Executive).GetMethod("_BinaryEquals",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [], null)
        ?? throw new MissingMethodException(nameof(Executive), "_BinaryEquals");

    private static readonly MethodInfo _indexCollection =
        typeof(Executive).GetMethod("IndexCollection",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [], null)
        ?? throw new MissingMethodException(nameof(Executive), "IndexCollection");

    private static readonly MethodInfo _operatorOpsyn =
        typeof(Executive).GetMethod("Operator",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(string), typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "Operator");

    private static readonly MethodInfo _choiceStartMethod =
        typeof(Executive).GetMethod("ChoiceStart",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [], null)
        ?? throw new MissingMethodException(nameof(Executive), "ChoiceStart");

    private static readonly FieldInfo _failureField =
        typeof(Executive).GetField("Failure",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(nameof(Executive), "Failure");

    // -----------------------------------------------------------------------
    // Entry point — called from Builder after ResolveSlots()
    // -----------------------------------------------------------------------

    /// <summary>
    /// Walk every source line's token lists, JIT-compile each expression body
    /// into a <c>DynamicMethod</c>, and populate <see cref="MsilCache"/> /
    /// <see cref="MsilDelegates"/>.  Idempotent: already-cached lists are skipped.
    /// </summary>
    internal void EmitMsilForAllStatements()
    {
        foreach (var line in Code.SourceLines)
        {
            TryCache(line.ParseBody);
            TryCache(line.ParseSuccessGoto);
            TryCache(line.ParseFailureGoto);
            TryCache(line.ParseUnconditionalGoto);
        }
    }

    private void TryCache(List<Token> tokens)
    {
        if (tokens.Count == 0) return;
        if (MsilCache.ContainsKey(tokens)) return;

        var dm = EmitAndCache(tokens);
        if (dm == null) return;

        int idx = MsilDelegates.Count;
        MsilDelegates.Add(dm);
        MsilCache[tokens] = idx;
    }

    // -----------------------------------------------------------------------
    // Core emitter
    // -----------------------------------------------------------------------

    /// <summary>
    /// Compile <paramref name="tokens"/> into a <c>DynamicMethod</c> and
    /// return it as an <c>Action&lt;Executive&gt;</c>, or <c>null</c> if the
    /// token list contains nothing emittable (structural tokens only).
    /// </summary>
    private Action<Executive>? EmitAndCache(List<Token> tokens)
    {
        var dm = new DynamicMethod(
            name:            "Snobol4_Expr",
            returnType:      typeof(void),
            parameterTypes:  [typeof(Executive)],
            owner:           typeof(Executive),
            skipVisibility:  true);

        var il = dm.GetILGenerator();

        // bookkeeping for IDENTIFIER_FUNCTION / R_PAREN_FUNCTION
        var pendingFunctionNames = new Stack<string>();

        // bookkeeping for COMMA_CHOICE / R_PAREN_CHOICE
        var choiceLabels = new Stack<Label>();

        bool anyEmitted = false;

        foreach (var t in tokens)
        {
            switch (t.TokenType)
            {
                // ── Binary operators ──────────────────────────────────────
                case Token.Type.BINARY_PLUS:
                    EmitOperator(il, OpCode.OpAdd, 2);      anyEmitted = true; break;
                case Token.Type.BINARY_MINUS:
                    EmitOperator(il, OpCode.OpSubtract, 2); anyEmitted = true; break;
                case Token.Type.BINARY_STAR:
                    EmitOperator(il, OpCode.OpMultiply, 2); anyEmitted = true; break;
                case Token.Type.BINARY_SLASH:
                    EmitOperator(il, OpCode.OpDivide, 2);   anyEmitted = true; break;
                case Token.Type.BINARY_CARET:
                    EmitOperator(il, OpCode.OpPower, 2);    anyEmitted = true; break;
                case Token.Type.BINARY_CONCAT:
                    EmitOperator(il, OpCode.OpConcat, 2);   anyEmitted = true; break;
                case Token.Type.BINARY_PIPE:
                    EmitOperator(il, OpCode.OpAlt, 2);      anyEmitted = true; break;
                case Token.Type.BINARY_PERIOD:
                    EmitOperator(il, OpCode.OpPeriod, 2);   anyEmitted = true; break;
                case Token.Type.BINARY_DOLLAR:
                    EmitOperator(il, OpCode.OpDollar, 2);   anyEmitted = true; break;
                case Token.Type.BINARY_QUESTION:
                    EmitOperator(il, OpCode.OpQuestion, 2); anyEmitted = true; break;
                case Token.Type.BINARY_AT:
                    EmitOperator(il, OpCode.OpAt, 2);       anyEmitted = true; break;
                case Token.Type.BINARY_AMPERSAND:
                    EmitOperator(il, OpCode.OpAmpersand, 2);anyEmitted = true; break;
                case Token.Type.BINARY_PERCENT:
                    EmitOperator(il, OpCode.OpPercent, 2);  anyEmitted = true; break;
                case Token.Type.BINARY_HASH:
                    EmitOperator(il, OpCode.OpHash, 2);     anyEmitted = true; break;
                case Token.Type.BINARY_TILDE:
                    EmitOperator(il, OpCode.OpTilde, 2);    anyEmitted = true; break;
                case Token.Type.BINARY_EQUAL:
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _binaryEquals);
                    anyEmitted = true; break;

                // ── Unary operators ───────────────────────────────────────
                case Token.Type.UNARY_OPERATOR:
                {
                    var opCode = t.MatchedString switch
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
                        _   => OpCode.OpUnaryOpsyn
                    };

                    if (opCode != OpCode.OpUnaryOpsyn)
                    {
                        EmitOperator(il, opCode, 1);
                    }
                    else
                    {
                        // User-defined opsyn unary: call Operator("_X", 1)
                        var key = "_" + t.MatchedString;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldstr, key);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Call, _operatorOpsyn);
                    }
                    anyEmitted = true;
                    break;
                }

                // ── Variable push ─────────────────────────────────────────
                case Token.Type.IDENTIFIER:
                case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
                {
                    var key = FoldCase(t.MatchedString);
                    var slotIdx = VariableSlotIndex[key];
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, slotIdx);
                    il.Emit(OpCodes.Call, _pushVarBySlot);
                    anyEmitted = true;
                    break;
                }

                // ── Function call ─────────────────────────────────────────
                case Token.Type.IDENTIFIER_FUNCTION:
                    // In the MSIL path the function name is NOT pushed onto the
                    // stack (unlike the threaded path).  Just record it for the
                    // matching R_PAREN_FUNCTION.
                    pendingFunctionNames.Push(t.MatchedString);
                    break;

                case Token.Type.R_PAREN_FUNCTION:
                {
                    var funcName = pendingFunctionNames.Pop();
                    var key      = FoldCase(funcName);
                    var slotIdx  = FunctionSlotIndex[key];
                    var argCount = (int)t.IntegerValue;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, slotIdx);
                    il.Emit(OpCodes.Ldc_I4, argCount);
                    il.Emit(OpCodes.Call, _callFuncBySlot);
                    anyEmitted = true;
                    break;
                }

                // ── Literal constants ─────────────────────────────────────
                case Token.Type.STRING:
                {
                    var poolIdx = Constants.GetOrAddString(t.MatchedString);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, poolIdx);
                    il.Emit(OpCodes.Call, _pushConstByIndex);
                    anyEmitted = true;
                    break;
                }

                case Token.Type.NULL:
                {
                    var poolIdx = Constants.GetOrAddString("");
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, poolIdx);
                    il.Emit(OpCodes.Call, _pushConstByIndex);
                    anyEmitted = true;
                    break;
                }

                case Token.Type.INTEGER:
                {
                    var poolIdx = Constants.GetOrAddInteger(t.IntegerValue);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, poolIdx);
                    il.Emit(OpCodes.Call, _pushConstByIndex);
                    anyEmitted = true;
                    break;
                }

                case Token.Type.REAL:
                {
                    var poolIdx = Constants.GetOrAddReal(t.DoubleValue);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, poolIdx);
                    il.Emit(OpCodes.Call, _pushConstByIndex);
                    anyEmitted = true;
                    break;
                }

                // ── Star-function (deferred expression) ───────────────────
                case Token.Type.EXPRESSION:
                {
                    // MatchedString is "Star00000000", "Star00000001", etc.
                    var exprIdx = int.Parse(t.MatchedString[4..]);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4, exprIdx);
                    il.Emit(OpCodes.Call, _pushExprByIndex);
                    anyEmitted = true;
                    break;
                }

                // ── Indexing ──────────────────────────────────────────────
                case Token.Type.R_ANGLE:
                case Token.Type.R_SQUARE:
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _indexCollection);
                    anyEmitted = true;
                    break;

                // ── Choice operator (A,B) ─────────────────────────────────
                case Token.Type.COMMA_CHOICE:
                {
                    // If the preceding expression succeeded, skip the alternative.
                    // JumpOnSuccess means "jump if NOT failure" i.e. Failure == false.
                    var skipLabel = il.DefineLabel();
                    choiceLabels.Push(skipLabel);

                    // if (!Failure) goto skipLabel
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, _failureField);
                    il.Emit(OpCodes.Brfalse_S, skipLabel);  // Failure==false → skip alternative

                    // Previous expression failed: pop its result and clear failure flag
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, _choiceStartMethod);
                    anyEmitted = true;
                    break;
                }

                case Token.Type.R_PAREN_CHOICE:
                {
                    int levels = (int)t.IntegerValue;
                    for (int i = 0; i < levels; i++)
                        if (choiceLabels.Count > 0)
                            il.MarkLabel(choiceLabels.Pop());
                    anyEmitted = true;
                    break;
                }

                // ── Structural tokens — no IL emitted ─────────────────────
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
                    // Unknown token — bail out; caller falls back to threaded path
                    return null;
            }
        }

        // Patch any remaining single-comma choice labels (no R_PAREN_CHOICE emitted)
        while (choiceLabels.Count > 0)
            il.MarkLabel(choiceLabels.Pop());

        if (!anyEmitted) return null;

        il.Emit(OpCodes.Ret);

        return (Action<Executive>)dm.CreateDelegate(typeof(Action<Executive>));
    }

    // -----------------------------------------------------------------------
    // Helper — emit a call to OperatorFast(opCode, argumentCount)
    // -----------------------------------------------------------------------

    private static void EmitOperator(ILGenerator il, OpCode opCode, int argumentCount)
    {
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, (int)opCode);   // OpCode cast to int
        il.Emit(OpCodes.Ldc_I4, argumentCount);
        il.Emit(OpCodes.Call, _operatorFast);
    }
}
