using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// TRACE's CALL, RETURN, FUNCTION, LABEL and KEYWORD events, and &amp;FTRACE, print SPITBOL's lines on
/// standard output; only program-defined functions and &amp;ERRTYPE, &amp;FNCLEVEL and &amp;STCOUNT can be
/// traced; the name is folded. Each expectation is sbl -b's output for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceEvents
{

    [TestMethod]
    public void TEST_Trace_Access_check_takes_an_empty_name()
    {
        // Growing the variable slot array reads every slot through the identifier table, an empty-named one
        // included, and the access-trace check indexed name[0]; snoflake's eliza-modernized crashed there
        // (IndexOutOfRangeException) where sbl -bf runs to the end
        var build = SetupTests.SetupScript("-b", @"
        X = 1
end");
        build.Execute!.TraceIdentifierAccess("", StringVar.Null());
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }
}
