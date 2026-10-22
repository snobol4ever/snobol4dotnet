using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// The errors of a pattern operator whose operand is no pattern: alternation reports its right operand first (5; the left one is 6, and they were swapped), and
/// ARBNO of a non-pattern is 61. Every expected line is what sbl -b printed for the same statement.
/// </summary>
[DoNotParallelize]
[TestClass]
public class PatternOperandErrors
{
    [TestMethod]
    public void TEST_PatternOperandErrors_alternation_and_arbno()
    {
        var s = @"
        &ERRLIMIT = 1000000
        &TRIM = 0
        DATA('D(X,Y,Z)')
        S = 'vv'
        A3 = ARRAY(3)
        T3 = TABLE()
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = V | 'a'   :S(S0_0)F(F0_0)
S0_0   OUTPUT = '79.DATA ok ' &ERRTYPE   :(N0_0)
F0_0   OUTPUT = '79.DATA fail ' &ERRTYPE
N0_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = V | 'a'   :S(S0_1)F(F0_1)
S0_1   OUTPUT = '79.ARR ok ' &ERRTYPE   :(N0_1)
F0_1   OUTPUT = '79.ARR fail ' &ERRTYPE
N0_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = V | 'a'   :S(S0_2)F(F0_2)
S0_2   OUTPUT = '79.TAB ok ' &ERRTYPE   :(N0_2)
F0_2   OUTPUT = '79.TAB fail ' &ERRTYPE
N0_2
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = 'a' | V   :S(S1_0)F(F1_0)
S1_0   OUTPUT = '80.DATA ok ' &ERRTYPE   :(N1_0)
F1_0   OUTPUT = '80.DATA fail ' &ERRTYPE
N1_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = 'a' | V   :S(S1_1)F(F1_1)
S1_1   OUTPUT = '80.ARR ok ' &ERRTYPE   :(N1_1)
F1_1   OUTPUT = '80.ARR fail ' &ERRTYPE
N1_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = 'a' | V   :S(S1_2)F(F1_2)
S1_2   OUTPUT = '80.TAB ok ' &ERRTYPE   :(N1_2)
F1_2   OUTPUT = '80.TAB fail ' &ERRTYPE
N1_2
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = ARBNO(V)   :S(S2_0)F(F2_0)
S2_0   OUTPUT = '35.DATA ok ' &ERRTYPE   :(N2_0)
F2_0   OUTPUT = '35.DATA fail ' &ERRTYPE
N2_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = ARBNO(V)   :S(S2_1)F(F2_1)
S2_1   OUTPUT = '35.ARR ok ' &ERRTYPE   :(N2_1)
F2_1   OUTPUT = '35.ARR fail ' &ERRTYPE
N2_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = ARBNO(V)   :S(S2_2)F(F2_2)
S2_2   OUTPUT = '35.TAB ok ' &ERRTYPE   :(N2_2)
F2_2   OUTPUT = '35.TAB fail ' &ERRTYPE
N2_2
END";
        Assert.AreEqual(@"79.DATA fail 6
79.ARR fail 6
79.TAB fail 6
80.DATA fail 5
80.ARR fail 5
80.TAB fail 5
35.DATA fail 61
35.ARR fail 61
35.TAB fail 61", SetupTests.RunWithInput(s));
    }
}
