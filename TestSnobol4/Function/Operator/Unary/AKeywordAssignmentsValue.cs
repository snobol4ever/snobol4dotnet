using Test.TestLexer;

namespace Test.Operator.Unary;

/// <summary>
/// The value of an assignment to a keyword is the value assigned, as it was given, as in SPITBOL: (&amp;ANCHOR = ' 1') is the string
/// ' 1', (&amp;ANCHOR = 2.5) the real 2.5, (&amp;TRIM = '') the null string, (&amp;ERRTEXT = 5) the integer 5, while the keyword keeps the
/// integer or the string it takes from it; so BREAK((&amp;FULLSCAN = 'c')) is error 208 for the 'c' and LEN((&amp;TRIM = '1')) is LEN of
/// '1'. dotnet gave the keyword's own value (the integer, the keyword's text). Found by the infinite_snobol4 generator with keyword
/// assignments one call in eight (change 19: (&amp;FULLSCAN = '2'), BREAK((&amp;TRIM = '')), seeds 386790 and 641681). Every expected
/// line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AKeywordAssignmentsValue
{
    [TestMethod]
    public void TEST_AKeywordAssignmentsValue_is_the_value_assigned()
    {
        var program = @"        &ERRLIMIT = 10
        V = (&ANCHOR = ' 1')
        OUTPUT = '1 ' DATATYPE(V) ' [' V '] ' &ANCHOR
        V = (&ANCHOR = 2.5)
        OUTPUT = '2 ' DATATYPE(V) ' [' V '] ' &ANCHOR
        V = (&TRIM = '')
        OUTPUT = '3 ' DATATYPE(V) ' [' V '] ' &TRIM
        V = (&ERRTEXT = 5)
        OUTPUT = '4 ' DATATYPE(V) ' [' V '] ' &ERRTEXT
        V = (&MAXLNGTH = '5000')
        OUTPUT = '5 ' DATATYPE(V) ' [' V '] ' &MAXLNGTH
        V = (&ANCHOR = '0') 'z'
        OUTPUT = '6 ' DATATYPE(V) ' [' V '] ' &ANCHOR
        'abc' ? BREAK((&FULLSCAN = 'c')) . W                :S(Y7)
        OUTPUT = '7 F ' &ERRTYPE                            :(E7)
Y7      OUTPUT = '7 S ' W
E7      'abc' ? LEN((&TRIM = '1')) . W
        OUTPUT = '8 ' W ' ' &TRIM
END";
        Assert.AreEqual(@"1 STRING [ 1] 1
2 REAL [2.5] 2
3 STRING [] 0
4 INTEGER [5] 5
5 STRING [5000] 5000
6 STRING [0z] 0
7 F 208
8 a 1", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
