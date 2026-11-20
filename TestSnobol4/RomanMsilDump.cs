using Snobol4.Common;
using Test.TestLexer;

namespace Test.Diagnostic;

/// <summary>
/// Prints a human-readable MSIL dump for Roman.sno so you can see exactly
/// what instructions the JIT-compiled delegates contain versus the old
/// per-opcode threaded stream.  Run with:
///   dotnet test --filter RomanMsilDump --logger "console;verbosity=detailed"
/// </summary>
[TestClass]
public class RomanMsilDump
{
    [TestMethod]
    public void DumpRomanMsil()
    {
        var script = @"
        DEFINE('ROMAN(N)T')                     :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =             :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                      :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                               :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
        R2 = ROMAN('9')
end";

        var b = SetupTests.SetupScript("-b", script, compileOnly: true);

        Console.WriteLine("=======================================================");
        Console.WriteLine("  Roman.sno — MSIL delegate summary");
        Console.WriteLine($"  {b.MsilCache.Count} delegates compiled,  " +
                          $"{b.Execute!.Thread!.Length} instructions in thread");
        Console.WriteLine("=======================================================");

        var lines  = b.Code.SourceLines;
        var thread = b.Execute!.Thread!;

        // ── 1. Annotated thread listing ─────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("── Annotated thread (Init/CallMsil/Finalize/goto) ──────");
        for (int i = 0; i < thread.Length; i++)
        {
            var instr = thread[i];
            if (instr.Op == OpCode.Init && instr.IntOperand < lines.Count)
            {
                Console.WriteLine();
                Console.WriteLine($"  ┌─ stmt {instr.IntOperand}: " +
                                  $"{lines[instr.IntOperand].Text.Trim()}");
            }
            string ann = instr.Op == OpCode.CallMsil
                ? $"   ← JIT delegate[{instr.IntOperand}]  (was: individual opcodes)"
                : "";
            Console.WriteLine($"  │ [{i,3}] {instr,-30}{ann}");
        }

        // ── 2. Per-delegate postfix → IL mapping ────────────────────────────
        Console.WriteLine();
        Console.WriteLine("── MSIL delegates: postfix tokens → IL calls ──────────");

        foreach (var line in lines)
        {
            int li = lines.IndexOf(line);
            DumpTokenList(b, $"stmt {li} body   ", line.ParseBody);
            DumpTokenList(b, $"stmt {li} :S goto ", line.ParseSuccessGoto);
            DumpTokenList(b, $"stmt {li} :F goto ", line.ParseFailureGoto);
            DumpTokenList(b, $"stmt {li} :(goto) ", line.ParseUnconditionalGoto);
        }

        Console.WriteLine();
        Console.WriteLine("=======================================================");

        // The test always passes — it's purely informational.
        Assert.IsTrue(b.MsilCache.Count > 0);
    }

    // -----------------------------------------------------------------------

