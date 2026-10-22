using Test.TestLexer;

namespace Test.FunctionControl;

/// <summary>
/// The arguments of a call are evaluated before the call is found to be to a function that is not defined, so INPUT given as one is read
/// (and at the end of input the statement fails, with no error 22), as SPITBOL evaluates them. The expected text is what sbl -b printed for
/// the same program and input.
/// </summary>
[DoNotParallelize]
[TestClass]
public class UndefinedFunctionArguments
{
    private const string Program = @"        &ERRLIMIT = 5
        X = FOO(INPUT)  :S(SK)F(FL)
SK      OUTPUT = 'S err ' &ERRTYPE  :(NEXT)
FL      OUTPUT = 'F err ' &ERRTYPE
NEXT    Y = INPUT
        OUTPUT = 'Y=' Y
        Z = FOO(INPUT, INPUT)  :S(SK2)F(FL2)
SK2     OUTPUT = 'S2 err ' &ERRTYPE  :(NEXT2)
FL2     OUTPUT = 'F2 err ' &ERRTYPE
NEXT2   W = INPUT
        OUTPUT = 'W=' W
END";

    [TestMethod]
    public void TEST_UndefinedFunctionArguments_input_arguments_are_read_before_error_22()
    {
        Assert.AreEqual("F err 22\nY=l2\nF2 err 22\nW=l5",
            SetupTests.RunWithInput(Program, "l1\nl2\nl3\nl4\nl5\nl6").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_UndefinedFunctionArguments_input_read_to_its_end_in_an_argument_fails_the_statement()
    {
        // one line is left for the first call; the second call finds none: the statement fails with no error
        var program = @"        &ERRLIMIT = 5
        X = INPUT
        Y = FOO(INPUT)  :S(SK)F(FL)
SK      OUTPUT = 'S err ' &ERRTYPE  :(END)
FL      OUTPUT = 'F err ' &ERRTYPE
END";
        Assert.AreEqual("F err 0", SetupTests.RunWithInput(program, "l1").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_UndefinedFunctionArguments_the_value_of_an_assignment_to_input_is_the_value_assigned()
    {
        // sbl -b: (INPUT = 'zz') has the value zz and reads nothing; the next read of INPUT gets the first line
        var program = @"        X = (INPUT = 'zz')
        Y = 'a' (INPUT = 'zz')
        Z = SIZE(INPUT = 'qqq')
        W = INPUT
        OUTPUT = X '|' Y '|' Z '|' W
END";
        Assert.AreEqual("zz|azz|3|l1", SetupTests.RunWithInput(program, "l1\nl2\nl3").Replace("\r\n", "\n"));
    }
}
