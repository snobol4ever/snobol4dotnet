using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// A function whose body is the program's first statement runs that statement when it is called, though END names an entry label the
/// run started at, as in SPITBOL (sbl -bf prints HELLO, WORLD; snobol4hs_examples define). snobol4dotnet re-entered its executor for the
/// call at instruction 0, read that as the program's start, and jumped to the entry label again, recursing to error 246.
/// </summary>
[DoNotParallelize]
[TestClass]
public class AFunctionAtTheFirstStatementUnderAnEntryLabel
{
    [TestMethod]
    public void TEST_AFunctionAtTheFirstStatementUnderAnEntryLabel_runs_its_body()
    {
        Assert.AreEqual("HELLO, WORLD", SetupTests.RunWithInputDirectives("-bf", @"PRINT.INIT  OUTPUT = MSG                        :(RETURN)
INIT        DEFINE('PRINT(MSG)','PRINT.INIT')
            PRINT('HELLO, WORLD')
END INIT").Replace("\r\n", "\n").TrimEnd());
    }
}
