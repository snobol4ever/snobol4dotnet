using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// UNLOAD undefines the function whose name it is given, any string at all or a NAME (a program-defined function stops being one; a name that
/// is a function of nothing is no error); the null string, a pattern or an array is 201, a system function 248. A name that looks like a path
/// did not find its library and ended the run with an unhandled exception. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class Unload
{
    [TestMethod]
    public void TEST_Unload_any_string_names_a_function_and_the_rest_are_201()
    {
        var s = @"
        &ERRLIMIT = 100
        P = LEN(2)
        A = ARRAY(2)
        E = ''
        DEFINE('G(X)')   :(SKIP)
G       G = X   :(RETURN)
SKIP    R1 = UNLOAD('G')
        OUTPUT = '1 [' R1 '] ' &ERRTYPE
        R2 = G('still')
        OUTPUT = '2 ' &ERRTYPE
        &ERRTYPE = 0
        R3 = UNLOAD('3.7')
        OUTPUT = '3 [' R3 '] ' &ERRTYPE
        R4 = UNLOAD('a/b.dll')
        OUTPUT = '4 [' R4 '] ' &ERRTYPE
        R5 = UNLOAD(7)
        OUTPUT = '5 [' R5 '] ' &ERRTYPE
        R6 = UNLOAD(.V)
        OUTPUT = '6 [' R6 '] ' &ERRTYPE
        R7 = UNLOAD(E)
        OUTPUT = '7 ' &ERRTYPE
        &ERRTYPE = 0
        R8 = UNLOAD(P)
        OUTPUT = '8 ' &ERRTYPE
        &ERRTYPE = 0
        R9 = UNLOAD(A)
        OUTPUT = '9 ' &ERRTYPE
        &ERRTYPE = 0
        R10 = UNLOAD('SIZE')
        OUTPUT = '10 ' &ERRTYPE
        &ERRTYPE = 0
        R11 = UNLOAD('NOSUCH')
        OUTPUT = '11 [' R11 '] ' &ERRTYPE
        R12 = UNLOAD('-5')
        OUTPUT = '12 [' R12 '] ' &ERRTYPE
END";
        Assert.AreEqual(@"1 [] 0
2 22
3 [] 0
4 [] 0
5 [] 0
6 [] 0
7 201
8 201
9 201
10 248
11 [] 0
12 [] 0", SetupTests.RunWithInput(s));
    }
}
