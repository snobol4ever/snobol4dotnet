using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// A first line of the program that begins #! is passed over and not counted, as sbl -bf reads it: #!x, X = 1 + is ERROR 221 at (1,8),
/// and #!x, OUTPUT = &amp;LINE ' ' &amp;STNO prints 1 1; a later #! line, or one in an included file, is a statement with a bad label (214).
/// snobol4dotnet answered 214 at line 1, so tkslide_examples take_note.sno (#! /usr/local/bin/snobol4 -b) ran nothing where sbl runs it.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AHashBangFirstLineIsPassedOver
{
    [TestMethod]
    public void TEST_AHashBangFirstLineIsPassedOver()
    {
        Assert.AreEqual("1 1", SetupTests.RunWithInputDirectives("-bf", "#!x\n        OUTPUT = &LINE ' ' &STNO\nEND\n").Replace("\r\n", "\n").TrimEnd());
    }

    [TestMethod]
    public void TEST_AHashBangFirstLineIsPassedOver_not_counted()
    {
        StringAssert.Contains(SetupTests.RunWithInputDirectives("-bf", "#!x\n X = 1 +\nEND\n"), "(1,8) : ERROR 221");
    }

    [TestMethod]
    public void TEST_AHashBangFirstLineIsPassedOver_only_the_first()
    {
        Assert.AreEqual("214", string.Join(" ", SetupTests.SetupScript("-bf", "        X = 1\n#!x\nEND\n").ErrorCodeHistory));
    }
}
