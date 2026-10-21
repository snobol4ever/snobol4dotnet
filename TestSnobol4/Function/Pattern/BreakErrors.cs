using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// The error numbers of the BREAK and BREAKX arguments, as sbl -b gives them for the same programs (&amp;ERRTYPE after the
/// statement, with &amp;ERRLIMIT letting the run go on). BREAK(*S) takes its argument when the pattern is matched, and a value
/// that is no string is 44; BREAKX(E) with a null string given when the pattern is built is 70 ("breakx argument is not a
/// string or expression"), 45 being the one for an argument evaluated at match time.
/// </summary>
[DoNotParallelize]
[TestClass]
public class BreakErrors
{
    [TestMethod]
    public void TEST_BreakErrors_break_of_an_evaluated_pattern_is_44()
    {
        var s = "        &ERRLIMIT = 10\n        S = LEN(1)\n        'abc' ? BREAK(*S)\n        OUTPUT = 'evaluated ' &ERRTYPE\nEND";
        Assert.AreEqual("evaluated 44", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_BreakErrors_break_of_an_evaluated_null_string_is_44()
    {
        var s = "        &ERRLIMIT = 10\n        E = ''\n        'abc' ? BREAK(*E)\n        OUTPUT = 'null ' &ERRTYPE\nEND";
        Assert.AreEqual("null 44", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_BreakErrors_break_of_a_null_string_is_69()
    {
        var s = "        &ERRLIMIT = 10\n        E = ''\n        P = BREAK(E)\n        OUTPUT = 'built ' &ERRTYPE\nEND";
        Assert.AreEqual("built 69", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_BreakErrors_breakx_of_an_evaluated_pattern_is_45()
    {
        var s = "        &ERRLIMIT = 10\n        S = LEN(1)\n        'abc' ? BREAKX(*S)\n        OUTPUT = 'evaluated ' &ERRTYPE\nEND";
        Assert.AreEqual("evaluated 45", SetupTests.RunWithInput(s));
    }
}
