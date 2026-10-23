using Test.TestLexer;

namespace Test.Operator.Unary;

/// <summary>
/// The name of a keyword (.&amp;ERRTEXT) has no text, as the name of an element has none, in SPITBOL: it is a NAME that $ reads and
/// assigns through, but no string, number or pattern, so CONVERT to STRING or PATTERN fails, SIZE is 189, a match on it 241, a
/// concatenation 8, an addition 1, LGT 126, EVAL 103 and CODE fails, and OUTPUT prints NAME. dotnet took the keyword's own name
/// (&amp;errtext) for its text. Found by the infinite_snobol4 generator (.&amp;TRACE 'l' 'g' | '{}'). Every expected line is what
/// sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AKeywordsNameHasNoText
{
    [TestMethod]
    public void TEST_AKeywordsNameHasNoText_is_no_string_number_or_pattern()
    {
        var program = @"        &ERRLIMIT = 10
        N = .&ERRTEXT
        OUTPUT = '1 ' DATATYPE(N)
        S = CONVERT(N, 'STRING')        :S(Y2)
        OUTPUT = '2 F'                  :(E2)
Y2      OUTPUT = '2 S [' S ']'
E2      L = SIZE(N)                     :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE        :(E3)
Y3      OUTPUT = '3 S ' L
E3      N ? 'x'                         :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE        :(E4)
Y4      OUTPUT = '4 S'
E4      V = N 'z'                       :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE        :(E5)
Y5      OUTPUT = '5 S ' V
E5      &ERRTEXT = 'boo'
        OUTPUT = '6 ' $N
        P = CONVERT(N, 'PATTERN')       :S(Y7)
        OUTPUT = '7 F'                  :(E7)
Y7      OUTPUT = '7 S'
E7      C = CODE(N)                     :S(Y8)
        OUTPUT = '8 F'                  :(E8)
Y8      OUTPUT = '8 S'
E8      I = N + 1                       :S(Y9)
        OUTPUT = '9 F ' &ERRTYPE        :(E9)
Y9      OUTPUT = '9 S ' I
E9      X = EVAL(N)                     :S(Y10)
        OUTPUT = '10 F ' &ERRTYPE       :(E10)
Y10     OUTPUT = '10 S ' X
E10     'abc' ? LEN(1) . N
        OUTPUT = '11 ' &ERRTEXT
        E = CONVERT(N, 'EXPRESSION')    :S(Y12)
        OUTPUT = '12 F ' &ERRTYPE       :(E12)
Y12     OUTPUT = '12 S ' DATATYPE(E)
E12     OUTPUT = '13 ' (LGT(N, 'a') 'gt', 'not')
        N = .&ERRTEXT
        OUTPUT = N                      :S(E14)
        OUTPUT = '14 F ' &ERRTYPE
E14     OUTPUT = '15 ' (IDENT(N, N) 'ident', 'differ') ' ' (DIFFER(N, '&ERRTEXT') 'differs', 'same')
        A = ARRAY(2)
        OUTPUT = .A<1>                  :S(E16)
        OUTPUT = '16 F ' &ERRTYPE
E16     OUTPUT = '17 ' (LGT(N, 'a') 'gt', 'not ' &ERRTYPE)
END";
        Assert.AreEqual(@"1 NAME
2 F
3 F 189
4 F 241
5 F 8
6 boo
7 F
8 F
9 F 1
10 F 103
11 eval argument is not expression
12 S EXPRESSION
13 not
NAME
15 ident differs
NAME
17 not 126", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
