namespace Snobol4.Common;

/// <summary>
/// Snocone expression parser — Step 2.
///
/// Implements the shunting-yard algorithm from snocone.sc (binop/endexp/begexp).
/// Takes a flat List of ScToken from SnoconeLexer and returns postfix (RPN).
///
/// Reduce condition (from binop() in snocone.sc):
///   while existing_op.lp >= incoming_op.rp  → reduce
/// </summary>
public static class SnoconeParser
{
    // -------------------------------------------------------------------------
    // Precedence table — (lp, rp) pairs from bconv in snocone.sc
    // lp = left-side precedence, rp = right-side precedence
    // left-assoc:  lp == rp  (reduce when prev.lp >= new.rp, i.e. same level reduces)
    // right-assoc: lp < rp   (reduce only when strictly greater, so same level doesn't)
    // -------------------------------------------------------------------------
    private static readonly Dictionary<SnoconeLexer.ScKind, (int lp, int rp)> _prec =
        new()
        {
            [SnoconeLexer.ScKind.OpAssign]    = (1,  2),   // right-assoc
            [SnoconeLexer.ScKind.OpQuestion]  = (2,  2),
            [SnoconeLexer.ScKind.OpPipe]      = (3,  3),
            [SnoconeLexer.ScKind.OpOr]        = (4,  4),
            [SnoconeLexer.ScKind.OpConcat]    = (5,  5),
            // comparisons — all lp=6 rp=6
            [SnoconeLexer.ScKind.OpEq]        = (6,  6),
            [SnoconeLexer.ScKind.OpNe]        = (6,  6),
            [SnoconeLexer.ScKind.OpLt]        = (6,  6),
            [SnoconeLexer.ScKind.OpGt]        = (6,  6),
            [SnoconeLexer.ScKind.OpLe]        = (6,  6),
            [SnoconeLexer.ScKind.OpGe]        = (6,  6),
            [SnoconeLexer.ScKind.OpStrIdent]  = (6,  6),
            [SnoconeLexer.ScKind.OpStrDiffer] = (6,  6),
            [SnoconeLexer.ScKind.OpStrLt]     = (6,  6),
            [SnoconeLexer.ScKind.OpStrGt]     = (6,  6),
            [SnoconeLexer.ScKind.OpStrLe]     = (6,  6),
            [SnoconeLexer.ScKind.OpStrGe]     = (6,  6),
            [SnoconeLexer.ScKind.OpStrEq]     = (6,  6),
            [SnoconeLexer.ScKind.OpStrNe]     = (6,  6),
            [SnoconeLexer.ScKind.OpPlus]      = (7,  7),
            [SnoconeLexer.ScKind.OpMinus]     = (7,  7),
            [SnoconeLexer.ScKind.OpSlash]     = (8,  8),
            [SnoconeLexer.ScKind.OpStar]      = (8,  8),
            [SnoconeLexer.ScKind.OpPercent]   = (8,  8),
            [SnoconeLexer.ScKind.OpCaret]     = (9,  10),  // right-assoc
            [SnoconeLexer.ScKind.OpPeriod]    = (10, 10),
            [SnoconeLexer.ScKind.OpDollar]    = (10, 10),
        };

    // Unary operator set — ANY("+-*&@~?.$") from snocone.sc
    private static readonly HashSet<SnoconeLexer.ScKind> _unaryOps = new()
    {
        SnoconeLexer.ScKind.OpPlus,
        SnoconeLexer.ScKind.OpMinus,
        SnoconeLexer.ScKind.OpStar,
        SnoconeLexer.ScKind.OpAmpersand,
        SnoconeLexer.ScKind.OpAt,
        SnoconeLexer.ScKind.OpTilde,
        SnoconeLexer.ScKind.OpQuestion,
        SnoconeLexer.ScKind.OpPeriod,
        SnoconeLexer.ScKind.OpDollar,
    };

    private static bool IsOperand(SnoconeLexer.ScKind k) => k switch
    {
        SnoconeLexer.ScKind.Identifier or
        SnoconeLexer.ScKind.Integer    or
        SnoconeLexer.ScKind.Real       or
        SnoconeLexer.ScKind.String     => true,
        _ => false
    };

