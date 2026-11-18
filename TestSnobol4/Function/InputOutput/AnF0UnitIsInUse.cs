using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// INPUT(.X, 5, '[-f0]') opens unit 5 on standard input, as SPITBOL does: a second INPUT naming a file specification for unit 5 is error
/// 289, input channel currently in use, and INPUT(.Y, 5) with none joins it (sbl -bf, probed; gate_cases_snobol4
/// test_gate_sno_input_honours_the_f_and_l_file_options__reassoc). snobol4dotnet bound X to standard input and left unit 5 free, so the
/// second INPUT succeeded and the third failed, error 116.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AnF0UnitIsInUse
{
    private static string Run(string second) => SetupTests.RunWithInputDirectives("-bf", $@"        &ERRLIMIT = 5
        INPUT(.X, 5, '[-f0]')
{second}    :F(F)
        OUTPUT = 'second ok'                    :(E)
F       OUTPUT = 'second fails ' &ERRTYPE
E       OUTPUT = 'end'
END").Replace("\r\n", "\n").TrimEnd();

    [TestMethod] public void TEST_AnF0UnitIsInUse_a_second_specification_is_289() => Assert.AreEqual("second fails 289\nend", Run("        INPUT(.Y, 5, '[-f0]')"));
    [TestMethod] public void TEST_AnF0UnitIsInUse_no_specification_joins_the_unit() => Assert.AreEqual("second ok\nend", Run("        INPUT(.Y, 5)"));
}
