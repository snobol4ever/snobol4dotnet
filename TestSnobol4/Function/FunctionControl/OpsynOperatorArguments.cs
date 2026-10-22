using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// An operator made a synonym of a function is given as many arguments as the function declares, as SPITBOL gives them: the ones the
/// operator lacks are null and the ones it has too many are dropped. OPSYN('#', 'LPAD', 2) makes ab # 5 the call LPAD('ab', 5, null), where
/// the console died with an unhandled ArgumentOutOfRangeException. Every expectation is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class OpsynOperatorArguments
{
    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_binary_operator_that_is_a_function_of_three()
    {
        var program = @"        OPSYN('#','LPAD',2)
        X = 'ab' # 5
        OUTPUT = '[' X ']'
END";
        Assert.AreEqual(@"[   ab]", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_another_binary_operator_that_is_a_function_of_three()
    {
        var program = @"        OPSYN('&','RPAD',2)
        X = 'ab' & 5
        OUTPUT = '[' X ']'
END";
        Assert.AreEqual(@"[ab   ]", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_binary_operator_that_is_a_function_of_one()
    {
        var program = @"        OPSYN('@','SIZE',2)
        X = 'abc' @ 5
        OUTPUT = X
END";
        Assert.AreEqual(@"3", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_unary_operator_that_is_a_function_of_two()
    {
        var program = @"        OPSYN('#','DUPL',1)
        X = #'ab'
        OUTPUT = '[' X ']'
END";
        Assert.AreEqual(@"[]", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_binary_operator_that_is_a_program_defined_function_of_three()
    {
        var program = @"        DEFINE('F(A,B,C)')  :(START)
F       F = 'a=' A ' b=' B ' c=' C  :(RETURN)
START   OPSYN('#','F',2)
        OUTPUT = 1 # 2
END";
        Assert.AreEqual(@"a=1 b=2 c=", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_unary_operator_that_is_a_program_defined_function_of_two()
    {
        var program = @"        DEFINE('F(A,B)')  :(START)
F       F = 'a=' A ' b=' B  :(RETURN)
START   OPSYN('%','F',1)
        OUTPUT = %7
END";
        Assert.AreEqual(@"a=7 b=", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_OpsynOperatorArguments_a_binary_operator_that_is_a_function_of_two_is_unchanged()
    {
        var program = @"        OPSYN('#','DUPL',2)
        OUTPUT = 'ab' # 3
END";
        Assert.AreEqual(@"ababab", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
