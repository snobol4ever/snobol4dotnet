using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// Where an integer is required, a real, or a string that reads as a number, converts to an
/// integer by truncation; the INTEGER predicate still answers only for integers. Expected results
/// are SPITBOL's (sbl -b; Budne's intval).
/// </summary>
[TestClass]
public class RealAsInteger
{
    private static string Result(string body)
    {
        var build = SetupTests.SetupScript("-b", body + "\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data;
    }

    [TestMethod]
    public void TEST_RealAsInteger_a_keyword_takes_a_real_or_a_numeric_string()
    {
        Assert.AreEqual("1 1", Result(@"
        &TRIM = 1.0
        R = &TRIM
        &TRIM = '1.0'
        R = R ' ' &TRIM"));
    }

    [TestMethod]
    public void TEST_RealAsInteger_a_function_argument_takes_a_real_string()
    {
        Assert.AreEqual("A ***** A 65", Result(@"
        R = CHAR('65.0') ' ' DUPL('*', '5.9999') ' ' CHAR('6.5E1') ' ' CONVERT('65.0', 'INTEGER')"));
    }

    [TestMethod]
    public void TEST_RealAsInteger_a_subscript_and_an_array_bound_take_reals()
    {
        Assert.AreEqual("two 3", Result(@"
        A = ARRAY(5)
        A<'2.0'> = 'two'
        R = A<2> ' ' PROTOTYPE(ARRAY(3.5))"));
    }

    [TestMethod]
    public void TEST_RealAsInteger_the_integer_predicate_stays_strict()
    {
        Assert.AreEqual("no no yes", Result(@"
        R = (INTEGER('65.0') 'yes', 'no') ' ' (INTEGER(65.0) 'yes', 'no') ' ' (INTEGER('65') 'yes', 'no')"));
    }
}
