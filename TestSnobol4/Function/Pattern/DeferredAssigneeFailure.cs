using Test.TestLexer;

namespace Test.Pattern;

/// <summary>
/// A conditional assignment whose deferred target ends in an error (22, a function not defined) or in a failure (its function returns by
/// FRETURN) makes the whole statement fail when &ERRLIMIT lets the run go on, so its :F is taken, and the assignments after it are not made.
/// The expected text is what sbl -b printed for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class DeferredAssigneeFailure
{
    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_target_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' . *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_target_of_an_immediate_assignment_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' $ *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_an_error_in_the_second_target_fails_the_statement()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('b' . X . *FOO(*1))  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_a_replacement_statement_fails_too()
    {
        var program = @"        &ERRLIMIT = 5
        S = 'abc'
        S ('b' . *FOO()) = 'z'  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_the_first_assignment_stops_the_commits()
    {
        var program = @"        &ERRLIMIT = 5
        'abc' ('a' . *FOO() 'b' . X)  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE ' X=[' X ']'  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE ' X=[' X ']'
END";
        Assert.AreEqual(@"F err 22 X=[]", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_DeferredAssigneeFailure_a_target_function_that_fails_fails_the_statement()
    {
        var program = @"        DEFINE('FOO()')   :(GO)
FOO     :(FRETURN)
GO      &ERRLIMIT = 5
        'abc' ('b' . *FOO())  :S(YES)F(NO)
YES     OUTPUT = 'S err ' &ERRTYPE  :(END)
NO      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual(@"F err 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