    private static void DumpTokenList(Builder b, string label, List<Token> tokens)
    {
        if (tokens.Count == 0) return;
        if (!b.MsilCache.TryGetValue(tokens, out var delegateIdx)) return;

        Console.WriteLine();
        Console.WriteLine($"  [{label}]  → delegate[{delegateIdx}]");
        Console.WriteLine($"    {"Postfix token",-40}  {"IL sequence",-55}");
        Console.WriteLine($"    {new string('-', 40)}  {new string('-', 55)}");

        var pendingFunc = new Stack<string>();

        foreach (var t in tokens)
        {
            string tok = t.TokenType switch
            {
                Token.Type.IDENTIFIER             => $"VAR  {t.MatchedString}",
                Token.Type.IDENTIFIER_ARRAY_OR_TABLE => $"VAR  {t.MatchedString}[]",
                Token.Type.IDENTIFIER_FUNCTION    => $"FUNC {t.MatchedString}(",
                Token.Type.R_PAREN_FUNCTION       => $"CALL /{t.IntegerValue} args)",
                Token.Type.INTEGER                => $"INT  {t.IntegerValue}",
                Token.Type.REAL                   => $"REAL {t.DoubleValue}",
                Token.Type.STRING                 => $"STR  '{Trunc(t.MatchedString, 24)}'",
                Token.Type.UNARY_OPERATOR         => $"UNARY {t.MatchedString}",
                Token.Type.BINARY_EQUAL           => "OP   =",
                Token.Type.BINARY_PLUS            => "OP   +",
                Token.Type.BINARY_MINUS           => "OP   -",
                Token.Type.BINARY_STAR            => "OP   *",
                Token.Type.BINARY_SLASH           => "OP   /",
                Token.Type.BINARY_CONCAT          => "OP   (space)",
                Token.Type.BINARY_PIPE            => "OP   |",
                Token.Type.BINARY_PERIOD          => "OP   .",
                Token.Type.BINARY_DOLLAR          => "OP   $",
                Token.Type.BINARY_QUESTION        => "OP   ?",
                Token.Type.R_ANGLE                => "INDEX ]",
                Token.Type.R_SQUARE               => "INDEX >",
                Token.Type.COMMA_CHOICE           => "CHOICE ,",
                Token.Type.R_PAREN_CHOICE         => $"CHOICE_END /{t.IntegerValue}",
                Token.Type.EXPRESSION             => $"STAR {t.MatchedString}",
                _                                 => $"{t.TokenType}"
            };

            string il = t.TokenType switch
            {
                Token.Type.IDENTIFIER or Token.Type.IDENTIFIER_ARRAY_OR_TABLE =>
                    b.VariableSlotIndex.TryGetValue(b.FoldCase(t.MatchedString), out var vs)
                        ? $"ldarg.0; ldc.i4 {vs}; call PushVarBySlot"
                        : "(slot not found)",

                Token.Type.IDENTIFIER_FUNCTION =>
                    PushFunc(pendingFunc, t.MatchedString),

                Token.Type.R_PAREN_FUNCTION =>
                    BuildCallFuncIL(b, pendingFunc.TryPop(out var fn) ? fn : "?",
                                    (int)t.IntegerValue),

                Token.Type.INTEGER =>
                    $"ldarg.0; ldc.i4 {b.Constants.GetOrAddInteger(t.IntegerValue)}; call PushConstByIndex",

                Token.Type.REAL =>
                    $"ldarg.0; ldc.i4 {b.Constants.GetOrAddReal(t.DoubleValue)}; call PushConstByIndex",

                Token.Type.STRING =>
                    $"ldarg.0; ldc.i4 {b.Constants.GetOrAddString(t.MatchedString)}; call PushConstByIndex",

                Token.Type.NULL =>
                    $"ldarg.0; ldc.i4 {b.Constants.GetOrAddString("")}; call PushConstByIndex",

                Token.Type.BINARY_EQUAL =>
                    "ldarg.0; call _BinaryEquals",

                Token.Type.BINARY_PLUS            => OperatorFastIL("OpAdd",       2),
                Token.Type.BINARY_MINUS           => OperatorFastIL("OpSubtract",  2),
                Token.Type.BINARY_STAR            => OperatorFastIL("OpMultiply",  2),
                Token.Type.BINARY_SLASH           => OperatorFastIL("OpDivide",    2),
                Token.Type.BINARY_CARET           => OperatorFastIL("OpPower",     2),
                Token.Type.BINARY_CONCAT          => OperatorFastIL("OpConcat",    2),
                Token.Type.BINARY_PIPE            => OperatorFastIL("OpAlt",       2),
                Token.Type.BINARY_PERIOD          => OperatorFastIL("OpPeriod",    2),
                Token.Type.BINARY_DOLLAR          => OperatorFastIL("OpDollar",    2),
                Token.Type.BINARY_QUESTION        => OperatorFastIL("OpQuestion",  2),
                Token.Type.BINARY_AT              => OperatorFastIL("OpAt",        2),
                Token.Type.BINARY_AMPERSAND       => OperatorFastIL("OpAmpersand", 2),
                Token.Type.BINARY_PERCENT         => OperatorFastIL("OpPercent",   2),
                Token.Type.BINARY_HASH            => OperatorFastIL("OpHash",      2),
                Token.Type.BINARY_TILDE           => OperatorFastIL("OpTilde",     2),

                Token.Type.UNARY_OPERATOR =>
                    t.MatchedString is "~" or "?"
                        ? OperatorFastIL($"Op{(t.MatchedString == "~" ? "Negation" : "Interrogation")}", 0)
                        : OperatorFastIL($"OpUnary{MapUnaryName(t.MatchedString)}", 1),

                Token.Type.R_ANGLE or Token.Type.R_SQUARE =>
                    "ldarg.0; call IndexCollection",

                Token.Type.EXPRESSION =>
                    $"ldarg.0; ldc.i4 {int.Parse(t.MatchedString[4..])}; call PushExprByIndex",

                Token.Type.COMMA_CHOICE =>
                    "ldarg.0; ldfld Failure; brfalse.s <skipLabel>; ldarg.0; call ChoiceStart",

                Token.Type.R_PAREN_CHOICE =>
                    $"markLabel x{t.IntegerValue}",

                _ => "(structural — no IL)"
            };

            Console.WriteLine($"    {tok,-40}  {il}");
        }
    }

    private static string BuildCallFuncIL(Builder b, string funcName, int argc)
    {
        var key = $"{b.FoldCase(funcName)}/{argc}";
        var slot = b.FunctionSlotIndex.TryGetValue(key, out var si) ? si : -1;
        return $"ldarg.0; ldc.i4 {slot}; ldc.i4 {argc}; call CallFuncBySlot  // {funcName}";
    }

    private static string OperatorFastIL(string opName, int argc) =>
        $"ldarg.0; ldc.i4 ({opName}); ldc.i4 {argc}; call OperatorFast";

    private static string MapUnaryName(string sym) => sym switch
    {
        "-" => "Minus", "+" => "Plus", "$" => "Indirection",
        "&" => "Keyword", "." => "Name", "@" => "At",
        "%" => "Percent", "#" => "Hash", "/" => "Slash",
        _ => sym
    };

    private static string Trunc(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";

    private static string PushFunc(Stack<string> stack, string name)
    {
        stack.Push(name);
        return "(bookkeeping — no IL)";
    }
}
