using Test.TestLexer;

namespace Test.ArraysTables;

/// <summary>
/// After a failure in a statement or an expression the element of a subscript is not taken, but its subscripts and the collection still
/// leave the stack, one failure value in their place, as SPITBOL never evaluates them: (NE(1, 1) U['v'], 'alt1') is alt1 and
/// ~(NE(1, 1) U['v']) succeeds with U no table. dotnet left the collection and the subscripts on the stack under the failure value, and
/// the choice or the negation after it read them for its own operand (null, a failure). Found by the infinite_snobol4 generator under -b
/// ((NE(q, e) ? $n u['v'], 'r[8v]'), seed 413896). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ASubscriptAfterAFailure
{
    [TestMethod]
    public void TEST_ASubscriptAfterAFailure_leaves_its_operands_off_the_stack()
    {
        var program = @"        &ERRLIMIT = 10
        U = ''
        T = TABLE()
        T<'v'> = 'tv'
        R = (NE(1, 1) U['v'], 'alt1')                   :S(Y1)
        OUTPUT = '1 F ' &ERRTYPE                        :(E1)
Y1      OUTPUT = '1 S [' R ']'
E1      R = ~(NE(1, 1) U['v'])                          :S(Y2)
        OUTPUT = '2 F ' &ERRTYPE                        :(E2)
Y2      OUTPUT = '2 S [' R ']'
E2      R = EVAL(""(NE(1, 1) ? U['v'], 'alt3')"")         :S(Y3)
        OUTPUT = '3 F ' &ERRTYPE                        :(E3)
Y3      OUTPUT = '3 S [' R ']'
E3      R = (NE(1, 1) T<'v'> T<'v'>, 'alt4') 'z'        :S(Y4)
        OUTPUT = '4 F ' &ERRTYPE                        :(E4)
Y4      OUTPUT = '4 S [' R ']'
E4      R = ~(EQ(1, 2) T<'v', 1>) 'w'                   :S(Y5)
        OUTPUT = '5 F ' &ERRTYPE                        :(E5)
Y5      OUTPUT = '5 S [' R '] ' &ERRLIMIT
E5
END";
        Assert.AreEqual(@"1 S [alt1]
2 S []
3 S [alt3]
4 S [alt4z]
5 S [w] 10", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
