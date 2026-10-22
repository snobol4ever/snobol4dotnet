using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A DATA object is no string, as an array or a table is not: OUTPUT prints its datatype name, but a concatenation, SIZE, REVERSE, TRIM, DUPL, LPAD, SUBSTR,
/// REPLACE, a string comparison, a pattern-match subject, SPAN, BREAK, ANY and APPLY refuse it, each with the error number sbl gives. TRIM of any
/// value that is no string is error 200. Every expected line is what sbl -b printed for the same statement.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NonStringValues
{
    [TestMethod]
    public void TEST_NonStringValues_data_in_string_contexts()
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
        L = V 'x'   :S(S0_0)F(F0_0)
S0_0   OUTPUT = '0.DATA ok ' &ERRTYPE   :(N0_0)
F0_0   OUTPUT = '0.DATA fail ' &ERRTYPE
N0_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = V 'x'   :S(S0_1)F(F0_1)
S0_1   OUTPUT = '0.ARR ok ' &ERRTYPE   :(N0_1)
F0_1   OUTPUT = '0.ARR fail ' &ERRTYPE
N0_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = V 'x'   :S(S0_2)F(F0_2)
S0_2   OUTPUT = '0.TAB ok ' &ERRTYPE   :(N0_2)
F0_2   OUTPUT = '0.TAB fail ' &ERRTYPE
N0_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = V 'x'   :S(S0_3)F(F0_3)
S0_3   OUTPUT = '0.PAT ok ' &ERRTYPE   :(N0_3)
F0_3   OUTPUT = '0.PAT fail ' &ERRTYPE
N0_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = V 'x'   :S(S0_4)F(F0_4)
S0_4   OUTPUT = '0.EXPR ok ' &ERRTYPE   :(N0_4)
F0_4   OUTPUT = '0.EXPR fail ' &ERRTYPE
N0_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = 'x' V   :S(S1_0)F(F1_0)
S1_0   OUTPUT = '1.DATA ok ' &ERRTYPE   :(N1_0)
F1_0   OUTPUT = '1.DATA fail ' &ERRTYPE
N1_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = 'x' V   :S(S1_1)F(F1_1)
S1_1   OUTPUT = '1.ARR ok ' &ERRTYPE   :(N1_1)
F1_1   OUTPUT = '1.ARR fail ' &ERRTYPE
N1_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = 'x' V   :S(S1_2)F(F1_2)
S1_2   OUTPUT = '1.TAB ok ' &ERRTYPE   :(N1_2)
F1_2   OUTPUT = '1.TAB fail ' &ERRTYPE
N1_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = 'x' V   :S(S1_3)F(F1_3)
S1_3   OUTPUT = '1.PAT ok ' &ERRTYPE   :(N1_3)
F1_3   OUTPUT = '1.PAT fail ' &ERRTYPE
N1_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = 'x' V   :S(S1_4)F(F1_4)
S1_4   OUTPUT = '1.EXPR ok ' &ERRTYPE   :(N1_4)
F1_4   OUTPUT = '1.EXPR fail ' &ERRTYPE
N1_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = SIZE(V)   :S(S2_0)F(F2_0)
S2_0   OUTPUT = '2.DATA ok ' &ERRTYPE   :(N2_0)
F2_0   OUTPUT = '2.DATA fail ' &ERRTYPE
N2_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = SIZE(V)   :S(S2_1)F(F2_1)
S2_1   OUTPUT = '2.ARR ok ' &ERRTYPE   :(N2_1)
F2_1   OUTPUT = '2.ARR fail ' &ERRTYPE
N2_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = SIZE(V)   :S(S2_2)F(F2_2)
S2_2   OUTPUT = '2.TAB ok ' &ERRTYPE   :(N2_2)
F2_2   OUTPUT = '2.TAB fail ' &ERRTYPE
N2_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = SIZE(V)   :S(S2_3)F(F2_3)
S2_3   OUTPUT = '2.PAT ok ' &ERRTYPE   :(N2_3)
F2_3   OUTPUT = '2.PAT fail ' &ERRTYPE
N2_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = SIZE(V)   :S(S2_4)F(F2_4)
S2_4   OUTPUT = '2.EXPR ok ' &ERRTYPE   :(N2_4)
F2_4   OUTPUT = '2.EXPR fail ' &ERRTYPE
N2_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = REVERSE(V)   :S(S3_0)F(F3_0)
S3_0   OUTPUT = '3.DATA ok ' &ERRTYPE   :(N3_0)
F3_0   OUTPUT = '3.DATA fail ' &ERRTYPE
N3_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = REVERSE(V)   :S(S3_1)F(F3_1)
S3_1   OUTPUT = '3.ARR ok ' &ERRTYPE   :(N3_1)
F3_1   OUTPUT = '3.ARR fail ' &ERRTYPE
N3_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = REVERSE(V)   :S(S3_2)F(F3_2)
S3_2   OUTPUT = '3.TAB ok ' &ERRTYPE   :(N3_2)
F3_2   OUTPUT = '3.TAB fail ' &ERRTYPE
N3_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = REVERSE(V)   :S(S3_3)F(F3_3)
S3_3   OUTPUT = '3.PAT ok ' &ERRTYPE   :(N3_3)
F3_3   OUTPUT = '3.PAT fail ' &ERRTYPE
N3_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = REVERSE(V)   :S(S3_4)F(F3_4)
S3_4   OUTPUT = '3.EXPR ok ' &ERRTYPE   :(N3_4)
F3_4   OUTPUT = '3.EXPR fail ' &ERRTYPE
N3_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = TRIM(V)   :S(S4_0)F(F4_0)
S4_0   OUTPUT = '4.DATA ok ' &ERRTYPE   :(N4_0)
F4_0   OUTPUT = '4.DATA fail ' &ERRTYPE
N4_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = TRIM(V)   :S(S4_1)F(F4_1)
S4_1   OUTPUT = '4.ARR ok ' &ERRTYPE   :(N4_1)
F4_1   OUTPUT = '4.ARR fail ' &ERRTYPE
N4_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = TRIM(V)   :S(S4_2)F(F4_2)
S4_2   OUTPUT = '4.TAB ok ' &ERRTYPE   :(N4_2)
F4_2   OUTPUT = '4.TAB fail ' &ERRTYPE
N4_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = TRIM(V)   :S(S4_3)F(F4_3)
S4_3   OUTPUT = '4.PAT ok ' &ERRTYPE   :(N4_3)
F4_3   OUTPUT = '4.PAT fail ' &ERRTYPE
N4_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = TRIM(V)   :S(S4_4)F(F4_4)
S4_4   OUTPUT = '4.EXPR ok ' &ERRTYPE   :(N4_4)
F4_4   OUTPUT = '4.EXPR fail ' &ERRTYPE
N4_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = DUPL(V, 2)   :S(S5_0)F(F5_0)
S5_0   OUTPUT = '5.DATA ok ' &ERRTYPE   :(N5_0)
F5_0   OUTPUT = '5.DATA fail ' &ERRTYPE
N5_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = DUPL(V, 2)   :S(S5_1)F(F5_1)
S5_1   OUTPUT = '5.ARR ok ' &ERRTYPE   :(N5_1)
F5_1   OUTPUT = '5.ARR fail ' &ERRTYPE
N5_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = DUPL(V, 2)   :S(S5_2)F(F5_2)
S5_2   OUTPUT = '5.TAB ok ' &ERRTYPE   :(N5_2)
F5_2   OUTPUT = '5.TAB fail ' &ERRTYPE
N5_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = DUPL(V, 2)   :S(S5_3)F(F5_3)
S5_3   OUTPUT = '5.PAT ok ' &ERRTYPE   :(N5_3)
F5_3   OUTPUT = '5.PAT fail ' &ERRTYPE
N5_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = DUPL(V, 2)   :S(S5_4)F(F5_4)
S5_4   OUTPUT = '5.EXPR ok ' &ERRTYPE   :(N5_4)
F5_4   OUTPUT = '5.EXPR fail ' &ERRTYPE
N5_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = LGT(V, 'a')   :S(S6_0)F(F6_0)
S6_0   OUTPUT = '14.DATA ok ' &ERRTYPE   :(N6_0)
F6_0   OUTPUT = '14.DATA fail ' &ERRTYPE
N6_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = LGT(V, 'a')   :S(S6_1)F(F6_1)
S6_1   OUTPUT = '14.ARR ok ' &ERRTYPE   :(N6_1)
F6_1   OUTPUT = '14.ARR fail ' &ERRTYPE
N6_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = LGT(V, 'a')   :S(S6_2)F(F6_2)
S6_2   OUTPUT = '14.TAB ok ' &ERRTYPE   :(N6_2)
F6_2   OUTPUT = '14.TAB fail ' &ERRTYPE
N6_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = LGT(V, 'a')   :S(S6_3)F(F6_3)
S6_3   OUTPUT = '14.PAT ok ' &ERRTYPE   :(N6_3)
F6_3   OUTPUT = '14.PAT fail ' &ERRTYPE
N6_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = LGT(V, 'a')   :S(S6_4)F(F6_4)
S6_4   OUTPUT = '14.EXPR ok ' &ERRTYPE   :(N6_4)
F6_4   OUTPUT = '14.EXPR fail ' &ERRTYPE
N6_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = LGT('a', V)   :S(S7_0)F(F7_0)
S7_0   OUTPUT = '15.DATA ok ' &ERRTYPE   :(N7_0)
F7_0   OUTPUT = '15.DATA fail ' &ERRTYPE
N7_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = LGT('a', V)   :S(S7_1)F(F7_1)
S7_1   OUTPUT = '15.ARR ok ' &ERRTYPE   :(N7_1)
F7_1   OUTPUT = '15.ARR fail ' &ERRTYPE
N7_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = LGT('a', V)   :S(S7_2)F(F7_2)
S7_2   OUTPUT = '15.TAB ok ' &ERRTYPE   :(N7_2)
F7_2   OUTPUT = '15.TAB fail ' &ERRTYPE
N7_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = LGT('a', V)   :S(S7_3)F(F7_3)
S7_3   OUTPUT = '15.PAT ok ' &ERRTYPE   :(N7_3)
F7_3   OUTPUT = '15.PAT fail ' &ERRTYPE
N7_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = LGT('a', V)   :S(S7_4)F(F7_4)
S7_4   OUTPUT = '15.EXPR ok ' &ERRTYPE   :(N7_4)
F7_4   OUTPUT = '15.EXPR fail ' &ERRTYPE
N7_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        V ? 'a'   :S(S8_0)F(F8_0)
S8_0   OUTPUT = '24.DATA ok ' &ERRTYPE   :(N8_0)
F8_0   OUTPUT = '24.DATA fail ' &ERRTYPE
N8_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        V ? 'a'   :S(S8_1)F(F8_1)
S8_1   OUTPUT = '24.ARR ok ' &ERRTYPE   :(N8_1)
F8_1   OUTPUT = '24.ARR fail ' &ERRTYPE
N8_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        V ? 'a'   :S(S8_2)F(F8_2)
S8_2   OUTPUT = '24.TAB ok ' &ERRTYPE   :(N8_2)
F8_2   OUTPUT = '24.TAB fail ' &ERRTYPE
N8_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        V ? 'a'   :S(S8_3)F(F8_3)
S8_3   OUTPUT = '24.PAT ok ' &ERRTYPE   :(N8_3)
F8_3   OUTPUT = '24.PAT fail ' &ERRTYPE
N8_3
        &ERRTYPE = 0
        V = *S
        L = ''
        V ? 'a'   :S(S8_4)F(F8_4)
S8_4   OUTPUT = '24.EXPR ok ' &ERRTYPE   :(N8_4)
F8_4   OUTPUT = '24.EXPR fail ' &ERRTYPE
N8_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        V ? LEN(1)   :S(S9_0)F(F9_0)
S9_0   OUTPUT = '25.DATA ok ' &ERRTYPE   :(N9_0)
F9_0   OUTPUT = '25.DATA fail ' &ERRTYPE
N9_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        V ? LEN(1)   :S(S9_1)F(F9_1)
S9_1   OUTPUT = '25.ARR ok ' &ERRTYPE   :(N9_1)
F9_1   OUTPUT = '25.ARR fail ' &ERRTYPE
N9_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        V ? LEN(1)   :S(S9_2)F(F9_2)
S9_2   OUTPUT = '25.TAB ok ' &ERRTYPE   :(N9_2)
F9_2   OUTPUT = '25.TAB fail ' &ERRTYPE
N9_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        V ? LEN(1)   :S(S9_3)F(F9_3)
S9_3   OUTPUT = '25.PAT ok ' &ERRTYPE   :(N9_3)
F9_3   OUTPUT = '25.PAT fail ' &ERRTYPE
N9_3
        &ERRTYPE = 0
        V = *S
        L = ''
        V ? LEN(1)   :S(S9_4)F(F9_4)
S9_4   OUTPUT = '25.EXPR ok ' &ERRTYPE   :(N9_4)
F9_4   OUTPUT = '25.EXPR fail ' &ERRTYPE
N9_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = SPAN(V)   :S(S10_0)F(F10_0)
S10_0   OUTPUT = '32.DATA ok ' &ERRTYPE   :(N10_0)
F10_0   OUTPUT = '32.DATA fail ' &ERRTYPE
N10_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = SPAN(V)   :S(S10_1)F(F10_1)
S10_1   OUTPUT = '32.ARR ok ' &ERRTYPE   :(N10_1)
F10_1   OUTPUT = '32.ARR fail ' &ERRTYPE
N10_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = SPAN(V)   :S(S10_2)F(F10_2)
S10_2   OUTPUT = '32.TAB ok ' &ERRTYPE   :(N10_2)
F10_2   OUTPUT = '32.TAB fail ' &ERRTYPE
N10_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = SPAN(V)   :S(S10_3)F(F10_3)
S10_3   OUTPUT = '32.PAT ok ' &ERRTYPE   :(N10_3)
F10_3   OUTPUT = '32.PAT fail ' &ERRTYPE
N10_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = SPAN(V)   :S(S10_4)F(F10_4)
S10_4   OUTPUT = '32.EXPR ok ' &ERRTYPE   :(N10_4)
F10_4   OUTPUT = '32.EXPR fail ' &ERRTYPE
N10_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = BREAK(V)   :S(S11_0)F(F11_0)
S11_0   OUTPUT = '33.DATA ok ' &ERRTYPE   :(N11_0)
F11_0   OUTPUT = '33.DATA fail ' &ERRTYPE
N11_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = BREAK(V)   :S(S11_1)F(F11_1)
S11_1   OUTPUT = '33.ARR ok ' &ERRTYPE   :(N11_1)
F11_1   OUTPUT = '33.ARR fail ' &ERRTYPE
N11_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = BREAK(V)   :S(S11_2)F(F11_2)
S11_2   OUTPUT = '33.TAB ok ' &ERRTYPE   :(N11_2)
F11_2   OUTPUT = '33.TAB fail ' &ERRTYPE
N11_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = BREAK(V)   :S(S11_3)F(F11_3)
S11_3   OUTPUT = '33.PAT ok ' &ERRTYPE   :(N11_3)
F11_3   OUTPUT = '33.PAT fail ' &ERRTYPE
N11_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = BREAK(V)   :S(S11_4)F(F11_4)
S11_4   OUTPUT = '33.EXPR ok ' &ERRTYPE   :(N11_4)
F11_4   OUTPUT = '33.EXPR fail ' &ERRTYPE
N11_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = ANY(V)   :S(S12_0)F(F12_0)
S12_0   OUTPUT = '34.DATA ok ' &ERRTYPE   :(N12_0)
F12_0   OUTPUT = '34.DATA fail ' &ERRTYPE
N12_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = ANY(V)   :S(S12_1)F(F12_1)
S12_1   OUTPUT = '34.ARR ok ' &ERRTYPE   :(N12_1)
F12_1   OUTPUT = '34.ARR fail ' &ERRTYPE
N12_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = ANY(V)   :S(S12_2)F(F12_2)
S12_2   OUTPUT = '34.TAB ok ' &ERRTYPE   :(N12_2)
F12_2   OUTPUT = '34.TAB fail ' &ERRTYPE
N12_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = ANY(V)   :S(S12_3)F(F12_3)
S12_3   OUTPUT = '34.PAT ok ' &ERRTYPE   :(N12_3)
F12_3   OUTPUT = '34.PAT fail ' &ERRTYPE
N12_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = ANY(V)   :S(S12_4)F(F12_4)
S12_4   OUTPUT = '34.EXPR ok ' &ERRTYPE   :(N12_4)
F12_4   OUTPUT = '34.EXPR fail ' &ERRTYPE
N12_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = REPLACE(V, 'a', 'b')   :S(S13_0)F(F13_0)
S13_0   OUTPUT = '55.DATA ok ' &ERRTYPE   :(N13_0)
F13_0   OUTPUT = '55.DATA fail ' &ERRTYPE
N13_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = REPLACE(V, 'a', 'b')   :S(S13_1)F(F13_1)
S13_1   OUTPUT = '55.ARR ok ' &ERRTYPE   :(N13_1)
F13_1   OUTPUT = '55.ARR fail ' &ERRTYPE
N13_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = REPLACE(V, 'a', 'b')   :S(S13_2)F(F13_2)
S13_2   OUTPUT = '55.TAB ok ' &ERRTYPE   :(N13_2)
F13_2   OUTPUT = '55.TAB fail ' &ERRTYPE
N13_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = REPLACE(V, 'a', 'b')   :S(S13_3)F(F13_3)
S13_3   OUTPUT = '55.PAT ok ' &ERRTYPE   :(N13_3)
F13_3   OUTPUT = '55.PAT fail ' &ERRTYPE
N13_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = REPLACE(V, 'a', 'b')   :S(S13_4)F(F13_4)
S13_4   OUTPUT = '55.EXPR ok ' &ERRTYPE   :(N13_4)
F13_4   OUTPUT = '55.EXPR fail ' &ERRTYPE
N13_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = SUBSTR(V, 1, 1)   :S(S14_0)F(F14_0)
S14_0   OUTPUT = '56.DATA ok ' &ERRTYPE   :(N14_0)
F14_0   OUTPUT = '56.DATA fail ' &ERRTYPE
N14_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = SUBSTR(V, 1, 1)   :S(S14_1)F(F14_1)
S14_1   OUTPUT = '56.ARR ok ' &ERRTYPE   :(N14_1)
F14_1   OUTPUT = '56.ARR fail ' &ERRTYPE
N14_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = SUBSTR(V, 1, 1)   :S(S14_2)F(F14_2)
S14_2   OUTPUT = '56.TAB ok ' &ERRTYPE   :(N14_2)
F14_2   OUTPUT = '56.TAB fail ' &ERRTYPE
N14_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = SUBSTR(V, 1, 1)   :S(S14_3)F(F14_3)
S14_3   OUTPUT = '56.PAT ok ' &ERRTYPE   :(N14_3)
F14_3   OUTPUT = '56.PAT fail ' &ERRTYPE
N14_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = SUBSTR(V, 1, 1)   :S(S14_4)F(F14_4)
S14_4   OUTPUT = '56.EXPR ok ' &ERRTYPE   :(N14_4)
F14_4   OUTPUT = '56.EXPR fail ' &ERRTYPE
N14_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = LPAD(V, 5)   :S(S15_0)F(F15_0)
S15_0   OUTPUT = '58.DATA ok ' &ERRTYPE   :(N15_0)
F15_0   OUTPUT = '58.DATA fail ' &ERRTYPE
N15_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = LPAD(V, 5)   :S(S15_1)F(F15_1)
S15_1   OUTPUT = '58.ARR ok ' &ERRTYPE   :(N15_1)
F15_1   OUTPUT = '58.ARR fail ' &ERRTYPE
N15_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = LPAD(V, 5)   :S(S15_2)F(F15_2)
S15_2   OUTPUT = '58.TAB ok ' &ERRTYPE   :(N15_2)
F15_2   OUTPUT = '58.TAB fail ' &ERRTYPE
N15_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = LPAD(V, 5)   :S(S15_3)F(F15_3)
S15_3   OUTPUT = '58.PAT ok ' &ERRTYPE   :(N15_3)
F15_3   OUTPUT = '58.PAT fail ' &ERRTYPE
N15_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = LPAD(V, 5)   :S(S15_4)F(F15_4)
S15_4   OUTPUT = '58.EXPR ok ' &ERRTYPE   :(N15_4)
F15_4   OUTPUT = '58.EXPR fail ' &ERRTYPE
N15_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = APPLY(V)   :S(S16_0)F(F16_0)
S16_0   OUTPUT = '65.DATA ok ' &ERRTYPE   :(N16_0)
F16_0   OUTPUT = '65.DATA fail ' &ERRTYPE
N16_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = APPLY(V)   :S(S16_1)F(F16_1)
S16_1   OUTPUT = '65.ARR ok ' &ERRTYPE   :(N16_1)
F16_1   OUTPUT = '65.ARR fail ' &ERRTYPE
N16_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = APPLY(V)   :S(S16_2)F(F16_2)
S16_2   OUTPUT = '65.TAB ok ' &ERRTYPE   :(N16_2)
F16_2   OUTPUT = '65.TAB fail ' &ERRTYPE
N16_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = APPLY(V)   :S(S16_3)F(F16_3)
S16_3   OUTPUT = '65.PAT ok ' &ERRTYPE   :(N16_3)
F16_3   OUTPUT = '65.PAT fail ' &ERRTYPE
N16_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = APPLY(V)   :S(S16_4)F(F16_4)
S16_4   OUTPUT = '65.EXPR ok ' &ERRTYPE   :(N16_4)
F16_4   OUTPUT = '65.EXPR fail ' &ERRTYPE
N16_4
        &ERRTYPE = 0
        V = D(1,2,3)
        L = ''
        L = V V   :S(S17_0)F(F17_0)
S17_0   OUTPUT = '81.DATA ok ' &ERRTYPE   :(N17_0)
F17_0   OUTPUT = '81.DATA fail ' &ERRTYPE
N17_0
        &ERRTYPE = 0
        V = ARRAY(3)
        L = ''
        L = V V   :S(S17_1)F(F17_1)
S17_1   OUTPUT = '81.ARR ok ' &ERRTYPE   :(N17_1)
F17_1   OUTPUT = '81.ARR fail ' &ERRTYPE
N17_1
        &ERRTYPE = 0
        V = TABLE()
        L = ''
        L = V V   :S(S17_2)F(F17_2)
S17_2   OUTPUT = '81.TAB ok ' &ERRTYPE   :(N17_2)
F17_2   OUTPUT = '81.TAB fail ' &ERRTYPE
N17_2
        &ERRTYPE = 0
        V = LEN(2)
        L = ''
        L = V V   :S(S17_3)F(F17_3)
S17_3   OUTPUT = '81.PAT ok ' &ERRTYPE   :(N17_3)
F17_3   OUTPUT = '81.PAT fail ' &ERRTYPE
N17_3
        &ERRTYPE = 0
        V = *S
        L = ''
        L = V V   :S(S17_4)F(F17_4)
S17_4   OUTPUT = '81.EXPR ok ' &ERRTYPE   :(N17_4)
F17_4   OUTPUT = '81.EXPR fail ' &ERRTYPE
N17_4
END";
        Assert.AreEqual(@"0.DATA fail 8
0.ARR fail 8
0.TAB fail 8
0.PAT ok 0
0.EXPR ok 0
1.DATA fail 9
1.ARR fail 9
1.TAB fail 9
1.PAT ok 0
1.EXPR ok 0
2.DATA fail 189
2.ARR fail 189
2.TAB fail 189
2.PAT fail 189
2.EXPR fail 189
3.DATA fail 177
3.ARR fail 177
3.TAB fail 177
3.PAT fail 177
3.EXPR fail 177
4.DATA fail 200
4.ARR fail 200
4.TAB fail 200
4.PAT fail 200
4.EXPR fail 200
5.DATA fail 91
5.ARR fail 91
5.TAB fail 91
5.PAT ok 0
5.EXPR ok 0
14.DATA fail 126
14.ARR fail 126
14.TAB fail 126
14.PAT fail 126
14.EXPR fail 126
15.DATA fail 127
15.ARR fail 127
15.TAB fail 127
15.PAT fail 127
15.EXPR fail 127
24.DATA fail 241
24.ARR fail 241
24.TAB fail 241
24.PAT fail 241
24.EXPR fail 241
25.DATA fail 241
25.ARR fail 241
25.TAB fail 241
25.PAT fail 241
25.EXPR fail 241
32.DATA fail 188
32.ARR fail 188
32.TAB fail 188
32.PAT fail 188
32.EXPR ok 0
33.DATA fail 69
33.ARR fail 69
33.TAB fail 69
33.PAT fail 69
33.EXPR ok 0
34.DATA fail 59
34.ARR fail 59
34.TAB fail 59
34.PAT fail 59
34.EXPR ok 0
55.DATA fail 170
55.ARR fail 170
55.TAB fail 170
55.PAT fail 170
55.EXPR fail 170
56.DATA fail 194
56.ARR fail 194
56.TAB fail 194
56.PAT fail 194
56.EXPR fail 194
58.DATA fail 146
58.ARR fail 146
58.TAB fail 146
58.PAT fail 146
58.EXPR fail 146
65.DATA fail 60
65.ARR fail 60
65.TAB fail 60
65.PAT fail 60
65.EXPR fail 60
81.DATA fail 8
81.ARR fail 8
81.TAB fail 8
81.PAT ok 0
81.EXPR ok 0", SetupTests.RunWithInput(s));
    }
}
