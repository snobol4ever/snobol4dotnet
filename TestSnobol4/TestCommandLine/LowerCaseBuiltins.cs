using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// Without case folding the builtins exist in upper case only: OPSYN('sqrt','SQRT') makes a new
/// function and a lower-case size is undefined, as sbl has it (spitbol_x64_tests' math_* programs).
/// </summary>
[DoNotParallelize]
[TestClass]
public class LowerCaseBuiltins
{
    [TestMethod]
    public void TEST_LowerCaseBuiltins_are_not_predefined_without_folding()
    {
        Assert.AreEqual("4. 0\napply 3", SetupTests.RunWithInput(@"-CASE 0
        &ERRLIMIT = 5
        OPSYN('sqrt','SQRT')
        OUTPUT = sqrt(16) ' ' &ERRTYPE
        OUTPUT = 'size ' size('ab')
        OUTPUT = 'apply ' APPLY('SIZE', 'abc')
END").Replace("\r\n", "\n"));
    }
}
