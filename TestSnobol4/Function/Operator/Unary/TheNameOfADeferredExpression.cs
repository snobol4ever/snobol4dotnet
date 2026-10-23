using Test.TestLexer;

namespace Test.Operator.Unary;

/// <summary>
/// The name of a deferred expression (.*E) is a NAME in SPITBOL: the name of the variable when E is one (.*X is .X, IDENT to it and
/// with its text), else a name with no text that evaluates E by name each time it is used. $N after N = .*A&lt;I&gt; reads and
/// assigns A&lt;I&gt; for the I of that moment, $ of .*SIZE(X) reads SIZE(X) then, an error in E (22, 14) is the statement that uses the
/// name, and in a name's place what an expression that can only give a value gives is 249 ($(.*(X 'q')) = 1) and what a call gives 21.
/// Two such names are IDENT only as one. dotnet made error 21 of every .*E. Found by the infinite_snobol4 generator (.*E in its unary
/// operators). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class TheNameOfADeferredExpression
{
    [TestMethod]
    public void TEST_TheNameOfADeferredExpression_is_evaluated_by_name_when_used()
    {
        var program = @"        &ERRLIMIT = 20
        A = ARRAY(3)
        A<1> = 'one'
        A<2> = 'two'
        X = 'x'
        I = 1
        N = .*A<I>
        I = 2
        OUTPUT = '1 ' $N
        M = .*SIZE(X)
        X = 'xyz'
        OUTPUT = '2 ' $M
        $N = 'set'
        OUTPUT = '3 ' A<1> ' ' A<2>
        $M = 5                                  :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                :(E4)
Y4      OUTPUT = '4 S ' X
E4      K = .*X
        $K = 'via'
        OUTPUT = '5 ' X ' ' (IDENT(K, .X) 'ident', 'differ')
        OUTPUT = '6 ' DATATYPE(.*(X 'q')) ' ' $.*(X 'q')
        'abc' ? LEN(1) . $(.*A<3>)
        OUTPUT = '7 ' A<3>
        J = .*$'X'
        OUTPUT = '8 ' (IDENT(J, .X) 'ident', 'differ') ' ' $J
        L = .*F()
        OUTPUT = '9 ' DATATYPE(L)
        OUTPUT = '10 ' $L                       :S(E10)
        OUTPUT = '10 F ' &ERRTYPE
E10     OUTPUT = '11 ' (CONVERT(.*X, 'STRING'), 'no text') ' ' (CONVERT(.*A<1>, 'STRING'), 'no text') ' '
+              (CONVERT(.*SIZE(X), 'STRING'), 'no text')
END";
        Assert.AreEqual(@"1 two
2 3
3 one set
4 F 21
5 via ident
6 NAME viaq
7 a
8 differ via
9 NAME
10 F 22
11 X no text no text", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_TheNameOfADeferredExpression_in_a_names_place_and_its_identity()
    {
        var program = @"        &ERRLIMIT = 20
        X = 'x'
        A = ARRAY(3)
        $(.*(X 'q')) = 1                        :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                :(E1)
Y1      OUTPUT = '1 S'
E1      $(.*'abc') = 1                          :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                :(E2)
Y2      OUTPUT = '2 S'
E2      'abc' ? LEN(1) . $(.*(X 'q'))           :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                :(E3)
Y3      OUTPUT = '3 S'
E3      V = EVAL(.*SIZE(X))                     :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                :(E4)
Y4      OUTPUT = '4 S ' V
E4      Z = 0
        OUTPUT = '5 ' (IDENT(.*A<1>, .*A<1>) 'ident', 'differ') ' ' (IDENT(.*X, .*X) 'ident', 'differ')
        N = .*A<1>
        OUTPUT = '6 ' (IDENT(N, N) 'ident', 'differ')
        V = N + 1                               :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                :(E7)
Y7      OUTPUT = '7 S ' V
E7      V = .*(1 / Z)                           :S(Y8)
        OUTPUT = '8 F ' &ERRTYPE                :(E8)
Y8      OUTPUT = '8 S ' DATATYPE(V)
E8      OUTPUT = '9 ' $V                        :S(E9)
        OUTPUT = '9 F ' &ERRTYPE
E9      Y = .*$'X'
        $Y = 'yy'
        OUTPUT = '11 ' X
        A<2> = 'two'
        OUTPUT = '12 ' $.*A<1 + 1>
END";
        Assert.AreEqual(@"1 F 249
2 F 249
3 F 249
4 F 103
5 differ ident
6 ident
7 F 1
8 S NAME
9 F 14
11 yy
12 two", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
