using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// &amp;OUTPUT is an integer, 1 at the start; 0 suspends every output association while the variable still
/// takes its value, as in SPITBOL.
/// </summary>
[DoNotParallelize]
[TestClass]
public class OutputKeyword
{
    [TestMethod]
    public void TEST_OutputKeyword_zero_suspends_output()
    {
        // sbl -b answers 1 INTEGER, then shown hidden
        Assert.AreEqual("1 INTEGER\nshown hidden", SetupTests.RunWithInput(@"        OUTPUT = &OUTPUT ' ' DATATYPE(&OUTPUT)
        &OUTPUT = 0
        OUTPUT = 'hidden'
        X = OUTPUT
        &OUTPUT = 1
        OUTPUT = 'shown ' X
END").Replace("\r\n", "\n"));
    }
}
