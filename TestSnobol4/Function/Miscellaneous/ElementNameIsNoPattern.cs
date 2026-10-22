using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// The name of an element (.A&lt;1&gt;, .T&lt;'k'&gt;) has no text, so it is neither a string nor a pattern: in a concatenation it is error 9 or 8,
/// in an alternation 6, as the pattern of a match 240, as SPITBOL says. It was taken as a pattern matching the null string. Found beside
/// the name-as-number class. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ElementNameIsNoPattern
{
    [TestMethod]
    public void TEST_ElementNameIsNoPattern_in_a_concatenation_an_alternation_or_a_match()
    {
        var program = @"        &ERRLIMIT = 30
        A = ARRAY(3)
        A<1> = 7
        T = TABLE()
        V = .A<1>
        &ERRTYPE = 0
        W = 'x' V                                   :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                    :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(W)
E1      &ERRTYPE = 0
        W = V 'x'                                   :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                    :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(W)
E2      &ERRTYPE = 0
        W = SIZE(V)                                 :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                    :(E3)
Y3      OUTPUT = '3 S ' W
E3      &ERRTYPE = 0
        W = 'x' .T<'k'>                             :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                    :(E4)
Y4      OUTPUT = '4 S ' DATATYPE(W)
E4      &ERRTYPE = 0
        W = V | 'x'                                 :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                    :(E5)
Y5      OUTPUT = '5 S ' DATATYPE(W)
E5      &ERRTYPE = 0
        'axb' V                                     :S(Y6)
        OUTPUT = '6 F ' &ERRTYPE                    :(E6)
Y6      OUTPUT = '6 S'
E6      &ERRTYPE = 0
        W = 'x' .X                                  :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                    :(E7)
Y7      OUTPUT = '7 S ' DATATYPE(W) ' ' W
E7
END";
        Assert.AreEqual(@"1 F 9
2 F 8
3 F 189
4 F 9
5 F 6
6 F 240
7 S STRING xX", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
