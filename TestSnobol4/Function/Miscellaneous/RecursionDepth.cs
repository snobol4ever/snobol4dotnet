using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// Recursion as SPITBOL limits it: a program may call as deep as its stack allows (-s, 4m by default), 58,209 levels of a function
/// with one parameter, fewer for a function with more parameters and locals and for a call made with operands waiting in the
/// expression (each level takes 64 bytes and 8 for each parameter, local and waiting operand, from a stack that starts 3,216 bytes
/// down), and a call past it is error 246, stack overflow, where the console died on a .NET stack overflow below five thousand
/// levels. The numbers are what sbl -b printed for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class RecursionDepth
{
    private static string Runaway(string definition, string body) =>
        $"        DEFINE('{definition}')\n        OUTPUT = F(1)\n        :(END)\nF       {body}  :(RETURN)\nEND";

    [TestMethod]
    public void TEST_RecursionDepth_a_program_may_recurse_fifty_thousand_levels()
    {
        var program = @"        DEFINE('F(N)')
        OUTPUT = F(50000)
        :(END)
F       F = EQ(N,0) 0  :S(RETURN)
        F = F(N - 1)  :(RETURN)
END";
        Assert.AreEqual("0", SetupTests.RunWithInput(program));
    }

    [TestMethod]
    public void TEST_RecursionDepth_runaway_recursion_is_error_246_at_the_depth_the_stack_allows()
    {
        var output = SetupTests.MaskReport(SetupTests.RunWithInput(Runaway("F(N)", "F = F(N + 1)")));
        StringAssert.Contains(output, "Test.sno(4) : ERROR 246 -- stack overflow");
        StringAssert.Contains(output, "in statement         4\n");
        StringAssert.Contains(output, "stmts executed       58211\n");
    }

    [TestMethod]
    public void TEST_RecursionDepth_parameters_locals_and_waiting_operands_make_the_frames_bigger()
    {
        StringAssert.Contains(SetupTests.RunWithInput(Runaway("F(N)A,B", "F = F(N + 1)")), "stmts executed       47628\n");
        StringAssert.Contains(SetupTests.RunWithInput(Runaway("F(N)", "F = 'a' F(N + 1)")), "stmts executed       52390\n");
        StringAssert.Contains(SetupTests.RunWithInput(Runaway("F(N)", "F = 1 + (2 + F(N + 1))")), "stmts executed       47628\n");
    }

    [TestMethod]
    public void TEST_RecursionDepth_the_stack_switch_sets_the_depth()
    {
        StringAssert.Contains(SetupTests.RunWithInputDirectives("-b -s64k", Runaway("F(N)", "F = F(N + 1)")), "stmts executed       867\n");
        StringAssert.Contains(SetupTests.RunWithInputDirectives("-b -s1m", Runaway("F(N)", "F = F(N + 1)")), "stmts executed       14520\n");
    }
}
