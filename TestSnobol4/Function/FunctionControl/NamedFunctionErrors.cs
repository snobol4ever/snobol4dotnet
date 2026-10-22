using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// OPSYN, CLEAR, EVAL, CODE and CONVERT to CODE as sbl takes them: a null function name is 155 (an operator that is no string 156); CLEAR trims the blanks of the names it is given, and a null one is 72; EVAL of a NAME is the value of the variable (an element's name is 103) and EVAL of text with a comma outside every bracket is 223; CODE compiles the text of a number or of a NAME (CODE(.S) compiles S, not what S holds) and fails for anything else. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NamedFunctionErrors
{
    [TestMethod]
    public void TEST_NamedFunctionErrors_functions()
    {
        var s = @"        &ERRLIMIT = 100
        E = ''
        P = LEN(2)
        BL = '  '
        X = OPSYN(E, 'SIZE')
        OUTPUT = '1 ' &ERRTYPE
        &ERRTYPE = 0
        X = OPSYN(BL, 'SIZE', 1)
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        X = OPSYN(P, 'SIZE', 1)
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        X = OPSYN(P, 'SIZE')
        OUTPUT = '4 ' &ERRTYPE
        &ERRTYPE = 0
        X = OPSYN('MYSIZE', 'SIZE')
        OUTPUT = '5 ' MYSIZE('abc') ' ' &ERRTYPE
        &ERRTYPE = 0
        A = 1
        B = 2
        C = 3
        CLEAR(' A , B ')
        OUTPUT = '6 [' A '][' B '][' C ']'
        A = 1
        B = 2
        C = 3
        CLEAR('B')
        OUTPUT = '7 [' A '][' B '][' C ']'
        X = CLEAR('  ')
        OUTPUT = '8 ' &ERRTYPE
        &ERRTYPE = 0
        X = CLEAR('A,')
        OUTPUT = '9 ' &ERRTYPE
        &ERRTYPE = 0
        X = CLEAR(P)
        OUTPUT = '10 ' &ERRTYPE
        &ERRTYPE = 0
        W = 5
        V = 'vv'
        T = TABLE()
        T<'k'> = 'tv'
        X = EVAL('W,W')
        OUTPUT = '11 ' &ERRTYPE
        &ERRTYPE = 0
        X = EVAL('(W,W)')
        OUTPUT = '12 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        X = EVAL('W ,')
        OUTPUT = '13 ' &ERRTYPE
        &ERRTYPE = 0
        X = EVAL(.V)
        OUTPUT = '14 [' X '] ' &ERRTYPE
        N = .V
        X = EVAL(N)
        OUTPUT = '15 [' X '] ' &ERRTYPE
        X = EVAL(.W)
        OUTPUT = '16 [' X '] ' &ERRTYPE
        X = EVAL(.T<'k'>)
        OUTPUT = '17 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        X = EVAL(""'a,b'"")
        OUTPUT = '18 [' X '] ' &ERRTYPE
        X = EVAL('SIZE(""a,b"")')
        OUTPUT = '19 [' X '] ' &ERRTYPE
        X = EVAL('W + 1')
        OUTPUT = '20 [' X '] ' &ERRTYPE
        S = ' X = 1 :(BACK)'
        C1 = CODE(.S)
        OUTPUT = '21 ' DATATYPE(C1) ' ' &ERRTYPE
        C3 = CODE(7)
        OUTPUT = '22 ' DATATYPE(C3) ' ' &ERRTYPE
        C4 = CODE(2.5)
        OUTPUT = '23 ' DATATYPE(C4) ' ' &ERRTYPE
        C7 = CODE(C3)
        OUTPUT = '24 ' DATATYPE(C7) ' ' &ERRTYPE
        &ERRTYPE = 0
        C8 = CODE(P)
        OUTPUT = '25 ' DATATYPE(C8) ' ' &ERRTYPE
        C9 = CODE(ARRAY(2))
        OUTPUT = '26 ' DATATYPE(C9) ' ' &ERRTYPE
        C10 = CONVERT(7, 'CODE')
        OUTPUT = '27 ' DATATYPE(C10) ' ' &ERRTYPE
        C11 = CONVERT(2.5, 'CODE')
        OUTPUT = '28 ' DATATYPE(C11) ' ' &ERRTYPE
END";
        Assert.AreEqual(@"1 155
2 156
3 156
4 155
5 3 0
6 [1][2][]
7 [][2][]
8 72
9 72
10 0
11 223
12 [5] 0
13 223
14 [vv] 0
15 [vv] 0
16 [5] 0
17 [5] 103
18 [a,b] 0
19 [3] 0
20 [6] 0
21 CODE 0
22 CODE 0
23 CODE 0
24 CODE 0
25 CODE 0
26 STRING 0
27 CODE 0
28 CODE 0", SetupTests.RunWithInput(s));
    }
}
