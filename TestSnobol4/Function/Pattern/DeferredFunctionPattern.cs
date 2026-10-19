using Snobol4.Common;
using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A match whose pattern is a deferred expression calling a program-defined function runs that function at
/// match time, as in SPITBOL (sbl -bf; Gimpel's ONCE builds its pattern so).
/// </summary>
[TestClass]
public class DeferredFunctionPattern
{
    [TestMethod]
    public void TEST_DeferredFunctionPattern_once()
    {
        // sbl -bf: S P with P = *ONCE() matches the first time and fails after, since ONCE then answers FAIL;
        // snobol4dotnet crashed (the function's calls cleared the match's argument list)
        var build = SetupTests.SetupScript("-bf", """
                 DEFINE('ONCE()')               :(ONCE_END)
         ONCE    ONCE = DIFFER(DONE) FAIL       :S(RETURN)
                 DONE = 1
                 ONCE = ''                      :(RETURN)
         ONCE_END
                 P = *ONCE()
                 'xyz' P                        :F(T2)
                 R = R 'a'
         T2      'xyz' P                        :F(T3)
                 R = R 'b'
         T3      'xyz' P                        :F(T4)
                 R = R 'c'
         T4
         END
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("a", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
    }
}
