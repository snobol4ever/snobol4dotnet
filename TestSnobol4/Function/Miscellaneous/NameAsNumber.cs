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

    /// <summary>
    /// The name of an element or of a table entry has no text, so it is no integer and no real, as in SPITBOL: a builtin that wants an
    /// integer gives its own error (DUPL 90, RPAD 179, CHAR 281, LEN 120, SUBSTR 193, LPAD 145) and CONVERT fails. The console converted
    /// the empty text and took 0: RPAD('u6', .a[g]) was u6 (found by the infinite_snobol4 generator at scope 6). A name with digits for
    /// its text is still that number (9). Every expected line is what sbl -b printed.
    /// </summary>
    [TestMethod]
    public void TEST_NameAsNumber_the_name_of_an_element_is_no_number()
    {
        var program = @"        &ERRLIMIT = 30
        A = ARRAY(3)
        A[1] = 2
        T = TABLE()
        T['k'] = 3
        &ERRTYPE = 0
        V = 'none'
        V = DUPL('ab', .A[1])
        OUTPUT = '1 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = RPAD('u6', .A[1])
        OUTPUT = '2 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = CHAR(.A[1])
        OUTPUT = '3 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = 'u6' ? LEN(.A[1])
        OUTPUT = '4 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = SUBSTR('abc', .A[1])
        OUTPUT = '5 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = LPAD('u6', .T['k'])
        OUTPUT = '6 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = CONVERT(.A[1], 'INTEGER')
        OUTPUT = '7 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = CONVERT(.T['k'], 'REAL')
        OUTPUT = '8 ' &ERRTYPE ' ' V
        &ERRTYPE = 0
        V = 'none'
        V = DUPL('ab', .$'3')
        OUTPUT = '9 ' &ERRTYPE ' ' V
END";
        Assert.AreEqual(@"1 90 none
2 179 none
3 281 none
4 120 none
5 193 none
6 145 none
7 0 none
8 0 none
9 0 ababab", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
