using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// EJECT, BACKSPACE, SET, INPUT, OUTPUT and DATE as sbl takes them: EJECT() writes a form feed to standard output and EJECT(n) a form feed and a line end to the file open for output on channel n (a name, a channel with nothing open, or standard output by name is 93; a value that is no string is 92); BACKSPACE of the null string is 316; SET looks for its file (295) before its offset and origin, a non-integer offset or origin is 293 and an origin that is an integer but not 0, 1 or 2 is 297; INPUT and OUTPUT return the null string when they associate a name; DATE takes an integer (330 otherwise) and gives MM/DD/YY HH:MM:SS, MM/DD/YYYY for 1 and YYYY-MM-DD for 2. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FileFunctions
{
    [TestMethod]
    public void TEST_FileFunctions_eject_backspace_input_output_and_date()
    {
        var s = @"        &ERRLIMIT = 100
        E = ''
        P = LEN(2)
        R1 = OUTPUT(.F, 7, 'ejt.out')
        OUTPUT = '1 [' R1 '] ' &ERRTYPE
        F = 'x'
        R2 = EJECT(7)
        OUTPUT = '2 [' R2 '] ' &ERRTYPE
        F = 'y'
        R3 = EJECT(.F)
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        R4 = EJECT(8)
        OUTPUT = '4 ' &ERRTYPE
        &ERRTYPE = 0
        R5 = EJECT('OUTPUT')
        OUTPUT = '5 ' &ERRTYPE
        &ERRTYPE = 0
        R6 = EJECT(-1)
        OUTPUT = '6 ' &ERRTYPE
        &ERRTYPE = 0
        R7 = EJECT(P)
        OUTPUT = '7 ' &ERRTYPE
        &ERRTYPE = 0
        R8 = BACKSPACE(E)
        OUTPUT = '8 ' &ERRTYPE
        &ERRTYPE = 0
        R9 = BACKSPACE(9)
        OUTPUT = '9 ' &ERRTYPE
        &ERRTYPE = 0
        R10 = SET(9, 'x', 'y')
        OUTPUT = '10 ' &ERRTYPE
        &ERRTYPE = 0
        R11 = SET(E, 1, 0)
        OUTPUT = '11 ' &ERRTYPE
        &ERRTYPE = 0
        R12 = SET(P, 1, 0)
        OUTPUT = '12 ' &ERRTYPE
        &ERRTYPE = 0
        R13 = INPUT(.G, 8, 'ejt.out')
        OUTPUT = '13 [' R13 '] ' &ERRTYPE
        R14 = INPUT(.H, 8)
        OUTPUT = '14 [' R14 '] ' &ERRTYPE
        R15 = SET(8, 'x', 0)
        OUTPUT = '15 ' &ERRTYPE
        &ERRTYPE = 0
        R16 = SET(8, 0, 5)
        OUTPUT = '16 ' &ERRTYPE
        &ERRTYPE = 0
        R17 = SET(8, 1, 0)
        OUTPUT = '17 [' R17 '] ' &ERRTYPE
        R18 = OUTPUT(.J, 7)
        OUTPUT = '18 [' R18 '] ' &ERRTYPE
        R19 = OUTPUT(.K, 6, 'ejt2.out')
        OUTPUT = '19 [' R19 '] ' &ERRTYPE
        R20 = OUTPUT(.L, 6)
        OUTPUT = '20 [' R20 '] ' &ERRTYPE
        R21 = OUTPUT(.M)
        OUTPUT = '21 [' R21 '] ' &ERRTYPE
        R22 = OUTPUT(.N, E, E)
        OUTPUT = '22 [' R22 '] ' &ERRTYPE
        N = 'to stdout'
        R23 = DATE()
        OUTPUT = '23 ' SIZE(R23) ' ' &ERRTYPE
        R24 = DATE(1)
        OUTPUT = '24 ' SIZE(R24) ' ' &ERRTYPE
        R25 = DATE(2)
        OUTPUT = '25 ' SIZE(R25) ' ' &ERRTYPE
        R26 = DATE(5)
        OUTPUT = '26 ' SIZE(R26) ' ' &ERRTYPE
        R27 = DATE('x')
        OUTPUT = '27 ' &ERRTYPE
        &ERRTYPE = 0
        ENDFILE(8)
        ENDFILE(7)
        ENDFILE(6)
END";
        Assert.AreEqual(@"1 [] 0
2 [] 0
3 93
4 93
5 93
6 93
7 92
8 316
9 317
10 295
11 292
12 291
13 [] 0
14 [] 0
15 293
16 297
17 [0] 0
18 [] 0
19 [] 0
20 [] 0
21 [] 0
22 [] 0
to stdout
23 17 0
24 19 0
25 19 0
26 17 0
27 330", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_FileFunctions_set_offset_and_origin()
    {
        var s = @"        &ERRLIMIT = 100
        OUTPUT(.W, 9, 'sf.txt')
        W = 'x'
        W = 'y'
        W = 'z'
        ENDFILE(9)
        INPUT(.G, 8, 'sf.txt')
        R = SET(8, 0, 'x')
        OUTPUT = '1 ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 0, -1)
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 0, 3)
        OUTPUT = '3 ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 0, 1.5)
        OUTPUT = '4 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, -1, 0)
        OUTPUT = '5 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 100, 0)
        OUTPUT = '6 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 0, 2)
        OUTPUT = '7 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, -2, 2)
        OUTPUT = '8 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 'x', 'y')
        OUTPUT = '9 ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 1.5, 0)
        OUTPUT = '10 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 2, 1)
        OUTPUT = '11 [' R '] ' &ERRTYPE
        &ERRTYPE = 0
        R = SET(8, 2, 100000000000)
        OUTPUT = '12 [' R '] ' &ERRTYPE
END";
        Assert.AreEqual(@"1 293
2 297
3 297
4 [] 293
5 [0] 0
6 [6] 0
7 [6] 0
8 [4] 0
9 293
10 [4] 293
11 [6] 0
12 [6] 297", SetupTests.RunWithInput(s));
    }
}
