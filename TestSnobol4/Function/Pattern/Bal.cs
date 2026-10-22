using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Bal
{
    [TestMethod]
    public void TEST_Bal_keyword_is_the_balanced_pattern()
    {
        // sbl -b: m1 (A(B)C), m2 (A)(B), m3 () and m4 AB. The &BAL keyword was built as the bare holder of
        // BAL's parts and ended the run in an InvalidCastException when a match reached it.
        var build = SetupTests.SetupScript("-b", @"
        '(A(B)C)D' ? &BAL . m1 'D'
        '(A)(B)C' ? POS(0) &BAL . m2 'C'
        '()(' ? POS(0) &BAL . m3 RPOS(1)
        'AB)' ? POS(0) &BAL . m4 ')'
end
");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("(A(B)C)", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("m1")]).Data);
        Assert.AreEqual("(A)(B)", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("m2")]).Data);
        Assert.AreEqual("()", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("m3")]).Data);
        Assert.AreEqual("AB", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("m4")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_001()
    {
        var testFile = Path.GetTempFileName();
        var testFile2 = Path.GetTempFileName();
        try
        {
            var s = $"""
                             output('print', '2', '{testFile}', 2, 3, 1)
                             &anchor = 0
                             subject = '((A+(B*C))+D)'
                             pattern = bal $ print fail
                             subject pattern
                             endfile('2')

                             input('read',3,'{testFile}')
                     loop    line = read ';'      :f(close)
                             lines = lines line   :(loop)
                     close   endfile(3)
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(
                "((A+(B*C))+D);(A+(B*C));(A+(B*C))+;(A+(B*C))+D;A;A+;A+(B*C);+;+(B*C);(B*C);B;B*;B*C;*;*C;C;+;+D;D;",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("lines")]).Data);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(testFile2);
        }
    }

    [TestMethod]
    public void TEST_Bal_002()
    {
        var testFile = Path.GetTempFileName();
        var testFile2 = Path.GetTempFileName();
        try
        {
            var s = $"""
                             OUTPUT('PRINT', '2', '{testFile}', 2, 3, 1)
                             &ANCHOR = 0
                             SUBJECT = '((A+(B*C))+D)'
                             PATTERN = BAL $ PRINT FAIL
                             SUBJECT PATTERN
                             ENDFILE('2')

                             INPUT('READ',3,'{testFile}')
                     LOOP    LINE = OUTPUT = READ ';'      :F(CLOSE)
                             LINES = LINES LINE   :(LOOP)
                     CLOSE   ENDFILE(3)
                     end
                     """;
            var directives = "-b";
            var build = SetupTests.SetupScript(directives, s);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual(
                "((A+(B*C))+D);(A+(B*C));(A+(B*C))+;(A+(B*C))+D;A;A+;A+(B*C);+;+(B*C);(B*C);B;B*;B*C;*;*C;C;+;+D;D;",
                ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("LINES")]).Data);
        }
        finally
        {
            File.Delete(testFile);
            File.Delete(testFile2);
        }
    }
    [TestMethod]
    public void TEST_Bal_003_nested()
    {
        // BAL matches balanced nested parens
        var s = @"
        subject = '(a(b)c)rest'
        subject bal . matched   :f(bad)
        result = matched   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // BAL matches at least 1 balanced char
        Assert.AreNotEqual("bad",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }
    [TestMethod]
    public void TEST_Bal_004_simple_char()
    {
        // BAL matches a single non-paren char
        var s = @"
        subject = 'x'
        subject bal . cap   :f(bad)
        result = cap   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("x",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_005_in_expr()
    {
        // BAL used to parse a balanced expression token
        var s = @"
        subject = 'a+b'
        subject bal . tok   :f(bad)
        differ(tok, '')   :f(bad)
        result = 'ok'   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok",
            ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_006_empty_parens()
    {
        // BAL matches '()' as a balanced unit
        var s = @"
        subject = '()'
        subject bal . cap   :f(bad)
        result = cap   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("()", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("cap")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_007_unbalanced_fails()
    {
        // BAL does NOT match unbalanced open paren by itself
        var s = @"
        &anchor = 1
        subject = '(abc'
        subject bal rpos(0)   :s(bad)
        result = 'ok'   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ok", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_008_preceded_by_lit()
    {
        // BAL after a literal — confirms position context
        var s = @"
        &anchor = 0
        subject = 'x(a+b)y'
        subject '(' bal . inner ')'   :f(bad)
        result = inner   :(end)
bad     result = 'bad'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("a+b", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("inner")]).Data);
    }
}
