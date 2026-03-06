using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

[TestClass]
public class Bal
{
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
}
