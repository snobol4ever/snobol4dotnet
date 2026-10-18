using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

[DoNotParallelize]
[TestClass]
public class Rung4_Arith
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
    public void TEST_Corpus_410_arith_int()
    {
        var s = @"
        differ(3 + 2, 5)                   :f(e001)
        output = 'FAIL 410/001: 3+2'                   :(end)
e001
        differ(3 - 2, 1)                   :f(e002)
        output = 'FAIL 410/002: 3-2'                   :(end)
e002
        differ(3 * 2, 6)                   :f(e003)
        output = 'FAIL 410/003: 3*2'                   :(end)
e003
        differ(5 / 2, 2)                   :f(e004)
        output = 'FAIL 410/004: 5/2 integer division'  :(end)
e004
        differ(2 ** 3, 8)                   :f(e005)
        output = 'FAIL 410/005: 2**3'                  :(end)
e005
        differ('3' + 2, 5)                   :f(e006)
        output = 'FAIL 410/006: string+int coerce'     :(end)
e006
        differ(3 + '-2', 1)                   :f(e007)
        output = 'FAIL 410/007: int+neg-string'        :(end)
e007
        differ('1' + '0', 1)                   :f(e008)
        output = 'FAIL 410/008: string+string coerce'  :(end)
e008
        differ(5 + '', 5)                   :f(e009)
        output = 'FAIL 410/009: null addend is zero'   :(end)
e009
        output = 'PASS 410_arith_int (9/9)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_411_arith_unary()
    {
        var s = @"
        differ(-5, 0 - 5)                   :f(e001)
        output = 'FAIL 411/001: unary minus'           :(end)
e001
        differ(+'4', 4)                   :f(e002)
        output = 'FAIL 411/002: unary plus string->int' :(end)
e002
        output = 'PASS 411_arith_unary (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_412_arith_real()
    {
        var s = @"
        differ(2.0 + 3.0, 5.0)                   :f(e001)
        output = 'FAIL 412/001: 2.0+3.0'              :(end)
e001
        differ(3.0 - 1.0, 2.0)                   :f(e002)
        output = 'FAIL 412/002: 3.0-1.0'              :(end)
e002
        differ(3.0 * 2.0, 6.0)                   :f(e003)
        output = 'FAIL 412/003: 3.0*2.0'              :(end)
e003
        differ(3.0 / 2.0, 1.5)                   :f(e004)
        output = 'FAIL 412/004: 3.0/2.0'              :(end)
e004
        differ(3.0 ** 3, 27.0)                   :f(e005)
        output = 'FAIL 412/005: 3.0**3'               :(end)
e005
        differ(-1.0, 0.0 - 1.0)                   :f(e006)
        output = 'FAIL 412/006: unary minus on real'   :(end)
e006
        output = 'PASS 412_arith_real (6/6)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_413_arith_mixed()
    {
        var s = @"
        differ(1 + 2.0, 3.0)                   :f(e001)
        output = 'FAIL 413/001: int+real promotes to real' :(end)
e001
        differ(3.0 / 2, 1.5)                   :f(e002)
        output = 'FAIL 413/002: real/int promotes to real' :(end)
e002
        output = 'PASS 413_arith_mixed (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_414_remdr()
    {
        var s = @"
        differ(remdr(10, 3), 1)                   :f(e001)
        output = 'FAIL 414/001: remdr(10,3)'           :(end)
e001
        differ(remdr(11, 10), 1)                   :f(e002)
        output = 'FAIL 414/002: remdr(11,10)'          :(end)
e002
        output = 'PASS 414_remdr (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
    [TestMethod]
    public void TEST_Corpus_415_arith_expon()
    {
        var s = @"
        differ(2 ^ 10, 1024)                   :f(e001)
        output = 'FAIL 415/001: 2^10=1024'             :(end)
e001
        differ(3 ^ 3, 27)                   :f(e002)
        output = 'FAIL 415/002: 3^3=27'               :(end)
e002
        differ(2 ^ 0, 1)                   :f(e003)
        output = 'FAIL 415/003: 2^0=1'                :(end)
e003
        output = 'PASS 415_arith_expon (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_416_arith_coerce()
    {
        var s = @"
        differ('5' + '3', 8)                   :f(e001)
        output = 'FAIL 416/001: string coerce add'      :(end)
e001
        differ('10' - '4', 6)                   :f(e002)
        output = 'FAIL 416/002: string coerce sub'      :(end)
e002
        differ('3' * '4', 12)                   :f(e003)
        output = 'FAIL 416/003: string coerce mul'      :(end)
e003
        output = 'PASS 416_arith_coerce (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
}
