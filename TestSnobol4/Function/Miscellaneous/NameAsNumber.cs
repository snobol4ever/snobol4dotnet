using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A name is a number by its text, as SPITBOL converts it: .X + 1 is error 1, LT(.X, 6) 147, REMDR(.X, 2) 166, and CONVERT('12', 'NAME') * 2
/// is 24; a call that returns a name by NRETURN still gives the variable's value (H() + 1). Each read the named variable's value. Found by the
/// infinite_snobol4 generator (+(CONVERT(c, 'NAME')) ** j '6z': 5126z in sbl, 06z here). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NameAsNumber
{
    [TestMethod]
    public void TEST_NameAsNumber_a_name_is_a_number_by_its_text()
    {
        var program = @"        &ERRLIMIT = 30
        DEFINE('H()')                               :(HE)
H       H = .X                                      :(NRETURN)
HE      X = 5
        A = ARRAY(3)
        A<1> = 7
        T = TABLE()
        T<'k'> = 9
        N = .X
        &ERRTYPE = 0
        V = H() + 1
        OUTPUT = '1 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = N + 1
        OUTPUT = '2 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = .A<1>
        V = V + 1
        OUTPUT = '3 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = .T<'k'>
        V = -V
        OUTPUT = '4 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = CONVERT('12', 'NAME')
        V = V * 2
        OUTPUT = '5 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = LT(N, 6)
        OUTPUT = '6 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = REMDR(N, 2)
        OUTPUT = '7 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = LEN(N)
        OUTPUT = '8 ' &ERRTYPE ' ' DATATYPE(V)
        &ERRTYPE = 0
        V = H() * H()
        OUTPUT = '9 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = DUPL('a', N)
        OUTPUT = '10 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = SIZE(N)
        OUTPUT = '11 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = N 'z'
        OUTPUT = '12 ' &ERRTYPE ' ' V
END";
        Assert.AreEqual(@"1 0 6
2 1 6
5 0 24
6 147 24
7 166 24
8 120 INTEGER
9 0 25
10 90 25
11 0 1
12 0 Xz", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
