using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// TRACE and STOPTR as SPITBOL's trace procedure does them: a null first argument to TRACE only toggles the system trace switch and is never
/// an error; the type is taken by its first letter (A, V or blank and the null string, F, R, C, L, K), and a bad type (199, STOPTR's 191) is
/// reported before a bad name (198, 190): a call, return or function trace names a program function, a label trace a label that exists, a
/// keyword trace ERRTYPE, FNCLEVEL or STCOUNT; the fourth argument names the trace function (197 when it is none); TRACE returns the null
/// string; and a trace of &ERRTYPE fires when the program assigns it. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceTypes
{
    [TestMethod]
    public void TEST_TraceTypes_the_type_name_and_function_are_checked_as_sbl_checks_them()
    {
        // the program starts on the first line of the string, so its statement numbers are the ones the trace lines carry
        var s = @"        &ERRLIMIT = 1000
        &TRIM = 0
        P = LEN(2)
        E = ''
        DEFINE('F(A)')   :(SKIP)
F       F = A   :(RETURN)
SKIP    DEFINE('H(NAME,TAG)')   :(SKIP2)
H       OUTPUT = 'handler ' NAME ' ' TAG   :(RETURN)
SKIP2
L1      OUTPUT = 'at L1'
        &TRACE = 100
        R1 = TRACE('F', 'CALL')
        OUTPUT = '1 ' &ERRTYPE
        R2 = TRACE('F', 'call')
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        R3 = TRACE('G', 'CALL')
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        R4 = TRACE('L1', 'LABEL')
        OUTPUT = '4 ' &ERRTYPE
        R5 = TRACE('NOLABEL', 'LABEL')
        OUTPUT = '5 ' &ERRTYPE
        &ERRTYPE = 0
        R6 = TRACE('ERRTYPE', 'KEYWORD')
        OUTPUT = '6 ' &ERRTYPE
        R7 = TRACE('&ERRTYPE', 'K')
        OUTPUT = '7 ' &ERRTYPE
        &ERRTYPE = 0
        R8 = TRACE('STLIMIT', 'K')
        OUTPUT = '8 ' &ERRTYPE
        &ERRTYPE = 0
        R9 = TRACE('X', 'V', 'tag', 'H')
        OUTPUT = '9 ' &ERRTYPE
        R10 = TRACE('Y', 'V', 'tag', 'NOFUNC')
        OUTPUT = '10 ' &ERRTYPE
        R11 = TRACE('Z', 'V', 'tag', 5)
        OUTPUT = '11 ' &ERRTYPE
        R12 = TRACE('W', 'V', 'tag', P)
        OUTPUT = '12 ' &ERRTYPE
        &ERRTYPE = 0
        R13 = TRACE(E, 'bogus')
        OUTPUT = '13 ' &ERRTYPE
        R14 = TRACE('bogus', E)
        OUTPUT = '14 ' &ERRTYPE
        X = 'first'
        X = 'second'
        Q = F('arg')
        R15 = STOPTR('X', 'V')
        OUTPUT = '15 ' &ERRTYPE
        X = 'third'
        R16 = STOPTR('F', 'CALL')
        OUTPUT = '16 ' &ERRTYPE
        R17 = STOPTR('NOLABEL', 'L')
        OUTPUT = '17 ' &ERRTYPE
        &ERRTYPE = 0
        R18 = STOPTR('X', 'Q')
        OUTPUT = '18 ' &ERRTYPE
        &ERRTYPE = 0
        R19 = STOPTR('', 'V')
        OUTPUT = '19 ' &ERRTYPE
        &ERRTYPE = 0
        R20 = TRACE('A1', 'ACCESS')
        T1 = A1
        T2 = A1
        R21 = TRACE('ERRTYPE', 'K')
        &ERRTYPE = 0
        R22 = TRACE(E, E, E, E)
        OUTPUT = '22 ' &ERRTYPE
        :(L1X)
L1X     OUTPUT = 'done'
END";
        Assert.AreEqual(@"at L1
1 0
2 0
3 198
4 0
5 198
6 0
****27******  &ERRTYPE = 198
7 198
****29******  &ERRTYPE = 0
****30******  &ERRTYPE = 198
8 198
****32******  &ERRTYPE = 0
9 0
10 0
11 0
****39******  &ERRTYPE = 197
12 197
****41******  &ERRTYPE = 0
13 0
14 0
handler X tag
handler X tag
****48******  F('arg')
15 0
16 0
****54******  &ERRTYPE = 190
17 190
****56******  &ERRTYPE = 0
****57******  &ERRTYPE = 191
18 191
****59******  &ERRTYPE = 0
****60******  &ERRTYPE = 190
19 190
****62******  &ERRTYPE = 0
****64******  A1 = ''
****65******  A1 = ''
****67******  &ERRTYPE = 0
22 0
done", SetupTests.RunWithInput(s));
    }
}
