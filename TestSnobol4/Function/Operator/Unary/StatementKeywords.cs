using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// &amp;STNO counts statements from 1, a blank line being a null statement and a comment none;
/// &amp;LINE is the line in the file; &amp;LASTNO and &amp;LASTLINE name the statement begun
/// before this one, which inside a function is the calling statement. Expected results are
/// SPITBOL's (sbl -b).
/// </summary>
[DoNotParallelize]
[TestClass]
public class StatementKeywords
{
    private static string Run(string script) => SetupTests.RunWithInput(script).Replace("\r\n", "\n");

    [TestMethod]
    public void TEST_StatementKeywords_number_line_and_last()
    {
        Assert.AreEqual("A 2 3 0 0\nB 4 4 3 4\nC 5 5 4 4", Run(@"
* comment line
        OUTPUT = 'A ' &STNO ' ' &LINE ' ' &LASTNO ' ' &LASTLINE
        X = 1 ; OUTPUT = 'B ' &STNO ' ' &LINE ' ' &LASTNO ' ' &LASTLINE
        OUTPUT = 'C ' &STNO ' ' &LINE
+               ' ' &LASTNO ' ' &LASTLINE
end"));
    }

    [TestMethod]
    public void TEST_StatementKeywords_across_a_function_call()
    {
        Assert.AreEqual("F 2 2 3 3\nD 4 4 3 3", Run(@"        DEFINE('F()')  :(EF)
F       OUTPUT = 'F ' &STNO ' ' &LINE ' ' &LASTNO ' ' &LASTLINE  :(RETURN)
EF      F()
        OUTPUT = 'D ' &STNO ' ' &LINE ' ' &LASTNO ' ' &LASTLINE
end"));
    }
}
