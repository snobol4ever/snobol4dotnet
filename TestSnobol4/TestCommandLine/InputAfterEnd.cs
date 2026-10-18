using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// -r gives INPUT the source lines after END, and INPUT fails after the last of them; it never
/// reads stdin. Expected results are SPITBOL's (sbl -b -r).
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputAfterEnd
{
    private const string Script = @"
LOOP    L = INPUT            :F(DONE)
        R = R '[' L ']'      :(LOOP)
DONE    R = R 'done'
END
first after end

third";

    private static string Result(string directives)
    {
        Executive.ReadLineDelegate = () => "stdin";
        try
        {
            var build = SetupTests.SetupScript(directives, Script);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            return ((StringVar)build.Execute!.IdentifierTable["R"]).Data;
        }
        finally { Executive.ReadLineDelegate = null; }
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_InputAfterEnd_dash_r_reads_the_lines_after_end_then_fails()
    {
        Assert.AreEqual("[first after end][][third]done", Result("-b -r"));
    }
}