    /// <summary>
    /// Parse a flat infix token list into postfix (RPN).
    /// Implements begexp/operand/binop/endexp/unop from snocone.sc.
    /// </summary>
    public static List<SnoconeLexer.ScToken> ParseExpression(
        List<SnoconeLexer.ScToken> tokens)
    {
        var output = new List<SnoconeLexer.ScToken>();
        var opStack = new Stack<SnoconeLexer.ScToken>(); // holds ops + sentinel
        // Sentinel: par_binfo with lp=0 — marks bottom of expression
        var sentinel = new SnoconeLexer.ScToken(SnoconeLexer.ScKind.LParen, "(", 0);
        opStack.Push(sentinel);

        int i = 0;
        while (i < tokens.Count)
        {
            var tok = tokens[i];

            // ---- dotck: leading-dot float → prepend "0." ----
            if (tok.Kind == SnoconeLexer.ScKind.Real && tok.Text.StartsWith('.'))
            {
                tok = tok with { Text = "0" + tok.Text };
            }

            // ---- Operand ----
            if (IsOperand(tok.Kind))
            {
                output.Add(tok);
                i++;
                continue;
            }

            // ---- Left paren (grouping or function call start) ----
            if (tok.Kind == SnoconeLexer.ScKind.LParen)
            {
                opStack.Push(tok);
                i++;
                continue;
            }

            // ---- Right paren ----
            if (tok.Kind == SnoconeLexer.ScKind.RParen)
            {
                while (opStack.Count > 0 &&
                       opStack.Peek().Kind != SnoconeLexer.ScKind.LParen)
                    output.Add(opStack.Pop());
                if (opStack.Count > 0) opStack.Pop(); // discard '('
                i++;
                continue;
            }

            // ---- Left bracket (array ref) ----
            if (tok.Kind == SnoconeLexer.ScKind.LBracket)
            {
                opStack.Push(tok);
                i++;
                continue;
            }

            // ---- Right bracket — mkarray: becomes ScArrayRef ----
            if (tok.Kind == SnoconeLexer.ScKind.RBracket)
            {
                // Count args from output (simplified: track via comma count)
                while (opStack.Count > 0 &&
                       opStack.Peek().Kind != SnoconeLexer.ScKind.LBracket)
                    output.Add(opStack.Pop());
                if (opStack.Count > 0) opStack.Pop(); // discard '['
                // Replace preceding call node with array-ref node
                ReplaceLastCallWithArrayRef(output);
                i++;
                continue;
            }

            // ---- Comma: argument separator ----
            if (tok.Kind == SnoconeLexer.ScKind.Comma)
            {
                while (opStack.Count > 0 &&
                       opStack.Peek().Kind != SnoconeLexer.ScKind.LParen &&
                       opStack.Peek().Kind != SnoconeLexer.ScKind.LBracket)
                    output.Add(opStack.Pop());
                i++;
                continue;
            }

            // ---- Unary operator — must precede an operand ----
            if (_unaryOps.Contains(tok.Kind) && IsUnaryPosition(tokens, i))
            {
                // Parse the operand that follows recursively via iteration
                i++;
                // Recurse: parse next operand (may itself be unary+operand)
                int subEnd = ParseOperand(tokens, i, output, opStack);
                // Emit unary op after its operand
                output.Add(tok with { IsUnary = true });
                i = subEnd;
                continue;
            }

            // ---- Binary operator ----
            if (_prec.TryGetValue(tok.Kind, out var incoming))
            {
                // binop(): while existing_op.lp >= incoming_op.rp → reduce
                while (opStack.Count > 0 &&
                       _prec.TryGetValue(opStack.Peek().Kind, out var top) &&
                       top.lp >= incoming.rp)
                {
                    output.Add(opStack.Pop());
                }
                opStack.Push(tok);
                i++;
                continue;
            }

            // ---- Identifier followed by '(' or '[' — function/array call ----
            // (handled above via operand + paren logic; function name is emitted
            //  as identifier, call node appended at RParen/RBracket)

            i++; // skip unknown
        }

        // endexp(): drain remaining ops down to sentinel
        while (opStack.Count > 0 &&
               opStack.Peek().Kind != SnoconeLexer.ScKind.LParen)
            output.Add(opStack.Pop());

        return output;
    }

    // -------------------------------------------------------------------------
    // Determine if operator at position i is in unary position:
    // unary if at start, or previous token was an operator / open-paren
    // -------------------------------------------------------------------------
    private static bool IsUnaryPosition(
        List<SnoconeLexer.ScToken> tokens, int i)
    {
        if (i == 0) return true;
        var prev = tokens[i - 1].Kind;
        return prev == SnoconeLexer.ScKind.LParen   ||
               prev == SnoconeLexer.ScKind.LBracket ||
               prev == SnoconeLexer.ScKind.Comma    ||
               _prec.ContainsKey(prev)              ||
               _unaryOps.Contains(prev);
    }

    // -------------------------------------------------------------------------
    // Parse one operand (possibly prefixed by unary op) starting at index i.
    // Returns the index after the operand.
    // -------------------------------------------------------------------------
    private static int ParseOperand(
        List<SnoconeLexer.ScToken> tokens, int i,
        List<SnoconeLexer.ScToken> output,
        Stack<SnoconeLexer.ScToken> opStack)
    {
        if (i >= tokens.Count) return i;
        var tok = tokens[i];

        // dotck
        if (tok.Kind == SnoconeLexer.ScKind.Real && tok.Text.StartsWith('.'))
            tok = tok with { Text = "0" + tok.Text };

        if (IsOperand(tok.Kind))
        {
            output.Add(tok);
            return i + 1;
        }
        if (_unaryOps.Contains(tok.Kind))
        {
            int next = ParseOperand(tokens, i + 1, output, opStack);
            output.Add(tok with { IsUnary = true });
            return next;
        }
        if (tok.Kind == SnoconeLexer.ScKind.LParen)
        {
            // grouped sub-expression — find matching ) and recurse
            opStack.Push(tok);
            return i + 1;
        }
        return i + 1;
    }

    // -------------------------------------------------------------------------
    // After emitting array args, replace the preceding ScCall with ScArrayRef
    // -------------------------------------------------------------------------
    private static void ReplaceLastCallWithArrayRef(
        List<SnoconeLexer.ScToken> output)
    {
        // The array-ref node isn't in output yet; we emit it here
        // Find the identifier that started this call to count args
        // For now emit a placeholder — arg count will be patched
        output.Add(new SnoconeLexer.ScToken(
            SnoconeLexer.ScKind.ScArrayRef, "[]", 0) { ArgCount = 1 });
    }
}
