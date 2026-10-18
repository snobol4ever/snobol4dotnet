using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// CODE's statements are numbered on from the program's last statement, each block ending in an implicit
/// statement that takes a number of its own, and EVAL takes none, as SPITBOL numbers them.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CodeStatementNumbers
{
    [TestMethod]
    public void TEST_CodeStatementNumbers_follow_the_program()
    {
        // sbl -b answers 13 14 3 / 16 6 16 / E=7 / 18 for this twelve-statement program
        Assert.AreEqual("13 14 3\n16 6 16\nE=7\n18", SetupTests.RunWithInput(@"        C1 = CODE(' A = &STNO ; B = &STNO :(R1)')
        :<C1>
R1      OUTPUT = A ' ' B ' ' &STNO
        C2 = CODE(' D = &STNO :(R2)')
        :<C2>
R2      OUTPUT = D ' ' &STNO ' ' &LASTNO
        E = EVAL('&STNO')
        OUTPUT = 'E=' E
        C3 = CODE(' F = &STNO :(R3)')
        :<C3>
R3      OUTPUT = F
END").Replace("\r\n", "\n"));
    }
}
