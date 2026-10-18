using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Corpus rung 2 — indirect variable reference via $ operator.
/// Programs use PASS/FAIL output convention; we assert last output line starts with PASS.
/// </summary>
[DoNotParallelize]
[TestClass]
public class Rung2_Indirect
{
    /// <summary>
    /// Runs a script and returns PASS/FAIL lines written to OUTPUT (Console.Error in DOTNET).
    /// Uses a lock to be safe under the assembly-level parallel test runner.
    /// </summary>
    private static readonly object s_consoleLock = new();

    private static List<string> RunGetOutput(string script)
    {
        var lines = new List<string>();
        lock (s_consoleLock)
        {
            var old = Console.Out;
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms) { AutoFlush = true };
            Console.SetOut(sw);
            try { SetupTests.SetupScript("-b", script); }
            finally { Console.SetOut(old); }
            ms.Position = 0;
            using var sr = new System.IO.StreamReader(ms);
            foreach (var line in sr.ReadToEnd().Split('\n'))
            {
                var t = line.Trim();
                if (t.StartsWith("PASS") || t.StartsWith("FAIL")) lines.Add(t);
            }
        }
        return lines;
    }

    [TestMethod]
    public void TEST_Corpus_210_indirect_ref()
    {
        var s = @"
        myvar = 'the real myvar'
        differ($'myvar', myvar)                   :f(e001)
        output = 'FAIL 210/001: $string lookup'        :(end)
e001
        differ($.myvar, myvar)                   :f(e002)
        output = 'FAIL 210/002: $.var lookup'          :(end)
e002
        output = 'PASS 210_indirect_ref (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_211_indirect_assign()
    {
        var s = @"
        $'qq' = 'x'
        differ(qq, 'x')                   :f(e001)
        output = 'FAIL 211/001: indirect assign sets named var' :(end)
e001
        differ($'_no_such_var_')                   :f(e002)
        output = 'FAIL 211/002: undefined indirect is null'     :(end)
e002
        output = 'PASS 211_indirect_assign (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_212_indirect_array()
    {
        var s = @"
        a = array(3)
        a<2> = 'x'
        differ($.a<2>, 'x')                   :f(e001)
        output = 'FAIL 212/001: $.var<index> indirect array' :(end)
e001
        output = 'PASS 212_indirect_array (1/1)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_213_indirect_name()
    {
        var s = @"
        A = 42
        X = 'A'
        differ($X, 42)                             :F(e001)
        OUTPUT = 'FAIL 213/001: dollar-X indirect'    :(END)
e001
        NM = .A
        differ($NM, 42)                            :F(e002)
        OUTPUT = 'FAIL 213/002: dollar-NM DT_N deref' :(END)
e002
        $X = 99
        differ(A, 99)                              :F(e003)
        OUTPUT = 'FAIL 213/003: dollar-X lvalue assign' :(END)
e003
        A = 77
        differ($.A, 77)                            :F(e004)
        OUTPUT = 'FAIL 213/004: dollar-dot literal'   :(END)
e004
        define('ref_b()')                          :(ref_b_end)
ref_b   ref_b = .A                                 :(NRETURN)
ref_b_end
        differ(ref_b(), 77)                        :F(e005)
        OUTPUT = 'FAIL 213/005: NRETURN read value'   :(END)
e005
        OUTPUT = 'PASS 213_indirect_name (5/5)'
END";
        Assert.AreEqual("PASS 213_indirect_name (5/5)", SetupTests.RunWithInput(s));
    }
    [TestMethod]
    public void TEST_Corpus_214_indirect_chain()
    {
        var s = @"
        A = 'B'
        B = 'C'
        C = 'final'
        r1 = $A
        r2 = $$A
        OUTPUT = 'PASS 214_indirect_chain'
END";
        Assert.AreEqual("PASS 214_indirect_chain", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_215_indirect_pattern_match()
    {
        var s = @"
        varname = 'subject'
        subject = 'hello world'
        $varname 'world'   :f(fail)
        OUTPUT = 'PASS 215_indirect_pattern_match'   :(END)
fail    OUTPUT = 'FAIL 215'
END";
        Assert.AreEqual("PASS 215_indirect_pattern_match", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_216_indirect_in_loop()
    {
        // Build V1, V2, V3 via indirect assignment in a loop
        var s = @"
        N = 1
loop    ident(N, 4)           :s(done)
        $('V' N) = N
        N = N + 1             :(loop)
done    ident(V1, 1)          :f(fail)
        ident(V2, 2)          :f(fail)
        ident(V3, 3)          :f(fail)
        OUTPUT = 'PASS 216_indirect_in_loop'   :(END)
fail    OUTPUT = 'FAIL 216'
END";
        Assert.AreEqual("PASS 216_indirect_in_loop", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_Corpus_217_indirect_chain_assign()
    {
        // Double-indirect chain: A -> B -> C -> 'final', $$A reads 'final'
        var s = @"
        A = 'B'
        B = 'C'
        C = 'final'
        r = $$A
        ident(r, 'final')     :f(fail)
        OUTPUT = 'PASS 217_indirect_chain_assign'   :(END)
fail    OUTPUT = 'FAIL 217'
END";
        Assert.AreEqual("PASS 217_indirect_chain_assign", SetupTests.RunWithInput(s));
    }

}
