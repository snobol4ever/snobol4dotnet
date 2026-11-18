using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// The column of a compilation error, and the blanks before its pointer, count the bytes of the line in UTF-8, as sbl -bf counts them:
/// X = '┌' ` (a character of three bytes) is ERROR 230 at (1,19), Y = 'éé' + is ERROR 221 at (2,20). snobol4dotnet counted characters,
/// (1,17) and (2,18), so snolib's g360.spt, whose strings hold box-drawing characters, named its errors two columns short.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AnErrorColumnCountsBytes
{
    [TestMethod]
    public void TEST_AnErrorColumnCountsBytes()
    {
        var output = SetupTests.RunWithInputDirectives("-bf", "        X = '┌' `\n        Y = 'éé' +\nEND\n").Replace("\r\n", "\n");
        StringAssert.Contains(output, "(1,19) : ERROR 230");
        StringAssert.Contains(output, "(2,20) : ERROR 221");
        StringAssert.Contains(output, "\n" + new string(' ', 26) + "!\n");
    }
}
