using Test.TestLexer;

namespace Test.Operator;

/// <summary>
/// ~ negates its own operand's failure only, as in SPITBOL: a failure earlier in the statement (F(1) ~F(2)
/// with F failing) fails the statement, while a failure inside the operand (~F(1), ~(F(1) G(2)),
/// ~G(F(1))) is negated, and the negated value can be used.
/// </summary>
[DoNotParallelize]
[TestClass]
public class NegationScope
{
    [TestMethod]
    public void TEST_Negation_applies_to_its_own_operand()
    {
        // sbl -b answers failed, failed 2, then a b c d end
        Assert.AreEqual("failed\nfailed 2\na\nb\nc\nd\nend", SetupTests.RunWithInput(@"        DEFINE('F(X)')                 :(F_END)
F                                      :(FRETURN)
F_END
        DEFINE('G(X)')                 :(G_END)
G       G = X                          :(RETURN)
G_END
        ( F(1) ~F(2) )                 :S(Y1)F(N1)
Y1      OUTPUT = 'wrong'               :(E1)
N1      OUTPUT = 'failed'
E1      ( LT(1,2) F(1) ~F(2) )         :S(Y2)F(N2)
Y2      OUTPUT = 'wrong 2'             :(E2)
N2      OUTPUT = 'failed 2'
E2      OUTPUT = ~F(1) 'a'
        OUTPUT = ~(F(1) G(2)) 'b'
        OUTPUT = ~G(F(1)) 'c'
        OUTPUT = ~LT(2,1) 'd'
        OUTPUT = (F(1) ~F(2)) 'e'
        OUTPUT = (LT(2,1) ~F(2)) 'f'
        OUTPUT = ~G(1) 'g'
        OUTPUT = ~~F(1) 'h'
        OUTPUT = 'end'
END").Replace("\r\n", "\n"));
    }
}
