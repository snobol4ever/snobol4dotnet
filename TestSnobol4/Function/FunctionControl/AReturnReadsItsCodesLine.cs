using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A function returns to the code that called it, and &amp;LINE then reads that code's line (SPITBOL's retrn sets it from the code
/// block's line word): a call in a statement returns to the statement's line, one in a deferred expression to the line the expression
/// was written on, whichever statement runs it, and one in the text of EVAL to the line EVAL's code is compiled at, the program's last
/// (sbl -bf, below: 3, 11 and 9). snobol4dotnet read the line of the statement running each (rosetta-snobol4 leap-year, graded by its
/// dump, said &amp;LASTLINE = 12 where sbl says 13).
/// </summary>
[TestClass]
public class AReturnReadsItsCodesLine
{
    [TestMethod]
    public void TEST_A_return_reads_the_line_of_the_code_that_called()
    {
        var build = SetupTests.SetupScript("-bf", @" DEFINE('F()') :(E)
F :(RETURN)
E Z = *F() 'x'

 A = ('x' ? Z) &LINE
 B = EVAL('F()') &LINE

 C = F() &LINE

 D = EVAL('1') &LINE
END");
        var t = build.Execute!.IdentifierTable;
        static string Text(Var v) => v is IntegerVar i ? i.Data.ToString() : ((StringVar)v).Data;
        Assert.AreEqual("x3", Text(t["A"]), "deferred");
        Assert.AreEqual("11", Text(t["B"]), "EVAL");
        Assert.AreEqual("8", Text(t["C"]), "a statement");
        Assert.AreEqual("110", Text(t["D"]), "no call");
    }
}
