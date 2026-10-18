using Test.TestLexer;

namespace Test.Memory;

/// <summary>
/// CLEAR() nulls values but keeps input and output associations, as SPITBOL does (s_clr stores the null
/// at the end of the trap chain): OUTPUT still writes and INPUT still reads after it.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ClearAssociations
{
    [TestMethod]
    public void TEST_Clear_keeps_input_and_output_associations()
    {
        // sbl -b with input one/two answers [][world] then [][two][]
        Assert.AreEqual("[][world]\n[][two][]", SetupTests.RunWithInput(@"        X = 'hello'
        Y = 'world'
        CLEAR('Y')
        OUTPUT = '[' X '][' Y ']'
        LINE = INPUT
        CLEAR()
        OUTPUT = '[' LINE '][' INPUT '][' Y ']'
END", "one\ntwo\n").Replace("\r\n", "\n"));
    }
}
