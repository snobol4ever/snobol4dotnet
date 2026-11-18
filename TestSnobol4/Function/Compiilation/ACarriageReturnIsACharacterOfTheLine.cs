using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// A line of source ends at a line feed, and only its trailing blanks and tabs are trimmed, as SPITBOL reads it: a carriage return, form
/// feed or vertical tab at the end of a statement is error 230, illegal character, a line that is a carriage return alone is a statement
/// whose label is bad (214), and END followed by a carriage return is no END line. sbl -bf, on X = 1 CR LF, CR LF, OUTPUT = 'ok ' X,
/// Y = 2 FF, OUTPUT = 'y ' Y, END: errors 230 (1,14), 214 (2) and 230 (4,14), and nothing runs. snobol4dotnet read a CR LF file as plain
/// lines and trimmed every white space character, so spitbol_88's DEMOS/TREESORT.spt, a CR LF file, ran where sbl lists 35 errors.
/// </summary>
[TestClass]
public class ACarriageReturnIsACharacterOfTheLine
{
    [TestMethod]
    public void TEST_ACarriageReturnIsACharacterOfTheLine()
    {
        var build = SetupTests.SetupScript("-bf", "        X = 1\r\n\r\n        OUTPUT = 'ok ' X\n        Y = 2\f\n        OUTPUT = 'y ' Y\nEND\n");
        Assert.AreEqual("230 214 230", string.Join(" ", build.ErrorCodeHistory));
    }

    [TestMethod]
    public void TEST_ACarriageReturnIsACharacterOfTheLine_end_then_cr_is_no_end()
    {
        var build = SetupTests.SetupScript("-bf", "        X = 1\nEND\r\n");
        Assert.AreEqual("216", string.Join(" ", build.ErrorCodeHistory));
    }
}
