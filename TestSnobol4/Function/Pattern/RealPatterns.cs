using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A real used as a pattern is the pattern that matches its text, as an integer is; it used to throw an InvalidCastException that ended the run. Every expected
/// line is what sbl -b printed for the same statement.
/// </summary>
[DoNotParallelize]
[TestClass]
public class RealPatterns
{
    [TestMethod]
    public void TEST_RealPatterns_a_real_is_a_pattern()
    {
        var s = @"
        &ERRLIMIT = 1000000
        &TRIM = 0
        DATA('D(X,Y,Z)')
        S = 'vv'
        A3 = ARRAY(3)
        T3 = TABLE()
        &ERRTYPE = 0
        V = 2.5
        L = ''
        L = 'abc' ? V   :S(S0_0)F(F0_0)
S0_0   OUTPUT = '26.REAL ok ' &ERRTYPE   :(N0_0)
F0_0   OUTPUT = '26.REAL fail ' &ERRTYPE
N0_0
        &ERRTYPE = 0
        V = 7
        L = ''
        L = 'abc' ? V   :S(S0_1)F(F0_1)
S0_1   OUTPUT = '26.INT ok ' &ERRTYPE   :(N0_1)
F0_1   OUTPUT = '26.INT fail ' &ERRTYPE
N0_1
        &ERRTYPE = 0
        V = 'abc'
        L = ''
        L = 'abc' ? V   :S(S0_2)F(F0_2)
S0_2   OUTPUT = '26.STR ok ' &ERRTYPE   :(N0_2)
F0_2   OUTPUT = '26.STR fail ' &ERRTYPE
N0_2
        &ERRTYPE = 0
        V = 2.5
        L = ''
        L = ARBNO(V)   :S(S1_0)F(F1_0)
S1_0   OUTPUT = '35.REAL ok ' &ERRTYPE   :(N1_0)
F1_0   OUTPUT = '35.REAL fail ' &ERRTYPE
N1_0
        &ERRTYPE = 0
        V = 7
        L = ''
        L = ARBNO(V)   :S(S1_1)F(F1_1)
S1_1   OUTPUT = '35.INT ok ' &ERRTYPE   :(N1_1)
F1_1   OUTPUT = '35.INT fail ' &ERRTYPE
N1_1
        &ERRTYPE = 0
        V = 'abc'
        L = ''
        L = ARBNO(V)   :S(S1_2)F(F1_2)
S1_2   OUTPUT = '35.STR ok ' &ERRTYPE   :(N1_2)
F1_2   OUTPUT = '35.STR fail ' &ERRTYPE
N1_2
        &ERRTYPE = 0
        V = 2.5
        L = ''
        L = FENCE(V)   :S(S2_0)F(F2_0)
S2_0   OUTPUT = '36.REAL ok ' &ERRTYPE   :(N2_0)
F2_0   OUTPUT = '36.REAL fail ' &ERRTYPE
N2_0
        &ERRTYPE = 0
        V = 7
        L = ''
        L = FENCE(V)   :S(S2_1)F(F2_1)
S2_1   OUTPUT = '36.INT ok ' &ERRTYPE   :(N2_1)
F2_1   OUTPUT = '36.INT fail ' &ERRTYPE
N2_1
        &ERRTYPE = 0
        V = 'abc'
        L = ''
        L = FENCE(V)   :S(S2_2)F(F2_2)
S2_2   OUTPUT = '36.STR ok ' &ERRTYPE   :(N2_2)
F2_2   OUTPUT = '36.STR fail ' &ERRTYPE
N2_2
        &ERRTYPE = 0
        V = 2.5
        L = ''
        L = V | 'a'   :S(S3_0)F(F3_0)
S3_0   OUTPUT = '79.REAL ok ' &ERRTYPE   :(N3_0)
F3_0   OUTPUT = '79.REAL fail ' &ERRTYPE
N3_0
        &ERRTYPE = 0
        V = 7
        L = ''
        L = V | 'a'   :S(S3_1)F(F3_1)
S3_1   OUTPUT = '79.INT ok ' &ERRTYPE   :(N3_1)
F3_1   OUTPUT = '79.INT fail ' &ERRTYPE
N3_1
        &ERRTYPE = 0
        V = 'abc'
        L = ''
        L = V | 'a'   :S(S3_2)F(F3_2)
S3_2   OUTPUT = '79.STR ok ' &ERRTYPE   :(N3_2)
F3_2   OUTPUT = '79.STR fail ' &ERRTYPE
N3_2
        &ERRTYPE = 0
        V = 2.5
        L = ''
        L = 'a' | V   :S(S4_0)F(F4_0)
S4_0   OUTPUT = '80.REAL ok ' &ERRTYPE   :(N4_0)
F4_0   OUTPUT = '80.REAL fail ' &ERRTYPE
N4_0
        &ERRTYPE = 0
        V = 7
        L = ''
        L = 'a' | V   :S(S4_1)F(F4_1)
S4_1   OUTPUT = '80.INT ok ' &ERRTYPE   :(N4_1)
F4_1   OUTPUT = '80.INT fail ' &ERRTYPE
N4_1
        &ERRTYPE = 0
        V = 'abc'
        L = ''
        L = 'a' | V   :S(S4_2)F(F4_2)
S4_2   OUTPUT = '80.STR ok ' &ERRTYPE   :(N4_2)
F4_2   OUTPUT = '80.STR fail ' &ERRTYPE
N4_2
END";
        Assert.AreEqual(@"26.REAL fail 0
26.INT fail 0
26.STR ok 0
35.REAL ok 0
35.INT ok 0
35.STR ok 0
36.REAL ok 0
36.INT ok 0
36.STR ok 0
79.REAL ok 0
79.INT ok 0
79.STR ok 0
80.REAL ok 0
80.INT ok 0
80.STR ok 0", SetupTests.RunWithInput(s));
    }
}
