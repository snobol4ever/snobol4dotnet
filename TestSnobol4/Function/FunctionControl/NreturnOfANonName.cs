using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A function that NRETURNs a value that is no name gives the name that value spells when it is a string, an integer or a real, and
/// otherwise is error 243, function result in NRETURN is not name, as in SPITBOL: the null string, an array, a table, a pattern, an
/// expression (sbl -bf, probed one value at a time, directly and through APPLY). snobol4dotnet raised nothing and handed the value
/// back (gate_cases_snobol4 test_gate_sno_nreturn_of_a_non_name_converts_or_raises_243_as_spitbol_does__apply).
/// </summary>
[DoNotParallelize]
[TestClass]
public class NreturnOfANonName
{
    private static string Run(string value) => SetupTests.RunWithInputDirectives("-bf", $@"	&ERRLIMIT = 5
	ABC = 'hello'
	DEFINE('H()')	:(M)
H	H = {value}	:(NRETURN)
M	X = H()	:F(F1)
	OUTPUT = 'ok ' DATATYPE(X) ' [' X ']'
	Y = APPLY('H')	:F(F2)
	OUTPUT = 'apply ok'	:(END)
F1	OUTPUT = 'failed ' &ERRTYPE
	Y = APPLY('H')	:F(F2)
	OUTPUT = 'apply ok'	:(END)
F2	OUTPUT = 'apply failed ' &ERRTYPE
END").Replace("\r\n", "\n").TrimEnd();

    [TestMethod] public void TEST_NreturnOfANonName_a_string_is_the_name_it_spells() => Assert.AreEqual("ok STRING [hello]\napply ok", Run("'ABC'"));
    [TestMethod] public void TEST_NreturnOfANonName_an_array_is_243() => Assert.AreEqual("failed 243\napply failed 243", Run("ARRAY(2)"));
    [TestMethod] public void TEST_NreturnOfANonName_a_table_is_243() => Assert.AreEqual("failed 243\napply failed 243", Run("TABLE()"));
    [TestMethod] public void TEST_NreturnOfANonName_a_pattern_is_243() => Assert.AreEqual("failed 243\napply failed 243", Run("LEN(1)"));
    [TestMethod] public void TEST_NreturnOfANonName_the_null_string_is_243() => Assert.AreEqual("failed 243\napply failed 243", Run("''"));
}
