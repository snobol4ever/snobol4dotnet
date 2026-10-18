using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// At end of input an assignment from INPUT fails and assigns nothing, as in SPITBOL: OUTPUT = INPUT
/// writes no empty line and X = INPUT leaves X as it was.
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputEnd
{
    [TestMethod]
    public void TEST_InputEnd_the_failed_read_assigns_nothing()
    {
        // sbl -b with input x answers x then X=keep
        Assert.AreEqual("x\nX=keep", SetupTests.RunWithInput(@"        X = 'keep'
R       OUTPUT = INPUT   :F(D)
        :(R)
D       X = INPUT        :S(END)
        OUTPUT = 'X=' X
END", "x").Replace("\r\n", "\n"));
    }
}
