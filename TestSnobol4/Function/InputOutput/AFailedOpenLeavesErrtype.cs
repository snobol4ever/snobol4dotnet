using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// INPUT and OUTPUT of a file that cannot be opened fail and set nothing: &amp;ERRTYPE keeps what it held (sbl -bf: 0 in a program
/// with no error before, 1 after a trapped error 001). snobol4dotnet set it to 117 and 160, which a termination dump shows
/// (rosetta-snobol4 read-entire-file and read-a-file-line-by-line, spitbol_88 DEMOS/KEYWORD, each graded by its dump).
/// </summary>
[TestClass]
public class AFailedOpenLeavesErrtype
{
    [TestMethod]
    public void TEST_A_failed_open_sets_no_errtype()
    {
        var build = SetupTests.SetupScript("-bf", @"        INPUT(.X, 21, 'no-such-file-afailedopen.txt')  :S(END)
        A = &ERRTYPE
        OUTPUT(.Y, 22, '/no-such-dir-afailedopen/f')    :S(END)
        B = &ERRTYPE
END");
        Assert.AreEqual(0L, ((IntegerVar)build.Execute!.IdentifierTable["A"]).Data);
        Assert.AreEqual(0L, ((IntegerVar)build.Execute!.IdentifierTable["B"]).Data);
    }

    [TestMethod]
    public void TEST_A_failed_open_keeps_the_errtype_before_it()
    {
        var build = SetupTests.SetupScript("-bf", @"        &ERRLIMIT = 10
        V = 'a'
        X = V + 1
        INPUT(.X, 21, 'no-such-file-afailedopen.txt')  :S(END)
        A = &ERRTYPE
        OUTPUT(.Y, 22, '/no-such-dir-afailedopen/f')    :S(END)
        B = &ERRTYPE
END");
        Assert.AreEqual(1L, ((IntegerVar)build.Execute!.IdentifierTable["A"]).Data);
        Assert.AreEqual(1L, ((IntegerVar)build.Execute!.IdentifierTable["B"]).Data);
    }
}
