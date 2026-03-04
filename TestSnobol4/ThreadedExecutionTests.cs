using Snobol4.Common;
using Test.TestLexer;

namespace Test.Phase4;

/// <summary>
/// Correctness tests for the threaded execution engine.
/// SetupScript now uses UseThreadedExecution=true by default,
/// so these tests simply run programs and check results.
/// </summary>
[TestClass]
public class ThreadedExecutionTests
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    [TestMethod]
    public void Threaded_SimpleAssignment()
    {
        var b = Run("        N = 42\nend");
        Assert.AreEqual("42", b.Execute!.IdentifierTable["N"].ToString());
    }

    [TestMethod]
    public void Threaded_Addition()
    {
        var b = Run("        N = 3 + 4\nend");
        Assert.AreEqual("7", b.Execute!.IdentifierTable["N"].ToString());
    }

    [TestMethod]
    public void Threaded_StringConcat()
    {
        var b = Run("        S = 'Hello' ' ' 'World'\nend");
        Assert.AreEqual("Hello World", b.Execute!.IdentifierTable["S"].ToString());
    }

    [TestMethod]
    public void Threaded_CountTo10()
    {
        var b = Run(@"
        N = 0
LOOP    N = N + 1
        LT(N,10)        :S(LOOP)
        RESULT = N
end");
        Assert.AreEqual("10", b.Execute!.IdentifierTable["RESULT"].ToString());
    }

    [TestMethod]
    public void Threaded_Roman_1776()
    {
        var b = Run(@"
        DEFINE('ROMAN(N)T')                 :(ROMAN_END)
ROMAN   N   RPOS(1)  LEN(1) . T  =         :F(RETURN)
        '0,1I,2II,3III,4IV,5V,6VI,7VII,8VIII,9IX,'
+       T   BREAK(',') . T                  :F(FRETURN)
        ROMAN = REPLACE(ROMAN(N), 'IVXLCDM', 'XLCDM**') T
+                                           :S(RETURN)F(FRETURN)
ROMAN_END
        R1 = ROMAN('1776')
end");
        Assert.AreEqual(0, b.ErrorCodeHistory.Count,
            "Errors: " + string.Join(",", b.ErrorCodeHistory));
        Assert.AreEqual("MDCCLXXVI", b.Execute!.IdentifierTable["R1"].ToString());
    }
}

// Temporarily append a minimal UDF test for diagnosis
EOF
echo "FAIL - not appended correctly"