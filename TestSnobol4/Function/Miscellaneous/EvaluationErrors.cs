using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// An error inside the evaluation of an expression, by EVAL (of a string or an expression) or by a deferred expression in a pattern, does not
/// end the run, whatever &ERRLIMIT is: the statement it occurs in fails and the run goes on, &ERRTYPE tells the error, and &ERRLIMIT counts
/// down to 0 and no further. An error in a function called directly is still fatal at 0. The expected text is what sbl -b printed for the
/// same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class EvaluationErrors
{
    [TestMethod]
    public void TEST_EvaluationErrors_eval_of_a_string_whose_function_fails_goes_on()
    {
        var program = @"        DEFINE('G()')   :(GO)
G       OUTPUT = 'in G'
        X = FOO(3)
        OUTPUT = 'after error in G'  :(RETURN)
GO      Y = EVAL('G()')  :F(OOPS)
        OUTPUT = 'y=[' Y '] err ' &ERRTYPE  :(END)
OOPS    OUTPUT = 'failed ' &ERRTYPE
END";
        Assert.AreEqual(@"in G
after error in G
y=[] err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EvaluationErrors_deferred_expression_in_a_pattern_goes_on()
    {
        var program = @"        DEFINE('G()')   :(GO)
G       OUTPUT = 'in G'
        X = FOO(3)
        OUTPUT = 'after error in G'  :(RETURN)
GO      'abc' *G()  :F(OOPS)
        OUTPUT = 'matched err ' &ERRTYPE  :(END)
OOPS    OUTPUT = 'failed ' &ERRTYPE
END";
        Assert.AreEqual(@"in G
after error in G
matched err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EvaluationErrors_deferred_expression_in_len_goes_on()
    {
        var program = @"        DEFINE('G()')   :(GO)
G       OUTPUT = 'in G'
        X = FOO(3)
        OUTPUT = 'after error in G'  :(RETURN)
GO      'abc' LEN(*G())
        OUTPUT = 'done err ' &ERRTYPE  :(END)
END";
        Assert.AreEqual(@"in G
after error in G
done err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EvaluationErrors_eval_of_an_expression_object_goes_on()
    {
        var program = @"        DEFINE('G()')   :(GO)
G       OUTPUT = 'in G'
        X = FOO(3)
        OUTPUT = 'after error in G'  :(RETURN)
GO      E = *G()
        Y = EVAL(E)  :F(OOPS)
        OUTPUT = 'y=[' Y '] err ' &ERRTYPE  :(END)
OOPS    OUTPUT = 'failed ' &ERRTYPE
END";
        Assert.AreEqual(@"in G
after error in G
y=[] err 22", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EvaluationErrors_division_by_a_null_name_in_eval_fails()
    {
        var program = @"        X = EVAL('5 / N0')  :F(OOPS)
        OUTPUT = 'x=' X  :(END)
OOPS    OUTPUT = 'failed ' &ERRTYPE ' ' &ERRTEXT
END";
        Assert.AreEqual(@"failed 14 division caused integer overflow", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_EvaluationErrors_errlimit_counts_down_to_zero_and_no_further()
    {
        var program = @"        DEFINE('G()')   :(GO)
G       X = FOO(3)
        X = FOO(4)
        X = FOO(5)
        OUTPUT = 'G errlimit ' &ERRLIMIT :(RETURN)
GO      &ERRLIMIT = 2
        Y = EVAL('G()')  :F(OOPS)
        OUTPUT = 'y=[' Y '] err ' &ERRTYPE ' errlimit ' &ERRLIMIT :(END)
OOPS    OUTPUT = 'failed ' &ERRTYPE
END";
        Assert.AreEqual(@"G errlimit 0
y=[] err 22 errlimit 0", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
