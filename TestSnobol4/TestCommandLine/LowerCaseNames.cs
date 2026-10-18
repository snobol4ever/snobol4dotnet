using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// Without case folding (-f, or -CASE 0) the predefined names are upper case only: a lower-case output, input or arb is an
/// ordinary variable, as sbl has it (Budne's case1).
/// </summary>
[DoNotParallelize]
[TestClass]
public class LowerCaseNames
{
    [TestMethod]
    public void TEST_LowerCaseNames_are_ordinary_variables_without_folding()
    {
        var output = SetupTests.RunWithInput(@"-CASE 0
        output = 'lower'
        OUTPUT = DATATYPE(arb) ' ' DATATYPE(ARB) ' [' input '] ' output
END", "line");
        Assert.AreEqual("STRING PATTERN [] lower", output);
    }
}
