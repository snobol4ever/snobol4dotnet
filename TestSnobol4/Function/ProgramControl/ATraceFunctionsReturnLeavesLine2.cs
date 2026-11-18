using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// After a trace function returns, &amp;LINE reads 2 until the next statement starts, which then reads 2 as &amp;LASTLINE, as in SPITBOL:
/// trxeq calls the function with the dummy code block trxdc as the code block, retrn sets kvlin from that block's line word (the static
/// word after it, 2 in sbl x64) and trxq1 restores the statement number but not the line. snobol4dotnet read the calling statement's line,
/// so spitbol_88's GOTOS dump said &amp;LASTLINE = 49 where sbl says 2. The expected answers are sbl -bf's.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ATraceFunctionsReturnLeavesLine2
{
    private static string Run(string program) => SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd();

    [TestMethod]
    public void TEST_ATraceFunctionsReturnLeavesLine2_a_keyword_trace()
    {
        Assert.AreEqual("4 2 3 8\n4 2 3 10", Run(@"* c1
        DEFINE('T()')   :(TE)
T       N = N + 1
        M = M + 1     :(RETURN)
TE      TRACE('STCOUNT', 'KEYWORD', , 'T')
        &TRACE = 1000
        X = 1
        Y = 2
        OUTPUT = &LASTLINE ' ' &LINE ' ' &LASTNO ' ' &STNO
        Z = 3
        OUTPUT = &LASTLINE ' ' &LINE ' ' &LASTNO ' ' &STNO
END"));
    }

    [TestMethod]
    public void TEST_ATraceFunctionsReturnLeavesLine2_a_value_trace()
    {
        Assert.AreEqual("2 9 7 8\n2 10 9 10", Run(@"* c1
        DEFINE('T()')   :(TE)
T       N = N + 1
        M = M + 1     :(RETURN)
TE      TRACE('V', 'VALUE', , 'T')
        &TRACE = 1000
        X = 1
        V = 2
        OUTPUT = &LASTLINE ' ' &LINE ' ' &LASTNO ' ' &STNO
        V = 3 ; OUTPUT = &LASTLINE ' ' &LINE ' ' &LASTNO ' ' &STNO
END"));
    }
}
