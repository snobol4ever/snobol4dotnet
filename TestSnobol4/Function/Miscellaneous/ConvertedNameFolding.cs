using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// While &amp;CASE folds, the text of a string or a real converted to a NAME is folded to upper case, as names are, in SPITBOL: CONVERT('abc',
/// 'NAME') is .ABC, so $CONVERT('abc', 'NAME') reads the variable abc = 5 set, and the name of 1.0E-10 is 0.1E-9. The text was kept as
/// written, naming another variable. Found beside the real-name-text class. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ConvertedNameFolding
{
    [TestMethod]
    public void TEST_ConvertedNameFolding_follows_case()
    {
        var program = @"        abc = 5
        OUTPUT = 'x' CONVERT('abc', 'NAME')
        OUTPUT = $CONVERT('abc', 'NAME')
        OUTPUT = 'x' CONVERT(1.0E-10, 'NAME')
        OUTPUT = 'x' CONVERT('Mixed', 'NAME')
        &CASE = 0
        OUTPUT = 'y' CONVERT('Def', 'NAME')
        &CASE = 1
        OUTPUT = 'z' CONVERT('Def', 'NAME')
END";
        Assert.AreEqual(@"xABC
5
x0.1E-9
xMIXED
yDef
zDEF", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
