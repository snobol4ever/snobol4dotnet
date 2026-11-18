using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// Two edges of SPITBOL's conversion to INTEGER (sbl -bf, probed; gate_cases_snobol4
/// test_gate_sno_convert_reads_a_numeric_string_as_spitbol_gtnum_does__w rows 12nul, 2^31+.5 and real2^31+.5): a real whose truncation is
/// 2147483648 is no integer (2147483648.0 and 2147483648.5 fail; 2147483647.5 is 2147483647 and 2147483649.5 is 2147483649), and a
/// string of digits with a NUL after them is no number (.NET's long.TryParse forgives trailing NULs).
/// </summary>
[DoNotParallelize]
[TestClass]
public class GtnumEdges
{
    private static string Run(string body) => SetupTests.RunWithInputDirectives("-bf", "        &ERRLIMIT = 100\n" + body + "\nEND")
        .Replace("\r\n", "\n").TrimEnd();

    [TestMethod]
    public void TEST_GtnumEdges_a_real_that_truncates_to_2_to_the_31_fails()
    {
        Assert.AreEqual("FAIL FAIL 2147483647 2147483649 FAIL", Run(@"        OUTPUT = (CONVERT(2147483648.0,'INTEGER'), 'FAIL') ' ' (CONVERT(2147483648.5,'INTEGER'), 'FAIL') ' '
+          (CONVERT(2147483647.5,'INTEGER'), 'FAIL') ' ' (CONVERT(2147483649.5,'INTEGER'), 'FAIL') ' '
+          (CONVERT('2147483648.5','INTEGER'), 'FAIL')"));
    }

    [TestMethod]
    public void TEST_GtnumEdges_digits_and_a_nul_are_no_integer()
    {
        Assert.AreEqual("FAIL FAIL", Run(@"        S = '12' CHAR(0)
        OUTPUT = (CONVERT(S,'INTEGER'), 'FAIL') ' ' (S + 0, 'FAIL')"));
    }
}
