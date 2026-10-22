using Test.TestLexer;

namespace Test.Numeric;

/// <summary>
/// Numbers at the edges of the integers and the doubles, as sbl takes them: a real REMDR is chop(n1/n2) * n2 - n1 made to take the sign of n1, so it is positive for a positive first argument when the quotient is too big for the product to be exact (REMDR(3.0, 1.0E-5) is 0.444089209850063E-15); the least integer REMDR -1 is 0; a string of digits one past the largest integer is no number (error 1, 10 and 102) though with a point or an exponent it is a real; a real beyond the integers converts to the least integer, so DUPL fails, LPAD pads nothing and SUBSTR fails for it; 0 raised to 2147483648 is 0. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NumericEdges
{
    [TestMethod]
    public void TEST_NumericEdges_numbers_at_the_edges()
    {
        var s = @"        &ERRLIMIT = 100
        R = 3.0
        S = 1.0E-5
        X = REMDR(R, S)
        OUTPUT = '1 ' X
        X = REMDR(7, S)
        OUTPUT = '2 ' X
        X = REMDR(1.0E17, 0.3)
        OUTPUT = '3 ' X
        X = REMDR(-1.0E17, 0.3)
        OUTPUT = '4 ' X
        X = REMDR(1.5, 1.0E-5)
        OUTPUT = '5 ' X
        M = -9223372036854775807 - 1
        N = -1
        X = REMDR(M, N)
        OUTPUT = '6 ' X ' ' &ERRTYPE
        S2 = '9223372036854775808'
        X = S2 + 0
        OUTPUT = '7 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        X = -S2
        OUTPUT = '8 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        X = EQ(S2, S2)
        OUTPUT = '9 ' &ERRTYPE
        &ERRTYPE = 0
        X = CONVERT(S2, 'REAL')
        OUTPUT = '10 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        S3 = '9223372036854775808.'
        X = S3 + 0
        OUTPUT = '11 ' DATATYPE(X) ' ' X
        S4 = '-9223372036854775808'
        X = S4 + 0
        OUTPUT = '12 ' DATATYPE(X) ' ' X
        S5 = '92233720368547758080'
        X = S5 + 0
        OUTPUT = '13 [' X '] ' &ERRTYPE
        &ERRTYPE = 0
        BIG = 1.0E19
        X = CONVERT(BIG, 'INTEGER')
        OUTPUT = '14 ' DATATYPE(X) ' ' X
        X = CONVERT('1e19', 'INTEGER')
        OUTPUT = '15 ' DATATYPE(X) ' ' X
        HUGE = 1.0E100
        X = DUPL('x', HUGE)  :S(O16)F(F16)
O16     OUTPUT = '16 ok [' X ']'  :(N16)
F16     OUTPUT = '16 fails ' &ERRTYPE
N16     X = LPAD('x', HUGE)
        OUTPUT = '17 [' X '] ' &ERRTYPE
        X = SUBSTR('abc', 1, HUGE)  :S(O18)F(F18)
O18     OUTPUT = '18 ok [' X ']'  :(N18)
F18     OUTPUT = '18 fails ' &ERRTYPE
N18     Z = 0
        B = 2147483648
        X = Z ** B
        OUTPUT = '19 ' X ' ' &ERRTYPE
        X = Z ** 4294967296
        OUTPUT = '20 ' X ' ' &ERRTYPE
END";
        Assert.AreEqual(@"1 0.444089209850063e-15
2 0.888178419700125e-15
3 16.
4 -16.
5 0.222044604925031e-15
6 0 0
7 [0] 1
8 [0] 10
9 102
10 [0] 0
11 REAL 0.922337203685478e+19
12 INTEGER -9223372036854775808
13 [-9223372036854775808] 1
14 INTEGER -9223372036854775808
15 INTEGER -9223372036854775808
16 fails 0
17 [x] 0
18 fails 0
19 0 0
20 0 0", SetupTests.RunWithInput(s));
    }
}
