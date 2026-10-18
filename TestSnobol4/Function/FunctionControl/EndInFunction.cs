using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A goto to END inside a function body ends the program, as in SPITBOL, rather than returning from the
/// call (kalah's GETMOVE does N = INPUT :F(END) at end of input).
/// </summary>
[DoNotParallelize]
[TestClass]
public class EndInFunction
{
    [TestMethod]
    public void TEST_EndInFunction_ends_the_program()
    {
        // sbl -b with input a answers got a, and nothing more
        Assert.AreEqual("got a", SetupTests.RunWithInput(@"        DEFINE('G()')                  :(G_END)
G       X = INPUT                      :F(END)
        G = X                          :(RETURN)
G_END   OUTPUT = 'got ' G()
        OUTPUT = 'got ' G()
        OUTPUT = 'after'
END", "a").Replace("\r\n", "\n"));
    }
}
