using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

[DoNotParallelize]
[TestClass]
public class Rung10_Functions
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
            var old = Console.Error;
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms) { AutoFlush = true };
            Console.SetError(sw);
            try { SetupTests.SetupScript("-b", script); }
            finally { Console.SetError(old); }
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
    public void TEST_Corpus_1010_func_recursion()
    {
        var s = @"
        define('fact(n)')                              :(fact_end)
fact    fact = eq(n, 1) 1                              :s(return)
        fact = n * fact(n - 1)                         :(return)
fact_end
        ne(fact(5), 120)           :f(e001)
        output = 'FAIL 1010/001: fact(5)=120'          :(end)
e001
        differ(opsyn(.facto, 'fact'))                   :f(e002)
        output = 'FAIL 1010/002: opsyn alias'          :(end)
e002
        ne(facto(4), 24)           :f(e003)
        output = 'FAIL 1010/003: facto(4)=24 via alias' :(end)
e003
        define('fact2(n)', .fact2_entry)               :(fact2_end)
fact2_entry
        fact2 = eq(n, 1) 1                             :s(return)
        fact2 = n * fact2(n - 1)                       :(return)
fact2_end
        ne(fact2(6), 720)          :f(e004)
        output = 'FAIL 1010/004: fact2(6)=720 alt entry' :(end)
e004
        output = 'PASS 1010_func_recursion (4/4)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1011_func_redefine()
    {
        var s = @"
        define('myfunc(n)')                            :(myfunc_end)
myfunc  myfunc = n * 2                                 :(return)
myfunc_end
        ne(myfunc(3), 6)           :f(e001)
        output = 'FAIL 1011/001: first definition myfunc(3)=6' :(end)
e001
        differ(define('myfunc(myfunc)', 'myfunc2'))                   :f(e002)
        output = 'FAIL 1011/002: define returns function name' :(end)
e002    :(myfunc2_end)
myfunc2 myfunc = ne(myfunc, 1) myfunc * myfunc(myfunc - 1) :(return)
myfunc2_end
        ne(myfunc(4), 24)          :f(e003)
        output = 'FAIL 1011/003: redefined myfunc(4)=24'   :(end)
e003
        output = 'PASS 1011_func_redefine (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1012_func_locals()
    {
        var s = @"
        define('lfunc(a,b,c)d,e,f')               :(lfunc_end)
lfunc
        ident(a, 'p')              :s(e001)
        output = 'FAIL 1012/001: arg a should be p'    :(end)
e001    ident(b, 'q')              :s(e002)
        output = 'FAIL 1012/002: arg b should be q'    :(end)
e002
        differ(d)                  :f(e003)
        output = 'FAIL 1012/003: local d should be null' :(end)
e003
        a = 'aa' ; b = 'bb' ; d = 'dd'
        lfunc = a b d                                  :(return)
lfunc_end
        a = 'global_a'
        d = 'global_d'
        differ(lfunc('p', 'q', 'r'), 'aabbdd')                   :f(e004)
        output = 'FAIL 1012/004: lfunc return value'   :(end)
e004
        ident(a, 'global_a')                           :s(e005)
        output = 'FAIL 1012/005: global a not clobbered' :(end)
e005
        ident(d, 'global_d')                           :s(e006)
        output = 'FAIL 1012/006: global d not clobbered' :(end)
e006
        define('checklocal()x')                        :(cl_end)
checklocal
        differ(x)                  :f(e007_inner)
        checklocal = 'local-not-null'                  :(return)
e007_inner
        checklocal =                                   :(return)
cl_end
        differ(checklocal())       :f(e007)
        output = 'FAIL 1012/007: local null on fresh call' :(end)
e007
        output = 'PASS 1012_func_locals (7/7)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1013_func_nreturn()
    {
        var s = @"
        define('ref_a()')                              :(ref_a_end)
ref_a   ref_a = .a                                     :(nreturn)
ref_a_end
        a = 27
        differ(ref_a(), 27)                            :f(e001)
        output = 'FAIL 1013/001: nreturn read gives value' :(end)
e001
        ref_a() = 26                                   :s(e002)
        output = 'FAIL 1013/002: nreturn lvalue assign failed' :(end)
e002
        differ(a, 26)                                  :f(e003)
        output = 'FAIL 1013/003: a updated via nreturn'   :(end)
e003
        output = 'PASS 1013_func_nreturn (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1014_func_freturn()
    {
        var s = @"
        define('always_fail()')                        :(af_end)
always_fail                                            :(freturn)
af_end
        always_fail()                                  :f(e001)
        output = 'FAIL 1014/001: freturn should cause statement failure' :(end)
e001
        output = 'PASS 1014_func_freturn (1/1)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1015_opsyn()
    {
        var s = @"
        opsyn('@', .dupl, 2)
        differ('a' @ 4, 'aaaa')                   :f(e001)
        output = 'FAIL 1015/001: @ as binary dupl'     :(end)
e001
        opsyn('|', .size, 1)
        differ(|'string', 6)                   :f(e002)
        output = 'FAIL 1015/002: | as unary size'      :(end)
e002
        output = 'PASS 1015_opsyn (2/2)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1016_eval()
    {
        var s = @"
        expr = *('abc' 'def')
        differ(eval(expr), 'abcdef')                   :f(e001)
        output = 'FAIL 1016/001: eval concat expr'     :(end)
e001
        q = 'qqq'
        sexp = *q
        differ(eval(sexp), 'qqq')                   :f(e002)
        output = 'FAIL 1016/002: eval var ref'         :(end)
e002
        fexp = *ident(1, 2)
        eval(fexp)                                     :f(e003)
        output = 'FAIL 1016/003: eval failing expr should fail' :(end)
e003
        output = 'PASS 1016_eval (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1017_arg_local()
    {
        var s = @"
jlab    define('jlab(a,b,c)d,e,f')
        differ(arg(.jlab, 1), 'A')                   :f(e001)
        output = 'FAIL 1017/001: arg(.jlab,1) = A'     :(end)
e001
        differ(arg(.jlab, 3), 'C')                   :f(e002)
        output = 'FAIL 1017/002: arg(.jlab,3) = C'     :(end)
e002
        arg(.jlab, 0)              :f(e003)
        output = 'FAIL 1017/003: arg(.jlab,0) OOB should fail' :(end)
e003
        arg(.jlab, 4)              :f(e004)
        output = 'FAIL 1017/004: arg(.jlab,4) OOB should fail' :(end)
e004
        differ(local(.jlab, 1), 'D')                   :f(e005)
        output = 'FAIL 1017/005: local(.jlab,1) = D'   :(end)
e005
        differ(local(.jlab, 3), 'F')                   :f(e006)
        output = 'FAIL 1017/006: local(.jlab,3) = F'   :(end)
e006
        local(.jlab, 0)            :f(e007)
        output = 'FAIL 1017/007: local(.jlab,0) OOB should fail' :(end)
e007
        local(.jlab, 4)            :f(e008)
        output = 'FAIL 1017/008: local(.jlab,4) OOB should fail' :(end)
e008
        output = 'PASS 1017_arg_local (8/8)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }

    [TestMethod]
    public void TEST_Corpus_1018_apply()
    {
        var s = @"
        apply(.eq, 1, 2)           :f(e001)
        output = 'FAIL 1018/001: apply(.eq,1,2) should fail' :(end)
e001
        apply(.eq, 1, 1)           :s(e002)
        output = 'FAIL 1018/002: apply(.eq,1,1) should succeed' :(end)
e002
        differ(apply(.trim, 'abc   '), 'abc')                   :f(e003)
        output = 'FAIL 1018/003: apply(.trim,...)'     :(end)
e003
        output = 'PASS 1018_apply (3/3)'
end";
        var lines = RunGetOutput(s);
        Assert.IsTrue(lines.Count > 0);
        Assert.IsTrue(lines[^1].StartsWith("PASS"), $"Expected PASS, got: {lines[^1]}");
    }
}
