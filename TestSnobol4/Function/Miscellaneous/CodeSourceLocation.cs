using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A statement compiled by CODE is in the program's file, on a line of its own after the program's last: each CODE call takes the
/// next line, one that fails to compile too, and EVAL takes none, as SPITBOL counts them. &amp;LINE and &amp;FILE read so, and so does the
/// run report of an error there. The console numbered every CODE text's statements from line 1 of a file named after the compile
/// (CODE2, EVAL2), so &amp;LINE read 3 and &amp;FILE CODE5 (found by measuring the CODE road of DESIGN-PLAN random-codegen-scope step 11).
/// Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeSourceLocation
{
    [TestMethod]
    public void TEST_CodeSourceLocation_each_CODE_is_a_line_after_the_program()
    {
        Assert.AreEqual("a 11 Test.sno\nb 13\nc 14 8 Test.sno\nmain 9", SetupTests.RunWithInput(@"* a comment line
        &ERRLIMIT = 10
        X = EVAL('1 + 1')
        C = CODE(' OUTPUT = ""a "" &LINE "" "" &FILE :(R1)')   :<C>
R1      Y = CODE(' Z = (')
        D = CODE(' OUTPUT = ""b "" &LINE :(R2)')
+                                                 :<D>
R2      E = CODE(' OUTPUT = ""c "" &LINE "" "" &LASTLINE "" "" &FILE :(R3)')   :<E>
R3      OUTPUT = 'main ' &LINE
END").Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_CodeSourceLocation_an_error_in_CODE_is_reported_there()
    {
        // sbl -b reports Test.sno(5), in file Test.sno, in line 5, in statement 6: the second CODE is line 5 of a three-line program
        var report = SetupTests.RunWithInput(@"        C = CODE(' X = 1')
        D = CODE(' OUTPUT = 1 :(L9)')    :<D>
END").Replace("\r\n", "\n");
        StringAssert.Contains(report, "Test.sno(5) : ERROR 038 -- goto undefined label");
        StringAssert.Contains(report, "in file              Test.sno\nin line              5\nin statement         6\n");
    }
}
