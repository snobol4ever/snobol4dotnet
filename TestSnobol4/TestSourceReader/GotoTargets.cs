using Test.TestLexer;

namespace Test.TestSourceReader;

/// <summary>
/// The target of a goto in parens is a name SPITBOL generates by name, as the target of an assignment is: a constant, an arithmetic
/// result, a concatenation or a name operator's value there is error 212, "value used where name is required", found when the
/// statement is compiled. A target in angle brackets is a value (a code) and is judged when it is taken. Every expectation is
/// what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class GotoTargets
{
    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_string()
    {
        var program = @"        X = A :('L')
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :('L')

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_number()
    {
        var program = @"        X = A :(1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(1)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_real()
    {
        var program = @"        X = A :(1.5)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(1.5)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_concatenation()
    {
        var program = @"        X = A :(L M)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(L M)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_name_operators_value()
    {
        var program = @"        X = A :(.L)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(.L)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_an_arithmetic_result()
    {
        var program = @"        X = A :(A + B)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(A + B)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_a_concatenation_of_names()
    {
        var program = @"        X = A :(A B)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :(A B)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_success_goto_to_a_string()
    {
        var program = @"        X = A :S('L')
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S('L')

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_success_goto_to_a_number()
    {
        var program = @"        X = A :S(1)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(1)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_success_goto_to_an_arithmetic_result()
    {
        var program = @"        X = A :S(A + B)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(A + B)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_failure_goto_to_a_string()
    {
        var program = @"        X = A :F('L')
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :F('L')

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_failure_goto_to_an_arithmetic_result()
    {
        var program = @"        X = A :F(1 + 2)
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :F(1 + 2)

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_success_goto_to_nothing()
    {
        var program = @"        X = A :S()
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S()

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_goto_to_nothing()
    {
        var program = @"        X = A :()
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :()

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_a_second_goto_is_judged_too()
    {
        var program = @"        X = A :S(L)F('M')
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               X = A :S(L)F('M')

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_the_body_is_judged_first()
    {
        var program = @"        'A' = 1 :('L')
        OUTPUT = 'x'
END";
        Assert.AreEqual(@"


<identity>
<clock>



                                                                                                                page 1

1               'A' = 1 :('L')

Test.sno(1) : ERROR 212 -- syntax error: value used where name is required", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_GotoTargets_a_bracketed_assignment_is_212()
    {
        // sbl -b: a bracket just inside a goto's own groups what it holds, so :((X = 1)) is a goto to the value an assignment gives, 212,
        // in each goto (:S, :F, and inside another bracket); :($(X = 1)), :((L)) and a bracket in a direct goto compile. The console took
        // that bracket for the call's, with no function before it, and compiled them all (the master's ladder__rung05 entry
        // goto_a_value_operand_is_error_212..., read against a fresh sbl run)
        var build = SetupTests.SetupScript("-b -n", @"        Y = 1 :((X = 1))
        Y = 1 :((X = 'L'))
        Y = 1 :(((X = 1)))
        Y = 1 :S((X = 1))
        Y = 1 :F((X = 1))
        Y = 1 :($(X = 1))
        Y = 1 :((L))
        Y = 1 :S((L))F((L))
        Y = 1 :<(C)>
L
END");
        CollectionAssert.AreEqual(new[] { 212, 212, 212, 212, 212 }, build.ErrorCodeHistory.ToArray());
    }
}
