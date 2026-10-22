using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// The NAME of a variable assigned to an element of an array or table stays a NAME (L<1> = .F: DATATYPE(L<1>) is NAME, X = L<1> gives the NAME, and $L<1> = 'x' sets F); it was read through and stored as the variable's value. A function that returns a NAME with NRETURN gives its value there, as before. Not done: the NAME of an element stored in an element. Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class StoredNameValues
{
    [TestMethod]
    public void TEST_StoredNameValues_names_in_elements()
    {
        var s = @"        &ERRLIMIT = 100
        DEFINE('NR()')   :(SKIP)
NR      NR = .Z   :(NRETURN)
SKIP    F = 'fvalue'
        Z = 'zvalue'
        L = ARRAY(4)
        T = TABLE()
        L<1> = .F
        OUTPUT = '1 ' DATATYPE(L<1>)
        N = .F
        L<2> = N
        OUTPUT = '2 ' DATATYPE(L<2>)
        T<'k'> = .F
        OUTPUT = '3 ' DATATYPE(T<'k'>)
        T<'j'> = N
        OUTPUT = '4 ' DATATYPE(T<'j'>)
        L<3> = NR()
        OUTPUT = '5 ' DATATYPE(L<3>) ' ' L<3>
        $L<1> = 'set through name'
        OUTPUT = '6 ' F
        $(T<'k'>) = 'again'
        OUTPUT = '7 ' F
        X = L<2>
        OUTPUT = '8 ' DATATYPE(X) ' ' $X
        NR() = 'via nreturn'
        OUTPUT = '12 ' Z
        L<3> = 'x'
        OUTPUT = '13 ' DATATYPE(L<3>)
END";
        Assert.AreEqual(@"1 NAME
2 NAME
3 NAME
4 NAME
5 STRING zvalue
6 set through name
7 again
8 NAME again
12 via nreturn
13 STRING", SetupTests.RunWithInput(s));
    }
}
