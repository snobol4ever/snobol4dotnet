using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// FENCE is a system function in SPITBOL (FENCE(P)), so DEFINE('FENCE(X)') is error 248, attempted redefinition of system function, as
/// sbl -bf answers (probed; snolib FENCE.tst, whose module DEFINEs FENCE, ends there). snobol4dotnet exempted FENCE from the guard for the
/// beauty demo of 2026-04, whose FENCE.inc now defines FENCE only under CSNOBOL4 (IsSnobol4()), so no graded program needs the exemption.
/// </summary>
[TestClass]
public class DefineOfFenceIs248
{
    [TestMethod]
    public void TEST_DefineOfFenceIs248()
    {
        var build = SetupTests.SetupScript("-bf", @"        &ERRLIMIT = 1
        DEFINE('FENCE(X)')
END");
        CollectionAssert.Contains(build.ErrorCodeHistory, 248);
    }
}
