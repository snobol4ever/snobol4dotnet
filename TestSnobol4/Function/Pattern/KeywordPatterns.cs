using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// The pattern keywords (&ABORT &ARB &BAL &FAIL &FENCE &REM &SUCCEED) hold the very pattern their variables are first given, so IDENT(&ARB, ARB)
/// succeeds, and &ARB matches as ARB does: '1' ? &ARB matches the null string first. &ARB was a bare ArbPattern that matched '1', and every
/// keyword's pattern was a second copy, not the variable's. Found by the infinite_snobol4 generator. Every expected line is what sbl -bf printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class KeywordPatterns
{
    [TestMethod]
    public void TEST_KeywordPatterns_each_keyword_is_identical_to_its_variable()
    {
        var program = @"        OUTPUT = 'ABORT ' DATATYPE(&ABORT) ' ' (IDENT(&ABORT, ABORT) 'same', 'different')
        OUTPUT = 'ARB ' DATATYPE(&ARB) ' ' (IDENT(&ARB, ARB) 'same', 'different')
        OUTPUT = 'BAL ' DATATYPE(&BAL) ' ' (IDENT(&BAL, BAL) 'same', 'different')
        OUTPUT = 'FAIL ' DATATYPE(&FAIL) ' ' (IDENT(&FAIL, FAIL) 'same', 'different')
        OUTPUT = 'FENCE ' DATATYPE(&FENCE) ' ' (IDENT(&FENCE, FENCE) 'same', 'different')
        OUTPUT = 'REM ' DATATYPE(&REM) ' ' (IDENT(&REM, REM) 'same', 'different')
        OUTPUT = 'SUCCEED ' DATATYPE(&SUCCEED) ' ' (IDENT(&SUCCEED, SUCCEED) 'same', 'different')
END";
        Assert.AreEqual(@"ABORT PATTERN same
ARB PATTERN same
BAL PATTERN same
FAIL PATTERN same
FENCE PATTERN same
REM PATTERN same
SUCCEED PATTERN same", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_KeywordPatterns_each_keyword_matches_as_its_variable_does()
    {
        var program = @"        V = '1' ? ARB
        OUTPUT = '1 [' V ']'
        V = '1' ? &ARB
        OUTPUT = '2 [' V ']'
        V = 'abc' ? &ARB 'c'
        OUTPUT = '3 [' V ']'
        V = '1' ? &BAL
        OUTPUT = '5 [' V ']'
        V = 'xy' ? &REM
        OUTPUT = '6 [' V ']'
        V = 'xy' ? &FAIL                            :S(Y7)
        OUTPUT = '7 failed'                         :(E7)
Y7      OUTPUT = '7 [' V ']'
E7      V = 'xy' ? &SUCCEED
        OUTPUT = '8 [' V ']'
        V = 'xy' ? (&FENCE 'x')
        OUTPUT = '9 [' V ']'
END";
        Assert.AreEqual(@"1 []
2 []
3 [abc]
5 [1]
6 [xy]
7 failed
8 []
9 [x]", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
