using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A value that is an object (an array, a table, an expression, a code, a pattern, a DATA object) is the same object wherever it is copied: concatenating it with the null string gives it back (IDENT(E V, V) holds, where an array or table came back as a copy of its elements and an expression or code as a new object that was not IDENT to it), and R = V leaves IDENT(R, V). Every expected line is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SharedValues
{
    [TestMethod]
    public void TEST_SharedValues_null_concatenation_returns_the_same_object()
    {
        var s = @"        &ERRLIMIT = 100
        DATA('NODE(HEAD,TAIL)')
        E = ''
        V1 = ARRAY(2)
        V2 = TABLE()
        V3 = NODE(1,2)
        V4 = LEN(2)
        V5 = *E
        V6 = .E
        V7 = CODE(' X = 1')
        I = 0
L       I = I + 1
        V = $('V' I)
        R = E V
        OUTPUT = I ' left ' DATATYPE(R) ' ' IDENT(R, V) ' ' &ERRTYPE
        &ERRTYPE = 0
        R = V E
        OUTPUT = I ' right ' DATATYPE(R) ' ' IDENT(R, V) ' ' &ERRTYPE
        &ERRTYPE = 0
        R = E V E
        OUTPUT = I ' both ' DATATYPE(R) ' ' IDENT(R, V) ' ' &ERRTYPE
        &ERRTYPE = 0
        R = E V V
        OUTPUT = I ' two ' DATATYPE(R) ' ' &ERRTYPE
        &ERRTYPE = 0
        LT(I, 7)   :S(L)
END";
        Assert.AreEqual(@"1 left ARRAY  0
1 right ARRAY  0
1 both ARRAY  0
1 two ARRAY 8
2 left TABLE  0
2 right TABLE  0
2 both TABLE  0
2 two TABLE 8
3 left NODE  0
3 right NODE  0
3 both NODE  0
3 two NODE 8
4 left PATTERN  0
4 right PATTERN  0
4 both PATTERN  0
4 two PATTERN 0
5 left EXPRESSION  0
5 right EXPRESSION  0
5 both EXPRESSION  0
5 two PATTERN 0
6 left NAME  0
6 right NAME  0
6 both NAME  0
6 two STRING 0
7 left CODE  0
7 right CODE  0
7 both CODE  0
7 two CODE 8", SetupTests.RunWithInput(s));
    }

    [TestMethod]
    public void TEST_SharedValues_assigning_an_expression_or_code_keeps_its_identity()
    {
        var s = @"        E = ''
        V5 = *E
        V7 = CODE(' X = 1')
        R5 = V5
        R7 = V7
        OUTPUT = '1 ' IDENT(R5, V5) 'expr same'
        OUTPUT = '2 ' IDENT(R7, V7) 'code same'
        OUTPUT = '3 ' IDENT(V5, V5) 'expr self'
        OUTPUT = '4 ' IDENT(V7, V7) 'code self'
        W5 = *E
        OUTPUT = '5 ' IDENT(V5, W5) 'expr two'
        W7 = CODE(' X = 1')
        OUTPUT = '6 ' IDENT(V7, W7) 'code two'
        P = LEN(2)
        Q = P
        OUTPUT = '7 ' IDENT(P, Q) 'pat same'
        OUTPUT = '8 ' IDENT(LEN(2), LEN(2)) 'pat two'
        N1 = .E
        N2 = .E
        OUTPUT = '9 ' IDENT(N1, N2) 'name same'
        OUTPUT = '10 ' IDENT(V7, V5) 'diff'
END";
        Assert.AreEqual(@"1 expr same
2 code same
3 expr self
4 code self
7 pat same
9 name same", SetupTests.RunWithInput(s));
    }
}
