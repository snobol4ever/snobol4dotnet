using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The text of an error is sbl's, word for word and comma for comma: it is on the listing page and in &amp;ERRTEXT. Every expected
/// text below is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ErrorText
{
    [TestMethod]
    public void TEST_ErrorText_67_has_the_comma_sbl_puts_after_zero()
    {
        var s = "        &ERRLIMIT = 10\n        A = ARRAY(0)\n        OUTPUT = &ERRTYPE ' ' &ERRTEXT\nEND";
        Assert.AreEqual("67 array dimension is zero, negative or out of range", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_ErrorText_171_has_the_comma_sbl_puts_after_2nd()
    {
        var s = "        &ERRLIMIT = 10\n        X = REPLACE('a', 'ab', 'c')\nEND";
        StringAssert.EndsWith(SetupTests.RunWithInput(s), "Test.sno(2) : ERROR 171 -- null or unequally long 2nd, 3rd args to replace");
    }
}
