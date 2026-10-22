using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A NAME kept in a DATA object and read back (the cons cells of a library of lists) is still the name of its variable: APPLY, ARG, LOCAL, TRACE, STOPTR, FIELD, OPSYN, EVAL, CODE and UNLOAD take it as the name of a function. The reading of the name carries the place it was kept, and a check for the name of an element took that for one; the aisnobol suite's TEST program traces a function through such a name. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class StoredNames
{
    [TestMethod]
    public void TEST_StoredNames_names_kept_in_a_data_object()
    {
        var s = @"        &ERRLIMIT = 100
        DEFINE('F(A)B')   :(SKIP)
F       F = A   :(RETURN)
SKIP    DATA('NODE(HEAD,TAIL)')
        D = NODE(.F, 'x')
        N3 = HEAD(D)
        OUTPUT = '1 ' DATATYPE(N3) ' ' DATATYPE(HEAD(D))
        OUTPUT = '2 ' APPLY(N3, 'three')
        OUTPUT = '3 ' APPLY(HEAD(D), 'direct')
        OUTPUT = '4 ' ARG(HEAD(D), 1) ' ' LOCAL(HEAD(D), 1)
        OUTPUT = '5 ' ARG(N3, 1)
        R = TRACE(HEAD(D), 'CALL')
        OUTPUT = '6 [' R '] ' &ERRTYPE
        R = TRACE(N3, 'RETURN')
        OUTPUT = '7 [' R '] ' &ERRTYPE
        R = STOPTR(N3, 'CALL')
        OUTPUT = '8 [' R '] ' &ERRTYPE
        R = STOPTR(HEAD(D), 'RETURN')
        OUTPUT = '9 [' R '] ' &ERRTYPE
        R = FIELD(N3, 1)
        OUTPUT = '10 ' &ERRTYPE
        &ERRTYPE = 0
        R = OPSYN('F2', HEAD(D))
        OUTPUT = '11 ' F2('opsyn') ' ' &ERRTYPE
        X = EVAL(HEAD(D))
        OUTPUT = '12 [' X '] ' &ERRTYPE
        C = CODE(HEAD(D))
        OUTPUT = '13 ' DATATYPE(C) ' ' &ERRTYPE
        R = UNLOAD(HEAD(D))
        OUTPUT = '14 [' R '] ' &ERRTYPE
        OUTPUT = '15 ' ITEM(NODE(1,2), 1)
END";
        Assert.AreEqual(@"1 NAME NAME
2 three
3 direct
4 A B
5 A
6 [] 0
7 [] 0
8 [] 0
9 [] 0
10 108
11 opsyn 0
12 [] 0
13 CODE 0
14 [] 0", SetupTests.RunWithInput(s));
    }
}
