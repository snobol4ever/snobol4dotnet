using System.Text;

namespace Snobol4.Common;

/// <summary>
/// Lexer for Snocone (.sc) source files.
///
/// Snocone (Andrew Koenig, AT&T Bell Labs, 1985) is a syntactic front-end
/// for SNOBOL4 that adds C-like control flow (if/else, while, do/while, for,
/// procedure, struct, #include) while preserving full SNOBOL4 semantics.
/// This lexer tokenizes .sc source into <see cref="ScToken"/> values; the
/// Snocone parser (Step 2) consumes those tokens to build an AST.
///
/// ## Token stream contract
/// The output of <see cref="Tokenize"/> is a flat list of <see cref="ScToken"/>.
/// Every token carries its <see cref="ScToken.Kind"/>, its verbatim
/// <see cref="ScToken.Text"/>, and the 1-based <see cref="ScToken.Line"/> on
/// which it appeared.
///
/// ## Source model (from Koenig spec and snocone.sc)
/// - Comments: # to end of line (stripped; not in token stream).
/// - Statement termination: newline ends a statement UNLESS the last
///   non-whitespace token before the newline ends with a continuation
///   character (any of @ $ % ^ &amp; * ( - + = [ &lt; &gt; | ~ , ? :).
///   A semicolon also terminates a statement within a line.
/// - Strings: delimited by matching ' or ".
/// - Numbers: integer or real (with optional exponent eEdD±).
/// - Leading-dot floats (.5) are legal here; the parser rewrites them to 0.5.
/// - Identifiers: [A-Za-z_][A-Za-z0-9_]*.
/// - Multi-character operators are matched longest-first.
/// - Whitespace between tokens is insignificant (consumed, not emitted).
/// </summary>
public static class SnoconeLexer
{
    // -------------------------------------------------------------------------
    // Token kinds
    // -------------------------------------------------------------------------

    public enum ScKind
    {
        // Literals
        Integer,
        Real,
        String,
        Identifier,

        // Keywords  (matched as identifiers then re-classified)
        KwIf,
        KwElse,
        KwWhile,
        KwDo,
        KwFor,
        KwReturn,
        KwFreturn,
        KwNreturn,
        KwGo,       // "go" — always followed by whitespace then "to"
        KwTo,       // "to" after "go"
        KwProcedure,
        KwStruct,

        // Punctuation
        LParen,         // (
        RParen,         // )
        LBrace,         // {
        RBrace,         // }
        LBracket,       // [
        RBracket,       // ]
        Comma,          // ,
        Semicolon,      // ;
        Colon,          // :   (label suffix — user label followed by colon)
        Hash,           // #include directive (kept as token for parser)

        // Snocone binary operators (in precedence order, lowest first)
        OpAssign,       // =       precedence 1  → SNOBOL4 =
        OpQuestion,     // ?       precedence 2  → SNOBOL4 ?
        OpPipe,         // |       precedence 3  → SNOBOL4 |
        OpOr,           // ||      precedence 4  → SNOBOL4 (a,b) alternation
        OpConcat,       // &&      precedence 5  → SNOBOL4 blank concatenation
        OpEq,           // ==      precedence 6  → EQ(a,b)
        OpNe,           // !=      precedence 6  → NE(a,b)
        OpLt,           // <       precedence 6  → LT(a,b)
        OpGt,           // >       precedence 6  → GT(a,b)
        OpLe,           // <=      precedence 6  → LE(a,b)
        OpGe,           // >=      precedence 6  → GE(a,b)
        OpStrIdent,     // ::      precedence 6  → IDENT(a,b)
        OpStrDiffer,    // :!:     precedence 6  → DIFFER(a,b)
        OpStrLt,        // :<:     precedence 6  → LLT(a,b)
        OpStrGt,        // :>:     precedence 6  → LGT(a,b)
        OpStrLe,        // :<=:    precedence 6  → LLE(a,b)
        OpStrGe,        // :>=:    precedence 6  → LGE(a,b)
        OpStrEq,        // :==:    precedence 6  → LEQ(a,b)
        OpStrNe,        // :!=:    precedence 6  → LNE(a,b)
        OpPlus,         // +       precedence 7  → SNOBOL4 +
        OpMinus,        // -       precedence 7  → SNOBOL4 -
        OpSlash,        // /       precedence 8  → SNOBOL4 /
        OpStar,         // *       precedence 8  → SNOBOL4 *
        OpPercent,      // %       precedence 8  → REMDR(a,b)
        OpCaret,        // ^       precedence 9/10 right-assoc → SNOBOL4 **
        OpPeriod,       // .       precedence 10 → SNOBOL4 . (conditional capture)
        OpDollar,       // $       precedence 10 → SNOBOL4 $ (immediate capture)

