using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// What a subscript is applied to and what it is: a value that is neither an array nor a table is error 235 (it used to empty the stack
/// and end the run with an unhandled exception), an array index that is itself an array or table is 238, and a table's key may be an
/// array or table. The lexer counts the subscripts of each reference, so the object subscripted is known and not looked for on the stack.
/// Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class Subscripts
{
    [TestMethod]
    public void TEST_Subscripts_each_reference_gives_the_result_and_error_sbl_gives()
    {
        var s = @"
        &ERRLIMIT = 100
        DATA('D(X,Y,Z)')
        S = 'abc'
        N = 12
        R = 2.5
        E = ''
        P = LEN(2)
        V = D(1,2,3)
        A3 = ARRAY(3)
        A3<2> = 'two'
        T3 = TABLE()
        K1 = ARRAY(2)
        K2 = TABLE()
        L = S<1>
        OUTPUT = '1 ' &ERRTYPE
        &ERRTYPE = 0
        L = N<1>
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        L = R<1,2>
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        L = E<1>
        OUTPUT = '4 ' &ERRTYPE
        &ERRTYPE = 0
        L = P<1>
        OUTPUT = '5 ' &ERRTYPE
        &ERRTYPE = 0
        L = V<1>
        OUTPUT = '6 ' &ERRTYPE
        &ERRTYPE = 0
        L = A3<K1>
        OUTPUT = '7 ' &ERRTYPE
        &ERRTYPE = 0
        L = A3<K2>
        OUTPUT = '8 ' &ERRTYPE
        &ERRTYPE = 0
        T3<K1> = 'by array'
        T3<K2> = 'by table'
        OUTPUT = '9 ' T3<K1> ' ' T3<K2> ' ' &ERRTYPE
        K3 = K1
        OUTPUT = '10 ' T3<K3>
        K4 = ARRAY(2)
        OUTPUT = '11 [' T3<K4> ']'
        L = A3<1,2>
        OUTPUT = '12 ' &ERRTYPE
        &ERRTYPE = 0
        L = T3<1,2>
        OUTPUT = '13 ' &ERRTYPE
        &ERRTYPE = 0
        L = T3<>
        OUTPUT = '14 ' &ERRTYPE
        &ERRTYPE = 0
        L = A3<>
        OUTPUT = '15 ' &ERRTYPE
        &ERRTYPE = 0
        A3<2> = 'set'
        OUTPUT = '16 ' A3<2> ' ' &ERRTYPE
        S<1> = 'x'
        OUTPUT = '17 ' &ERRTYPE
        &ERRTYPE = 0
        L = A3<4>
        OUTPUT = '18 ' &ERRTYPE
        &ERRTYPE = 0
        A3<2>[1] = 'y'
        OUTPUT = '19 ' &ERRTYPE
        &ERRTYPE = 0
        L = S[1]
        OUTPUT = '20 ' &ERRTYPE
        &ERRTYPE = 0
        L = A3[2]
        OUTPUT = '21 ' L
        L = ITEM(A3, 2)
        OUTPUT = '22 ' L
        L = ITEM(S, 1)
        OUTPUT = '23 ' &ERRTYPE
END";
        Assert.AreEqual(@"1 235
2 235
3 235
4 235
5 235
6 235
7 238
8 238
9 by array by table 0
10 by array
11 []
12 236
13 237
14 0
15 0
16 set 0
17 235
18 0
19 235
20 235
21 set
22 set
23 235", SetupTests.RunWithInput(s));
    }
}
