using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A failure or an error before an assignment inside an expression stops the statement's evaluation, as in SPITBOL: the assignment is not
/// made (-A + (Y = 2) with A not numeric leaves Y as it was). With &amp;ERRLIMIT above 0 the assignment was made and the statement then ended
/// on an empty stack, an InvalidOperationException that killed the run. Found by the infinite_snobol4 generator with embedded assignments
/// in its closed world (-a[5] ! -(Y = '(')). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class EmbeddedAssignmentAfterAnError
{
    [TestMethod]
    public void TEST_EmbeddedAssignmentAfterAnError_is_not_made()
    {
        var program = @"        &ERRLIMIT = 20
        A = 'f'
        Z = 1
        T = TABLE()
        DEFINE('F()')                               :(FE)
F                                                   :(FRETURN)
FE      V = -A + (Y = 2)
        OUTPUT = '1 ' &ERRTYPE ' Y=' Y
        V = (Z / 0) + (Y = 2)
        OUTPUT = '2 ' &ERRTYPE ' Y=' Y
        V = SIZE(T) (Y = 2)
        OUTPUT = '3 ' &ERRTYPE ' Y=' Y
        &ERRTYPE = 0
        V = F() (Y = 2)
        OUTPUT = '4 ' &ERRTYPE ' Y=' Y
        V = (-A, (Y = 2))
        OUTPUT = '5 ' &ERRTYPE ' Y=' Y ' V=' V
        Y =
        V = EVAL(""-A ! -(Y = '(')"")
        OUTPUT = '6 ' &ERRTYPE ' Y=' Y
END";
        Assert.AreEqual(@"1 10 Y=
2 14 Y=
3 189 Y=
4 0 Y=
5 10 Y=2 V=2
6 10 Y=", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