        // Unary-only operators (also appear in unaryop set in snocone.sc)
        OpAt,           // @
        OpAmpersand,    // &
        OpTilde,        // ~   (logical negation)
        OpUnaryPlus,    // + (unary)  — distinguished at parse time
        OpUnaryMinus,   // - (unary)
        OpUnaryStar,    // * (unary — unevaluated pattern)

        // Synthetic
        Newline,        // logical end-of-statement newline
        Eof,
        Unknown,
    }

    // -------------------------------------------------------------------------
    // Token record
    // -------------------------------------------------------------------------

    public readonly record struct ScToken(ScKind Kind, string Text, int Line);

    // -------------------------------------------------------------------------
    // Keyword table
    // -------------------------------------------------------------------------

    private static readonly Dictionary<string, ScKind> _keywords = new(StringComparer.Ordinal)
    {
        ["if"]        = ScKind.KwIf,
        ["else"]      = ScKind.KwElse,
        ["while"]     = ScKind.KwWhile,
        ["do"]        = ScKind.KwDo,
        ["for"]       = ScKind.KwFor,
        ["return"]    = ScKind.KwReturn,
        ["freturn"]   = ScKind.KwFreturn,
        ["nreturn"]   = ScKind.KwNreturn,
        ["go"]        = ScKind.KwGo,
        ["to"]        = ScKind.KwTo,
        ["procedure"] = ScKind.KwProcedure,
        ["struct"]    = ScKind.KwStruct,
    };

    // Characters that, when last on a line (before optional whitespace+comment),
    // signal continuation — the logical statement is NOT ended by the newline.
    // Derived from getinput() in snocone.sc:
    //   ANY("@$%^&*(-+=[<>|~,?:")
    private static readonly HashSet<char> _continuationChars = new()
    {
        '@', '$', '%', '^', '&', '*', '(', '-', '+', '=', '[', '<', '>', '|', '~', ',', '?', ':'
    };

    // -------------------------------------------------------------------------
    // Public entry point
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tokenize a complete Snocone source string.
    /// </summary>
    /// <param name="source">Full source text (may contain \r\n or \n).</param>
    /// <returns>Flat list of tokens, terminated by a single <see cref="ScKind.Eof"/>.</returns>
    public static List<ScToken> Tokenize(string source)
    {
        // Normalise line endings
        var lines = source.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        var result = new List<ScToken>();
        var logical = new List<(string text, int lineNo)>(); // lines joined for one statement

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            int lineNo = i + 1;

            // Strip # comment (respecting strings)
            var stripped = StripComment(raw);

            logical.Add((stripped, lineNo));

            // Decide whether this logical line continues
            if (IsContinuation(stripped) && i < lines.Length - 1)
                continue;

            // Emit tokens for the accumulated logical line(s)
            // Use the line number of the first physical line in this group
            int stmtLine = logical[0].lineNo;
            var joined = string.Join("", logical.Select(l => l.text));
            logical.Clear();

            if (string.IsNullOrWhiteSpace(joined))
                continue;

            // Split on un-quoted semicolons — each segment is an independent statement
            var segments = SplitSemicolon(joined);
            foreach (var seg in segments)
            {
                if (string.IsNullOrWhiteSpace(seg))
                    continue;
                TokenizeSegment(seg.Trim(), stmtLine, result);
                result.Add(new ScToken(ScKind.Newline, "\n", stmtLine));
            }
        }

