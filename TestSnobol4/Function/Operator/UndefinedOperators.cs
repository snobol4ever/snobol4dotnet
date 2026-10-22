using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// The binary operators SPITBOL leaves for a program to define (# % &amp; @ ~) are error 29, "undefined operator referenced", until OPSYN
/// gives them a function. ~ took a pattern and a label and gave the pattern back (a parse-tree annotation of an older beauty.sno): sbl
/// has no such operator. Run under sbl -bf.
/// </summary>
[DoNotParallelize]
[TestClass]
public class UndefinedOperators
{
    [TestMethod]
    public void TEST_UndefinedOperators_a_binary_operator_nobody_defined_is_error_29()
    {
        foreach (var op in new[] { "#", "%", "&", "@", "~" })
        {
            var build = SetupTests.SetupScript("-b", $"        X = 'a' {op} 'b'\nEND");
            Assert.AreEqual(29, build.ErrorCodeHistory[0], op);
        }
    }

    [TestMethod]
    public void TEST_UndefinedOperators_a_binary_tilde_of_two_patterns_is_error_29_too()
    {
        var build = SetupTests.SetupScript("-b", "        X = LEN(1) ~ 'label'\nEND");
        Assert.AreEqual(29, build.ErrorCodeHistory[0]);
    }

    // the caret is the exclamation mark, and unary ! is undefined: ^X was a NullReferenceException that ended the run
    [TestMethod]
    public void TEST_UndefinedOperators_a_unary_caret_is_error_29()
    {
        var build = SetupTests.SetupScript("-b", "        X = ^Y\nEND");
        Assert.AreEqual(29, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_UndefinedOperators_opsyn_of_the_unary_exclamation_defines_the_unary_caret()
    {
        Assert.AreEqual("3\n4", SetupTests.RunWithInput("        OPSYN('!','SIZE',1)\n        OUTPUT = ^'abc'\n        OUTPUT = !'abcd'\nEND").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_UndefinedOperators_opsyn_defines_the_binary_tilde()
    {
        Assert.AreEqual("[   ab]", SetupTests.RunWithInput("        OPSYN('~','LPAD',2)\n        X = 'ab' ~ 5\n        OUTPUT = '[' X ']'\nEND"));
    }
}
