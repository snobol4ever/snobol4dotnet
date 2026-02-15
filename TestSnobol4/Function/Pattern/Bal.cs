using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Bal
{

    [TestMethod]
    public void TEST_Bal_001()
    {

        var testFile = Path.Combine(SetupTests.WindowsOutput, "BalTest.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "BalTest.txt");

        var s = $"""
                         output('print', '2', '{testFile}', 2, 3, 1)
                         &anchor = 0
                         subject = '((A+(B*C))+D)'
                         pattern = bal $ print fail
                         subject pattern
                         endfile('2')

                         input('READ',3,'{testFile}')
                 loop    line = read ';'      :f(close)
                         lines = lines line   :(loop)
                 close   endfile(3)
                 end
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("((A+(B*C))+D);(A+(B*C));(A+(B*C))+;(A+(B*C))+D;A;A+;A+(B*C);+;+(B*C);(B*C);B;B*;B*C;*;*C;C;+;+D;D;", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","lines")]).Data);
    }

    [TestMethod]
    public void TEST_Bal_002()
    {
        var testFile = Path.Combine(SetupTests.WindowsOutput, "BalTest2.txt");
        if (SetupTests.IsLinux)
            testFile = Path.Combine(SetupTests.LinuxOutput, "BalTest2.txt");

        var s = $"""
                         OUTPUT('print', '2', '{testFile}', 2, 3, 1)
                         &ANCHOR = 0
                         SUBJECT = '((A+(B*C))+D)'
                         PATTERN = BAL $ PRINT FAIL
                         SUBJECT PATTERN
                         ENDFILE('2')

                         INPUT('READ',3,'{testFile}')
                 LOOP    LINE = OUTPUT = READ ';'      :F(CLOSE)
                         LINES = LINES LINE   :(LOOP)
                 CLOSE   ENDFILE(3)
                 END
                 """;
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("((A+(B*C))+D);(A+(B*C));(A+(B*C))+;(A+(B*C))+D;A;A+;A+(B*C);+;+(B*C);(B*C);B;B*;B*C;*;*C;C;+;+D;D;", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","LINES")]).Data);
    }
}