        result.Add(new ScToken(ScKind.Eof, "", lines.Length));
        return result;
    }

    // -------------------------------------------------------------------------
    // Segment tokenizer
    // -------------------------------------------------------------------------

    private static void TokenizeSegment(string seg, int lineNo, List<ScToken> result)
    {
        int pos = 0;
        int len = seg.Length;

        while (pos < len)
        {
            // Skip whitespace
            if (char.IsWhiteSpace(seg[pos])) { pos++; continue; }

            char c = seg[pos];

            // --- String literal ---
            if (c == '\'' || c == '"')
            {
                char quote = c;
                int start = pos++;
                while (pos < len && seg[pos] != quote) pos++;
                if (pos < len) pos++; // consume closing quote
                result.Add(new ScToken(ScKind.String, seg[start..pos], lineNo));
                continue;
            }

            // --- Number: integer or real ---
            if (char.IsDigit(c) || (c == '.' && pos + 1 < len && char.IsDigit(seg[pos + 1])))
            {
                result.Add(ScanNumber(seg, ref pos, lineNo));
                continue;
            }

            // --- Identifier or keyword ---
            if (char.IsLetter(c) || c == '_')
            {
                int start = pos++;
                while (pos < len && (char.IsLetterOrDigit(seg[pos]) || seg[pos] == '_')) pos++;
                var word = seg[start..pos];
                var kind = _keywords.TryGetValue(word, out var kw) ? kw : ScKind.Identifier;
                result.Add(new ScToken(kind, word, lineNo));
                continue;
            }

            // --- Multi-character and single-character operators ---
            // Try longest match first (up to 4 chars)
            bool matched = false;
            for (int opLen = Math.Min(4, len - pos); opLen >= 1; opLen--)
            {
                var candidate = seg[pos..(pos + opLen)];
                if (_operators.TryGetValue(candidate, out var opKind))
                {
                    result.Add(new ScToken(opKind, candidate, lineNo));
                    pos += opLen;
                    matched = true;
                    break;
                }
            }
            if (!matched)
            {
                result.Add(new ScToken(ScKind.Unknown, seg[pos..(pos + 1)], lineNo));
                pos++;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Number scanner
    // -------------------------------------------------------------------------

    private static ScToken ScanNumber(string seg, ref int pos, int lineNo)
    {
        int start = pos;
        int len = seg.Length;
        bool isReal = false;

        // Leading dot (e.g. .5) — valid per dotck() in snocone.sc
        if (seg[pos] == '.')
        {
            isReal = true;
            pos++;
        }

        // Integer digits
        while (pos < len && char.IsDigit(seg[pos])) pos++;

        // Decimal part
        if (!isReal && pos < len && seg[pos] == '.' && pos + 1 < len && char.IsDigit(seg[pos + 1]))
        {
            isReal = true;
            pos++; // consume '.'
            while (pos < len && char.IsDigit(seg[pos])) pos++;
        }

        // Exponent: eEdD [+-] digits
        if (pos < len && "eEdD".Contains(seg[pos]))
        {
            isReal = true;
            pos++;
            if (pos < len && (seg[pos] == '+' || seg[pos] == '-')) pos++;
            while (pos < len && char.IsDigit(seg[pos])) pos++;
        }

        var text = seg[start..pos];
        return new ScToken(isReal ? ScKind.Real : ScKind.Integer, text, lineNo);
    }

    // -------------------------------------------------------------------------
    // Operator table — longest-match, built at static init
    // -------------------------------------------------------------------------

    private static readonly Dictionary<string, ScKind> _operators = new(StringComparer.Ordinal)
    {
        // 4-char
        [":!=:"]  = ScKind.OpStrNe,
        [":<=:"]  = ScKind.OpStrLe,
        [":>=:"]  = ScKind.OpStrGe,
        [":==:"]  = ScKind.OpStrEq,

        // 3-char
        [":!:"]   = ScKind.OpStrDiffer,
        [":<:"]   = ScKind.OpStrLt,
        [":>:"]   = ScKind.OpStrGt,

        // 2-char
        ["::"]    = ScKind.OpStrIdent,
        ["||"]    = ScKind.OpOr,
        ["&&"]    = ScKind.OpConcat,
        ["=="]    = ScKind.OpEq,
        ["!="]    = ScKind.OpNe,
        ["<="]    = ScKind.OpLe,
        [">="]    = ScKind.OpGe,
        ["**"]    = ScKind.OpCaret,   // SNOBOL4 ** → same as ^

        // 1-char
        ["="]     = ScKind.OpAssign,
        ["?"]     = ScKind.OpQuestion,
        ["|"]     = ScKind.OpPipe,
        ["+"]     = ScKind.OpPlus,
        ["-"]     = ScKind.OpMinus,
        ["/"]     = ScKind.OpSlash,
        ["*"]     = ScKind.OpStar,
        ["%"]     = ScKind.OpPercent,
        ["^"]     = ScKind.OpCaret,
        ["."]     = ScKind.OpPeriod,
        ["$"]     = ScKind.OpDollar,
        ["&"]     = ScKind.OpAmpersand,
        ["@"]     = ScKind.OpAt,
        ["~"]     = ScKind.OpTilde,
        ["<"]     = ScKind.OpLt,
        [">"]     = ScKind.OpGt,
        ["("]     = ScKind.LParen,
        [")"]     = ScKind.RParen,
        ["{"]     = ScKind.LBrace,
        ["}"]     = ScKind.RBrace,
        ["["]     = ScKind.LBracket,
        ["]"]     = ScKind.RBracket,
        [","]     = ScKind.Comma,
        [";"]     = ScKind.Semicolon,
        [":"]     = ScKind.Colon,
    };

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Strip a # comment from a raw line, respecting single- and double-quoted strings.
    /// Returns the portion before the comment (trailing whitespace preserved for
    /// continuation detection).
    /// </summary>
    internal static string StripComment(string line)
    {
        bool inSingle = false, inDouble = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\'' && !inDouble) { inSingle = !inSingle; continue; }
            if (c == '"'  && !inSingle) { inDouble = !inDouble; continue; }
            if (c == '#' && !inSingle && !inDouble)
                return line[..i];
        }
        return line;
    }

    /// <summary>
    /// A line is a continuation (does NOT end the logical statement) when its
    /// last non-whitespace character is one of the continuation characters.
    /// </summary>
    internal static bool IsContinuation(string strippedLine)
    {
        var trimmed = strippedLine.TrimEnd();
        if (trimmed.Length == 0) return false;
        return _continuationChars.Contains(trimmed[^1]);
    }

    /// <summary>
    /// Split on semicolons that are not inside string literals.
    /// </summary>
    internal static List<string> SplitSemicolon(string line)
    {
        var result = new List<string>();
        var buf = new StringBuilder();
        bool inSingle = false, inDouble = false;

        foreach (char c in line)
        {
            if (c == '\'' && !inDouble) { inSingle = !inSingle; buf.Append(c); continue; }
            if (c == '"'  && !inSingle) { inDouble = !inDouble; buf.Append(c); continue; }
            if (c == ';' && !inSingle && !inDouble)
            {
                result.Add(buf.ToString());
                buf.Clear();
            }
            else
            {
                buf.Append(c);
            }
        }
        result.Add(buf.ToString());
        return result;
    }
}
