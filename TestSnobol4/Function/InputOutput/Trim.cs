using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// &amp;TRIM nonzero removes trailing blanks and tabs from every line INPUT reads, and nothing
/// else; zero keeps them. Expected results are SPITBOL's (sbl -b).
/// </summary>
[DoNotParallelize]
[TestClass]
public class Trim
{
    private const string Show = @"
LOOP    L = INPUT            :F(END)
        OUTPUT = '[' L '] ' SIZE(L)  :(LOOP)
END";

    [TestMethod]
    public void TEST_Trim_1_removes_trailing_blanks_and_tabs_from_input()
    {
        var s = "        &TRIM = 1" + Show;
        Assert.AreEqual("[a] 1\n[b] 1\n[c] 1\n[ d] 2\n[e\f] 2",
            SetupTests.RunWithInput(s, "a  \nb\t\nc \t \n d\ne\f").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trim_0_keeps_trailing_blanks_and_tabs_on_input()
    {
        var s = "        &TRIM = 0" + Show;
        Assert.AreEqual("[a  ] 3\n[b\t] 2",
            SetupTests.RunWithInput(s, "a  \nb\t").Replace("\r\n", "\n"));
    }
}
