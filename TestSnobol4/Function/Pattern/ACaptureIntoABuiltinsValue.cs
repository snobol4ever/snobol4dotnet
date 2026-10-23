using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A capture (. or $) or a cursor assignment (@) into a builtin's value has no name to assign: error 21, function called by name
/// returned a value, as the pattern is built, as in SPITBOL (LEN(1) . BREAK('a'), LEN(1) . SIZE('a'), @SIZE('a')). A call that gives
/// a name (ITEM, a DATA field, EVAL of a variable), $, a keyword, OUTPUT and a deferred expression are names, and the pattern is built.
/// dotnet refused only a RETURN's value there, so the pattern was built and its capture went nowhere. Found by the infinite_snobol4
/// generator ('[' ?FENCE(LEN(1)) . BREAK('a'), seed 574270). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ACaptureIntoABuiltinsValue
{
    [TestMethod]
    public void TEST_ACaptureIntoABuiltinsValue_is_error_21_as_the_pattern_is_built()
    {
        var program = @"        &ERRLIMIT = 100
        X = 'x'
        A = ARRAY(2)
        DATA('PT(PX)')
        V = PT(1)
        P = LEN(1) . BREAK('a')                     :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                    :(E1)
Y1      OUTPUT = '1 ' DATATYPE(P)
E1
        P = LEN(1) $ BREAK('a')                     :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                    :(E2)
Y2      OUTPUT = '2 ' DATATYPE(P)
E2
        P = @BREAK('a')                             :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                    :(E3)
Y3      OUTPUT = '3 ' DATATYPE(P)
E3
        P = LEN(1) . SIZE('a')                      :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 ' DATATYPE(P)
E4
        P = LEN(1) . TRIM(X)                        :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                    :(E5)
Y5      OUTPUT = '5 ' DATATYPE(P)
E5
        P = LEN(1) . CONVERT(X, 'STRING')           :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                    :(E6)
Y6      OUTPUT = '6 ' DATATYPE(P)
E6
        P = @SIZE('a')                              :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                    :(E7)
Y7      OUTPUT = '7 ' DATATYPE(P)
E7
        P = LEN(1) . ITEM(A, 1)                     :S(Y8)
        OUTPUT = '8 F ' &ERRTYPE                    :(E8)
Y8      OUTPUT = '8 ' DATATYPE(P)
E8
        P = LEN(1) . PX(V)                          :S(Y9)
        OUTPUT = '9 F ' &ERRTYPE                    :(E9)
Y9      OUTPUT = '9 ' DATATYPE(P)
E9
        P = LEN(1) . $'X'                           :S(Y10)
        OUTPUT = '10 F ' &ERRTYPE                   :(E10)
Y10     OUTPUT = '10 ' DATATYPE(P)
E10
        P = LEN(1) . EVAL('X')                      :S(Y11)
        OUTPUT = '11 F ' &ERRTYPE                   :(E11)
Y11     OUTPUT = '11 ' DATATYPE(P)
E11
        P = LEN(1) . &TRACE                         :S(Y12)
        OUTPUT = '12 F ' &ERRTYPE                   :(E12)
Y12     OUTPUT = '12 ' DATATYPE(P)
E12
        P = LEN(1) . *SIZE(X)                       :S(Y13)
        OUTPUT = '13 F ' &ERRTYPE                   :(E13)
Y13     OUTPUT = '13 ' DATATYPE(P)
E13
        P = LEN(1) $ ITEM(A, 1)                     :S(Y14)
        OUTPUT = '14 F ' &ERRTYPE                   :(E14)
Y14     OUTPUT = '14 ' DATATYPE(P)
E14
END";
        Assert.AreEqual(@"1 F 21
2 F 21
3 F 21
4 F 21
5 F 21
6 F 21
7 F 21
8 PATTERN
9 PATTERN
10 PATTERN
11 PATTERN
12 PATTERN
13 PATTERN
14 PATTERN", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
