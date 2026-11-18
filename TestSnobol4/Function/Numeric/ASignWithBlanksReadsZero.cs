using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// A string that is a sign followed by one or more blanks and nothing else ('- ', ' - ', '+ ') reads as 0 for INTEGER, REAL and
/// arithmetic, as SPITBOL's gtnum reads it; a sign alone ('-') or a sign at the end (' -') does not (sbl -bf, probed; gate_cases_snobol4
/// test_gate_sno_convert_reads_a_numeric_string_as_spitbol_gtnum_does__w rows -sp, sp-sp and +sp). snobol4dotnet failed all three.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ASignWithBlanksReadsZero
{
    private static string Run(string s) => SetupTests.RunWithInputDirectives("-bf", $@"        &ERRLIMIT = 100
        S = '{s}'
        OUTPUT = (CONVERT(S,'INTEGER') ' ', 'FAIL ') (CONVERT(S,'REAL') ' ', 'FAIL ') (S + 0, 'FAIL')
END").Replace("\r\n", "\n").TrimEnd();

    [TestMethod] public void TEST_ASignWithBlanksReadsZero_minus_blank() => Assert.AreEqual("0 0. 0", Run("- "));
    [TestMethod] public void TEST_ASignWithBlanksReadsZero_blank_minus_blank() => Assert.AreEqual("0 0. 0", Run(" - "));
    [TestMethod] public void TEST_ASignWithBlanksReadsZero_plus_blank() => Assert.AreEqual("0 0. 0", Run("+ "));
    [TestMethod] public void TEST_ASignWithBlanksReadsZero_a_sign_alone_fails() => Assert.AreEqual("FAIL FAIL FAIL", Run("-"));
    [TestMethod] public void TEST_ASignWithBlanksReadsZero_a_sign_at_the_end_fails() => Assert.AreEqual("FAIL FAIL FAIL", Run(" -"));
}
