using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

[DoNotParallelize]
[TestClass]
public class Rung3_Concat
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
    public void TEST_Corpus_310_concat_strings()
    {
        var s = @"
        differ('a' 'b', 'ab')                   :f(e001)
        output = 'FAIL 310/001: two-string concat'      :(end)
e001
        differ('a' 'b' 'c', 'abc')                   :f(e002)
        output = 'FAIL 310/002: three-string concat'    :(end)
e002
        differ(('hello' ' ') 'world', 'hello world')                   :f(e003)
        output = 'FAIL 310/003: left-associative concat' :(end)
e003
        output = 'PASS 310_concat_strings (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_311_concat_numeric()
    {
        var s = @"
        differ(1 2, '12')                   :f(e001)
        output = 'FAIL 311/001: int int concat'         :(end)
e001
        differ(2 2 2, '222')                   :f(e002)
        output = 'FAIL 311/002: three int concat'       :(end)
e002
        differ(1 3.4, '13.4')                   :f(e003)
        output = 'FAIL 311/003: int real concat'        :(end)
e003
        output = 'PASS 311_concat_numeric (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_312_concat_null()
    {
        var s = @"
        x = 'hello'
        differ(x '', x)                   :f(e001)
        output = 'FAIL 312/001: null right identity'    :(end)
e001
        differ('' x, x)                   :f(e002)
        output = 'FAIL 312/002: null left identity'     :(end)
e002
        output = 'PASS 312_concat_null (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
    [TestMethod]
    public void TEST_Corpus_313_concat_mixed_types()
    {
        var s = @"
        differ(1 ' ' 2.5, '1 2.5')                   :f(e001)
        output = 'FAIL 313/001: int-space-real concat'   :(end)
e001
        differ('' '' '', '')                   :f(e002)
        output = 'FAIL 313/002: triple null concat'      :(end)
e002
        output = 'PASS 313_concat_mixed (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_314_concat_long()
    {
        var s = @"
        a = 'hello'
        b = ' '
        c = 'world'
        r = a b c
        differ(r, 'hello world')                   :f(e001)
        output = 'FAIL 314/001: 3-part concat via vars'   :(end)
e001
        differ(size(r), 11)                   :f(e002)
        output = 'FAIL 314/002: size of concat result'    :(end)
e002
        output = 'PASS 314_concat_long (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_315_concat_in_pattern()
    {
        // Concatenation used to build a pattern from parts
        var s = @"
        a = 'hel'
        b = 'lo'
        pat = a b
        'hello world' pat   :f(fail)
        output = 'PASS 315_concat_in_pattern'   :(end)
fail    output = 'FAIL 315'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_316_concat_with_integer()
    {
        // Concatenating integer to string coerces int to string
        var s = @"
        n = 42
        r = 'answer=' n
        ident(r, 'answer=42')   :s(ok)f(fail)
ok      output = 'PASS 316_concat_with_integer'   :(end)
fail    output = 'FAIL 316: r=' r
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_317_concat_in_loop()
    {
        // Accumulate string by concatenation in a loop
        var s = @"
        n = 0
loop    n = n + 1
        acc = acc 'x'
        differ(n, 5)   :f(loop)
        differ(acc, 'xxxxx')   :f(fail)
        output = 'PASS 317_concat_in_loop'   :(end)
fail    output = 'FAIL 317'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
}
