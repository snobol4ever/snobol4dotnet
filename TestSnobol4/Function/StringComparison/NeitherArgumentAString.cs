using Test.TestLexer;

namespace Test.StringComparison;

/// <summary>
/// When neither argument of a lexical comparison is a string, the error is the second argument's, as in SPITBOL, which converts the
/// arguments as it takes them off its stack, the last first: LGE(P, T) of a pattern and a table is 125. The console converted the first
/// argument first and gave 124, and the same for LEQ LGT LLE LLT LNE (found by the infinite_snobol4 generator at scope 6, LGE(w, $'u')).
/// One argument that is no string still gives its own error. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NeitherArgumentAString
{
    [TestMethod]
    public void TEST_NeitherArgumentAString_the_error_is_the_second_arguments()
    {
        var program = @"        &ERRLIMIT = 30
        P = ANY('bcd')
        T = TABLE()
        &ERRTYPE = 0
        LEQ(P, T)
        OUTPUT = 'LEQ ' &ERRTYPE
        &ERRTYPE = 0
        LGE(P, T)
        OUTPUT = 'LGE ' &ERRTYPE
        &ERRTYPE = 0
        LGT(P, T)
        OUTPUT = 'LGT ' &ERRTYPE
        &ERRTYPE = 0
        LLE(P, T)
        OUTPUT = 'LLE ' &ERRTYPE
        &ERRTYPE = 0
        LLT(P, T)
        OUTPUT = 'LLT ' &ERRTYPE
        &ERRTYPE = 0
        LNE(P, T)
        OUTPUT = 'LNE ' &ERRTYPE
        &ERRTYPE = 0
        LGE(P, 'a')
        OUTPUT = 'first ' &ERRTYPE
        &ERRTYPE = 0
        LGE('a', T)
        OUTPUT = 'second ' &ERRTYPE
END";
        Assert.AreEqual(@"LEQ 123
LGE 125
LGT 127
LLE 129
LLT 131
LNE 133
first 124
second 125", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
