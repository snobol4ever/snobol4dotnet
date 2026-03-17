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
    /// Compiled <c>Func&lt;Executive, int&gt;</c> delegates, indexed by the
    /// value stored in <see cref="MsilCache"/>.
    /// Return value convention:
    ///   int.MinValue = fall through (loop advances IP normally)
    ///   -1           = halt / end of program
    ///   >= 0         = jump to this instruction pointer
    /// Grows as statements are compiled; never shrinks.
    /// </summary>
    internal List<Func<Executive, int>> MsilDelegates = new();

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

    private static readonly MethodInfo _initStatementMsil =
        typeof(Executive).GetMethod("InitStatementMsil",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "InitStatementMsil");

    private static readonly MethodInfo _finalizeStatementMsil =
        typeof(Executive).GetMethod("FinalizeStatementMsil",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [], null)
        ?? throw new MissingMethodException(nameof(Executive), "FinalizeStatementMsil");

    private static readonly MethodInfo _resolveLabel =
        typeof(Executive).GetMethod("ResolveLabel",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(string), typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "ResolveLabel");

    private static readonly MethodInfo _resolveLabelFromStack =
        typeof(Executive).GetMethod("ResolveLabelFromStack",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "ResolveLabelFromStack");

    private static readonly MethodInfo _resolveCodeLabelFromStack =
        typeof(Executive).GetMethod("ResolveCodeLabelFromStack",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [typeof(int)], null)
        ?? throw new MissingMethodException(nameof(Executive), "ResolveCodeLabelFromStack");

    private static readonly MethodInfo _checkGotoExprFailure =
        typeof(Executive).GetMethod("CheckGotoExprFailure",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, [], null)
        ?? throw new MissingMethodException(nameof(Executive), "CheckGotoExprFailure");

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
        for (var si = 0; si < Code.SourceLines.Count; si++)
        {
            var line = Code.SourceLines[si];

            // ── Detect what can be absorbed into the body delegate ─────────
            // Plain-paren form :(LABEL) / :S(LABEL) / :F(LABEL) with a single
            // bare IDENTIFIER → directGotoLabel (IP returned directly, no thread op).
            // Angle-bracket forms (DirectGotoFirst == true) → indirectGotoExpr
            // absorbed into the delegate via EmitIndirectGotoIL — no GotoIndirectCode
            // remains in the thread, keeping ThreadIsMsilOnly=true.
            string?      directUncondLabel  = null;
            List<Token>? indirectUncondExpr = null;
            bool         indirectUncondCode = false;

            if (line.ParseUnconditionalGoto.Count > 0)
            {
                if (!line.DirectGotoFirst &&
                    line.ParseUnconditionalGoto.Count == 1 &&
                    line.ParseUnconditionalGoto[0].TokenType == Token.Type.IDENTIFIER)
                {
                    // Plain :(LABEL) — absorb as direct IP return.
                    directUncondLabel = line.ParseUnconditionalGoto[0].MatchedString;
                }
                else if (line.DirectGotoFirst)
                {
                    // Angle-bracket :(<EXPR>) or :<VAR> — absorb via indirectGotoExpr.
                    indirectUncondExpr = line.ParseUnconditionalGoto;
                    indirectUncondCode = true;
                }
            }

            // Conditional gotos — only when no unconditional goto (mutually exclusive).
            string?      successLabel    = null;
            List<Token>? successExpr     = null;
            bool         successExprCode = false;
            string?      failureLabel    = null;
            List<Token>? failureExpr     = null;
            bool         failureExprCode = false;

            if (directUncondLabel == null && indirectUncondExpr == null)
            {
                bool hasBoth = line.ParseSuccessGoto.Count > 0 && line.ParseFailureGoto.Count > 0;

                bool successIsFirst  = line.SuccessFirst;
                bool successIsDirect = hasBoth
                    ? (successIsFirst ? line.DirectGotoFirst : line.DirectGotoSecond)
                    : line.DirectGotoFirst;
                bool failureIsDirect = hasBoth
                    ? (successIsFirst ? line.DirectGotoSecond : line.DirectGotoFirst)
                    : line.DirectGotoFirst;

                // Success goto
                if (line.ParseSuccessGoto.Count > 0)
                {
                    if (!successIsDirect &&
                        line.ParseSuccessGoto.Count == 1 &&
                        line.ParseSuccessGoto[0].TokenType == Token.Type.IDENTIFIER)
                        successLabel = line.ParseSuccessGoto[0].MatchedString;
                    else if (successIsDirect)
                    {
                        successExpr     = line.ParseSuccessGoto;
                        successExprCode = true;
                    }
                }

                // Failure goto
                if (line.ParseFailureGoto.Count > 0)
                {
                    if (!failureIsDirect &&
                        line.ParseFailureGoto.Count == 1 &&
                        line.ParseFailureGoto[0].TokenType == Token.Type.IDENTIFIER)
                        failureLabel = line.ParseFailureGoto[0].MatchedString;
                    else if (failureIsDirect)
                    {
                        failureExpr     = line.ParseFailureGoto;
                        failureExprCode = true;
                    }
                }

                // Mixed cases (one side direct-label, other side indirect-expr) are
                // handled by EmitAndCache — both sides are passed and emitted together.
            }

            TryCache(line.ParseBody, stmtIdx: si, isBody: true,
                     directGotoLabel:  directUncondLabel,
                     indirectGotoExpr: indirectUncondExpr,
                     indirectGotoCode: indirectUncondCode,
                     successLabel:     successLabel,
                     successExpr:      successExpr,
                     successExprCode:  successExprCode,
                     failureLabel:     failureLabel,
                     failureExpr:      failureExpr,
                     failureExprCode:  failureExprCode,
                     successFirst:     line.SuccessFirst);

            // Only cache goto token lists separately when NOT absorbed into body.
            if (directUncondLabel == null && indirectUncondExpr == null)
                TryCache(line.ParseUnconditionalGoto, stmtIdx: si, isBody: false);
            if (successLabel == null && successExpr == null)
                TryCache(line.ParseSuccessGoto,       stmtIdx: si, isBody: false);
            if (failureLabel == null && failureExpr == null)
                TryCache(line.ParseFailureGoto,       stmtIdx: si, isBody: false);
        }
    }

    private void TryCache(List<Token> tokens, int stmtIdx, bool isBody,
                          string?      directGotoLabel  = null,
                          List<Token>? indirectGotoExpr = null,
                          bool         indirectGotoCode = false,
                          string?      successLabel     = null,
                          List<Token>? successExpr      = null,
                          bool         successExprCode  = false,
                          string?      failureLabel     = null,
                          List<Token>? failureExpr      = null,
                          bool         failureExprCode  = false,
                          bool         successFirst     = false)
    {
        bool hasGoto = directGotoLabel  != null || indirectGotoExpr != null ||
                       successLabel     != null || successExpr      != null ||
                       failureLabel     != null || failureExpr      != null;
        // Allow empty-body delegates through for body slots (label-only / 'end' statements)
        // so Init/Finalize are inlined in a CallMsil and no threaded opcodes remain.
        if (tokens.Count == 0 && !hasGoto && !isBody) return;
        if (tokens.Count == 0 && !isBody) return;
        if (MsilCache.ContainsKey(tokens)) return;

        var dm = EmitAndCache(tokens, stmtIdx, isBody,
                              directGotoLabel,  indirectGotoExpr, indirectGotoCode,
                              successLabel,     successExpr,      successExprCode,
                              failureLabel,     failureExpr,      failureExprCode,
                              successFirst);
        if (dm == null) return;

        int idx = MsilDelegates.Count;
        MsilDelegates.Add(dm);
        MsilCache[tokens] = idx;
    }
    /// Compile <paramref name="tokens"/> into a <c>DynamicMethod</c> and
    /// return it as an <c>Action&lt;Executive&gt;</c>, or <c>null</c> if the
    /// token list contains nothing emittable (structural tokens only).
    /// </summary>
    private Func<Executive, int>? EmitAndCache(List<Token> tokens, int stmtIdx, bool isBody,
                                               string?      directGotoLabel  = null,
                                               List<Token>? indirectGotoExpr = null,
                                               bool         indirectGotoCode = false,
                                               string?      successLabel     = null,
                                               List<Token>? successExpr      = null,
                                               bool         successExprCode  = false,
                                               string?      failureLabel     = null,
                                               List<Token>? failureExpr      = null,
                                               bool         failureExprCode  = false,
                                               bool         successFirst     = false)
    {
        var dm = new DynamicMethod(
            name:            "Snobol4_Expr",
            returnType:      typeof(int),
            parameterTypes:  [typeof(Executive)],
            owner:           typeof(Executive),
            skipVisibility:  true);

        var il = dm.GetILGenerator();

        // ── Body delegates: inline Init/Finalize ──────────────────────────
        // For body token lists we emit InitStatementMsil(stmtIdx) at the top.
        // If it returns true (statement limit exceeded) we return -1 (halt).
        // FinalizeStatementMsil() is emitted at the bottom before Ret.
        Label skipBody = default;
        if (isBody)
        {
            skipBody = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, stmtIdx);
            il.Emit(OpCodes.Call, _initStatementMsil);   // returns bool
            il.Emit(OpCodes.Brfalse, skipBody);           // if false → continue
            // Statement limit hit — return -1 (halt)
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Ret);
            il.MarkLabel(skipBody);
        }

        // bookkeeping for IDENTIFIER_FUNCTION / R_PAREN_FUNCTION
        var pendingFunctionNames = new Stack<string>();

        // bookkeeping for COMMA_CHOICE / R_PAREN_CHOICE
        var choiceLabels = new Stack<Label>();

        bool anyEmitted = false;

        var pendingFunctionNames2 = new Stack<string>();
        var choiceLabels2         = new Stack<Label>();

        foreach (var t in tokens)
        {
            if (!EmitSingleToken(il, t, pendingFunctionNames2, choiceLabels2))
                return null;  // unhandled token → fall back to threaded path
            if (t.TokenType != Token.Type.IDENTIFIER_FUNCTION)  // IDENTIFIER_FUNCTION emits nothing
                anyEmitted = true;
        }

        // Patch any remaining open choice labels.
        while (choiceLabels2.Count > 0)
            il.MarkLabel(choiceLabels2.Pop());

        // A body-only delegate with no tokens is valid when a goto is absorbed,
        // OR when this is a body with zero tokens (label-only / 'end' statement)
        // so that Init/Finalize still fire and the thread stays pure-CallMsil.
        bool hasGoto = directGotoLabel  != null || indirectGotoExpr != null ||
                       successLabel     != null || successExpr      != null ||
                       failureLabel     != null || failureExpr      != null;
        if (!anyEmitted && !hasGoto && !(isBody && tokens.Count == 0)) return null;

        // ── Body delegates: finalize + goto / fall-through return ─────────
        if (isBody)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, _finalizeStatementMsil);

            if (directGotoLabel != null)
            {
                // Unconditional :(LABEL) — resolve label by name, return IP.
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, directGotoLabel);
                il.Emit(OpCodes.Ldc_I4, 23);
                il.Emit(OpCodes.Call, _resolveLabel);
                il.Emit(OpCodes.Ret);
                return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
            }

            if (indirectGotoExpr != null)
            {
                // Unconditional :(EXPR) / :<VAR> — eval expr, resolve from stack.
                if (!EmitIndirectGotoIL(il, indirectGotoExpr, indirectGotoCode,
                                        isConditional: false, isFailureSide: false))
                    return null;
                return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
            }

            if ((successLabel != null || failureLabel != null) &&
                successExpr == null && failureExpr == null)
            {
                // Both sides plain direct-label (or one absent).
                EmitConditionalGotoIL(il, successLabel, failureLabel, successFirst);
                return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
            }

            if ((successExpr != null || failureExpr != null) &&
                successLabel == null && failureLabel == null)
            {
                // Both sides indirect-expression (or one absent).
                if (!EmitConditionalIndirectGotoIL(il,
                        successExpr, successExprCode,
                        failureExpr, failureExprCode,
                        successFirst))
                    return null;
                return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
            }

            if (successLabel != null || successExpr != null ||
                failureLabel != null || failureExpr != null)
            {
                // Mixed: one side direct-label, other side indirect-expr.
                if (!EmitMixedConditionalGotoIL(il,
                        successLabel, successExpr, successExprCode,
                        failureLabel, failureExpr, failureExprCode,
                        successFirst))
                    return null;
                return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
            }
        }

        // Return int.MinValue = "fall through" (loop advances IP normally).
        il.Emit(OpCodes.Ldc_I4, int.MinValue);
        il.Emit(OpCodes.Ret);

        return (Func<Executive, int>)dm.CreateDelegate(typeof(Func<Executive, int>));
    }

    // ── Indirect-goto IL emitters ────────────────────────────────────────

    /// <summary>
    /// Emit SaveFailure / expr tokens / CheckGotoExprFailure / RestoreFailure /
    /// ResolveLabelFromStack (or ResolveCodeLabelFromStack) / Ret.
    /// When <paramref name="isConditional"/> is true the whole block is wrapped
    /// in a branch that skips it when the condition is not met.
    /// Returns false when any token in <paramref name="gotoExpr"/> is unhandled.
    /// </summary>
    private bool EmitIndirectGotoIL(ILGenerator il, List<Token> gotoExpr, bool isCode,
                                     bool isConditional, bool isFailureSide)
    {
        // SaveFailure: stash Failure in a local before the skip branch so the
        // skip path can restore it correctly even when the body is not entered.
        var savedFailure = il.DeclareLocal(typeof(bool));
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, _failureField);
        il.Emit(OpCodes.Stloc, savedFailure);

        Label skipGoto = default;
        if (isConditional)
        {
            skipGoto = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _failureField);
            // Skip if the body-failure state is wrong for this side.
            if (isFailureSide)
                il.Emit(OpCodes.Brfalse, skipGoto);  // !Failure → skip failure goto
            else
                il.Emit(OpCodes.Brtrue,  skipGoto);  // Failure  → skip success goto
        }

        // Clear Failure for expr eval.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stfld, _failureField);

        // Emit each token in the goto expression.
        var gotoFunctionNames = new Stack<string>();
        var gotoChoiceLabels  = new Stack<Label>();
        foreach (var t in gotoExpr)
        {
            if (!EmitSingleToken(il, t, gotoFunctionNames, gotoChoiceLabels)) return false;
        }
        while (gotoChoiceLabels.Count > 0) il.MarkLabel(gotoChoiceLabels.Pop());

        // CheckGotoExprFailure: if expr failed, log error 20 and fall through.
        var exprOk = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, _checkGotoExprFailure);
        il.Emit(OpCodes.Brfalse, exprOk);
        // Restore Failure and fall through.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldloc, savedFailure);
        il.Emit(OpCodes.Stfld, _failureField);
        il.Emit(OpCodes.Ldc_I4, int.MinValue);
        il.Emit(OpCodes.Ret);
        il.MarkLabel(exprOk);

        // RestoreFailure.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldloc, savedFailure);
        il.Emit(OpCodes.Stfld, _failureField);

        // For the failure side, clear Failure before dispatching.
        if (isConditional && isFailureSide)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, _failureField);
        }

        // Resolve label from stack and return IP.
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, isCode ? 24 : 23);
        il.Emit(OpCodes.Call, isCode ? _resolveCodeLabelFromStack : _resolveLabelFromStack);
        il.Emit(OpCodes.Ret);

        if (isConditional)
        {
            il.MarkLabel(skipGoto);
            // Restore Failure on the skip path (was clobbered by savedFailure stash).
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, savedFailure);
            il.Emit(OpCodes.Stfld, _failureField);
        }

        return true;
    }

    /// <summary>
    /// Emit conditional indirect goto(s): :S(EXPR), :F(EXPR), or both.
    /// </summary>
    private bool EmitConditionalIndirectGotoIL(ILGenerator il,
                                                List<Token>? successExpr, bool successCode,
                                                List<Token>? failureExpr, bool failureCode,
                                                bool successFirst)
    {
        if (successFirst)
        {
            if (successExpr != null)
                if (!EmitIndirectGotoIL(il, successExpr, successCode, true, false)) return false;
            if (failureExpr != null)
                if (!EmitIndirectGotoIL(il, failureExpr, failureCode, true, true))  return false;
        }
        else
        {
            if (failureExpr != null)
                if (!EmitIndirectGotoIL(il, failureExpr, failureCode, true, true))  return false;
            if (successExpr != null)
                if (!EmitIndirectGotoIL(il, successExpr, successCode, true, false)) return false;
        }
        il.Emit(OpCodes.Ldc_I4, int.MinValue);
        il.Emit(OpCodes.Ret);
        return true;
    }

    /// <summary>
    /// Emit mixed conditional gotos where one side is a direct label and the other
    /// is an indirect expression — e.g. :S&lt;VAR&gt;F(LABEL) or :S(LABEL)F&lt;VAR&gt;.
    /// Emits each side in source order (successFirst governs order).
    /// The direct-label side is emitted as an inline Failure-check + ResolveLabel + Ret.
    /// The indirect-expr side is emitted via EmitIndirectGotoIL.
    /// </summary>
    private bool EmitMixedConditionalGotoIL(ILGenerator il,
                                             string?      successLabel,
                                             List<Token>? successExpr,  bool successExprCode,
                                             string?      failureLabel,
                                             List<Token>? failureExpr,  bool failureExprCode,
                                             bool successFirst)
    {
        // Emit one side: either a direct label or an indirect expr, conditioned on Failure flag.
        bool EmitOneSide(string? directLabel, List<Token>? indirectExpr, bool indirectCode, bool isFailureSide)
        {
            if (directLabel != null)
            {
                // Direct-label side: check Failure, resolve by name if triggered.
                var skip = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _failureField);
                if (isFailureSide)
                    il.Emit(OpCodes.Brfalse, skip);   // !Failure → skip failure goto
                else
                    il.Emit(OpCodes.Brtrue, skip);    // Failure  → skip success goto
                if (isFailureSide)
                {
                    // Clear Failure before resolving failure goto.
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Stfld, _failureField);
                }
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, directLabel);
                il.Emit(OpCodes.Ldc_I4, 23);
                il.Emit(OpCodes.Call, _resolveLabel);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(skip);
                return true;
            }
            if (indirectExpr != null)
                return EmitIndirectGotoIL(il, indirectExpr, indirectCode, true, isFailureSide);
            return true; // absent side — nothing to emit
        }

        if (successFirst)
        {
            if (!EmitOneSide(successLabel, successExpr, successExprCode, false)) return false;
            if (!EmitOneSide(failureLabel, failureExpr, failureExprCode, true))  return false;
        }
        else
        {
            if (!EmitOneSide(failureLabel, failureExpr, failureExprCode, true))  return false;
            if (!EmitOneSide(successLabel, successExpr, successExprCode, false)) return false;
        }
        il.Emit(OpCodes.Ldc_I4, int.MinValue);
        il.Emit(OpCodes.Ret);
        return true;
    }

    /// <summary>
    /// Emit IL that implements conditional goto branching after the body and Finalize.
    /// Mirrors the JumpOnFailure/JumpOnSuccess + CheckGotoFailure + GotoIndirect
    /// threaded sequence, but inline in IL returning the target IP directly.
    /// </summary>
    private void EmitConditionalGotoIL(ILGenerator il,
                                        string? successLabel, string? failureLabel,
                                        bool successFirst)
    {
        // Local helper: ldarg.0; ldstr label; ldc.i4 23; call ResolveLabel; ret
        void EmitResolve(string label)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, label);
            il.Emit(OpCodes.Ldc_I4, 23);
            il.Emit(OpCodes.Call, _resolveLabel);
            il.Emit(OpCodes.Ret);
        }

        // Local helper: ldc.i4 int.MinValue; ret  (fall through)
        void EmitFallthrough()
        {
            il.Emit(OpCodes.Ldc_I4, int.MinValue);
            il.Emit(OpCodes.Ret);
        }

        // Local helper: clear Failure flag
        void EmitClearFailure()
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, _failureField);
        }

        if (successLabel != null && failureLabel == null)
        {
            // :S(SL) only — if Failure fall through; else resolve SL.
            var fallLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _failureField);
            il.Emit(OpCodes.Brtrue, fallLabel);    // Failure == true → fall through
            EmitResolve(successLabel);
            il.MarkLabel(fallLabel);
            EmitFallthrough();
        }
        else if (failureLabel != null && successLabel == null)
        {
            // :F(FL) only — if !Failure fall through; else clear Failure + resolve FL.
            var fallLabel = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _failureField);
            il.Emit(OpCodes.Brfalse, fallLabel);   // Failure == false → fall through
            EmitClearFailure();
            EmitResolve(failureLabel!);
            il.MarkLabel(fallLabel);
            EmitFallthrough();
        }
        else // both successLabel != null && failureLabel != null
        {
            if (successFirst)
            {
                // :S(SL)F(FL) — success evaluated first
                var failPath = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _failureField);
                il.Emit(OpCodes.Brtrue, failPath);   // Failure → go to fail path
                EmitResolve(successLabel!);
                il.MarkLabel(failPath);
                EmitClearFailure();
                EmitResolve(failureLabel!);
            }
            else
            {
                // :F(FL)S(SL) — failure evaluated first
                var succPath = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _failureField);
                il.Emit(OpCodes.Brfalse, succPath);  // !Failure → go to success path
                EmitClearFailure();
                EmitResolve(failureLabel!);
                il.MarkLabel(succPath);
                EmitResolve(successLabel!);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Per-token IL emitter — used by both the main body loop and
    // EmitIndirectGotoIL for goto expression tokens.
    // Returns false for any token type that is unhandled (caller should bail).
    // -----------------------------------------------------------------------

    private bool EmitSingleToken(ILGenerator il, Token t,
                                  Stack<string> pendingFunctionNames,
                                  Stack<Label>  choiceLabels)
    {
        switch (t.TokenType)
        {
            // ── Binary operators ──────────────────────────────────────
            case Token.Type.BINARY_PLUS:      EmitOperator(il, OpCode.OpAdd,        2); return true;
            case Token.Type.BINARY_MINUS:     EmitOperator(il, OpCode.OpSubtract,   2); return true;
            case Token.Type.BINARY_STAR:      EmitOperator(il, OpCode.OpMultiply,   2); return true;
            case Token.Type.BINARY_SLASH:     EmitOperator(il, OpCode.OpDivide,     2); return true;
            case Token.Type.BINARY_CARET:     EmitOperator(il, OpCode.OpPower,      2); return true;
            case Token.Type.BINARY_CONCAT:    EmitOperator(il, OpCode.OpConcat,     2); return true;
            case Token.Type.BINARY_PIPE:      EmitOperator(il, OpCode.OpAlt,        2); return true;
            case Token.Type.BINARY_PERIOD:    EmitOperator(il, OpCode.OpPeriod,     2); return true;
            case Token.Type.BINARY_DOLLAR:    EmitOperator(il, OpCode.OpDollar,     2); return true;
            case Token.Type.BINARY_QUESTION:  EmitOperator(il, OpCode.OpQuestion,   2); return true;
            case Token.Type.BINARY_AT:        EmitOperator(il, OpCode.OpAt,         2); return true;
            case Token.Type.BINARY_AMPERSAND: EmitOperator(il, OpCode.OpAmpersand,  2); return true;
            case Token.Type.BINARY_PERCENT:   EmitOperator(il, OpCode.OpPercent,    2); return true;
            case Token.Type.BINARY_HASH:      EmitOperator(il, OpCode.OpHash,       2); return true;
            case Token.Type.BINARY_TILDE:     EmitOperator(il, OpCode.OpTilde,      2); return true;
            case Token.Type.BINARY_EQUAL:
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, _binaryEquals);
                return true;

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
                int unaryArgCount = opCode is OpCode.OpNegation or OpCode.OpInterrogation ? 0 : 1;
                if (opCode != OpCode.OpUnaryOpsyn)
                {
                    EmitOperator(il, opCode, unaryArgCount);
                }
                else
                {
                    var key = "_" + t.MatchedString;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, key);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Call, _operatorOpsyn);
                }
                return true;
            }

            // ── Variable push ─────────────────────────────────────────
            case Token.Type.IDENTIFIER:
            case Token.Type.IDENTIFIER_ARRAY_OR_TABLE:
            {
                var key = FoldCase(t.MatchedString);
                if (!VariableSlotIndex.TryGetValue(key, out var slotIdx)) return false;
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, slotIdx);
                il.Emit(OpCodes.Call, _pushVarBySlot);
                return true;
            }

            // ── Function call ─────────────────────────────────────────
            case Token.Type.IDENTIFIER_FUNCTION:
                pendingFunctionNames.Push(t.MatchedString);
                return true;  // nothing emitted yet — matched by R_PAREN_FUNCTION

            case Token.Type.R_PAREN_FUNCTION:
            {
                if (pendingFunctionNames.Count == 0) return false; // mismatched token — fall back
                var funcName = pendingFunctionNames.Pop();
                var key      = FoldCase(funcName);
                var argCount = (int)t.IntegerValue;
                var slotKey  = $"{key}/{argCount}";
                if (!FunctionSlotIndex.TryGetValue(slotKey, out var slotIdx)) return false;
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, slotIdx);
                il.Emit(OpCodes.Ldc_I4, argCount);
                il.Emit(OpCodes.Call, _callFuncBySlot);
                return true;
            }

            // ── Literal constants ─────────────────────────────────────
            case Token.Type.STRING:
            {
                var poolIdx = Constants.GetOrAddString(t.MatchedString);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, poolIdx);
                il.Emit(OpCodes.Call, _pushConstByIndex);
                return true;
            }
            case Token.Type.NULL:
            {
                var poolIdx = Constants.GetOrAddString("");
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, poolIdx);
                il.Emit(OpCodes.Call, _pushConstByIndex);
                return true;
            }
            case Token.Type.INTEGER:
            {
                var poolIdx = Constants.GetOrAddInteger(t.IntegerValue);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, poolIdx);
                il.Emit(OpCodes.Call, _pushConstByIndex);
                return true;
            }
            case Token.Type.REAL:
            {
                var poolIdx = Constants.GetOrAddReal(t.DoubleValue);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, poolIdx);
                il.Emit(OpCodes.Call, _pushConstByIndex);
                return true;
            }

            // ── Star-function (deferred expression) ───────────────────
            case Token.Type.EXPRESSION:
            {
                var exprIdx = int.Parse(t.MatchedString[4..]);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, exprIdx);
                il.Emit(OpCodes.Call, _pushExprByIndex);
                return true;
            }

            // ── Indexing ──────────────────────────────────────────────
            case Token.Type.R_ANGLE:
            case Token.Type.R_SQUARE:
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, _indexCollection);
                return true;

            // ── Choice operator (A,B) ─────────────────────────────────
            case Token.Type.COMMA_CHOICE:
            {
                var skipLabel = il.DefineLabel();
                choiceLabels.Push(skipLabel);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, _failureField);
                il.Emit(OpCodes.Brfalse_S, skipLabel);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, _choiceStartMethod);
                return true;
            }
            case Token.Type.R_PAREN_CHOICE:
            {
                int levels = (int)t.IntegerValue;
                for (int i = 0; i < levels; i++)
                    if (choiceLabels.Count > 0)
                        il.MarkLabel(choiceLabels.Pop());
                return true;
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
                return true;

            default:
                return false;  // unhandled — caller falls back to threaded path
        }
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
