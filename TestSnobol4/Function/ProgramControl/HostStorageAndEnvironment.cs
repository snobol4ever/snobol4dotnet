using Snobol4.Common;
using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// HOST(-1, 2), the heap's base, is positive and no greater than HOST(-1, 3), its top, and HOST(4, '') is error 254, erroneous argument
/// for host, as sbl -bf answers (gate_cases_snobol4 test_gate_sno_host_minus_one_answers_the_memory_statistics__w and
/// test_gate_sno_host_4_fails_when_the_variable_is_not_in_the_environment__e). snobol4dotnet answered the base 0 and HOST(4, '') a
/// failure. A .NET process has no one address for its heap, so the base is the heap's first byte, 1.
/// </summary>
[DoNotParallelize]
[TestClass]
public class HostStorageAndEnvironment
{
    [TestMethod]
    public void TEST_HostStorageAndEnvironment_the_heap_base_is_positive_and_below_its_top()
    {
        var output = SetupTests.RunWithInputDirectives("-bf", @"        OUTPUT = GT(HOST(-1,2),0) 'base positive'
        OUTPUT = GE(HOST(-1,3),HOST(-1,2)) 'top not below base'
END").Replace("\r\n", "\n").TrimEnd();
        Assert.AreEqual("base positive\ntop not below base", output);
    }

    [TestMethod]
    public void TEST_HostStorageAndEnvironment_host_4_of_the_null_string_is_error_254()
    {
        var build = SetupTests.SetupScript("-bf", @"        &ERRLIMIT = 1
        X = HOST(4,'')
END");
        CollectionAssert.Contains(build.ErrorCodeHistory, 254);
    }
}
