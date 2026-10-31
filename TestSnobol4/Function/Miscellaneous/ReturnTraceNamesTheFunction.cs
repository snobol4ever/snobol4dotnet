using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A function's RETURN trace runs before the call hands its value back, so the value is the call's only once the trace function is done:
/// a trace function that names the traced function (.F, as the AIS LTRACE does to print the value) names a variable, not a call that
/// returned a value (error 21), and a call the trace function makes does not take the place of the traced call's value, so .F(5) is
/// error 21 whatever the trace function called. dotnet recorded the value before the trace ran, and AIS TEST ended in error 21 at its
/// first traced return. Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ReturnTraceNamesTheFunction
{
    [TestMethod]
    public void TEST_ReturnTraceNamesTheFunction_the_trace_function_names_the_traced_function()
    {
        var program = @"        DEFINE('F(X)')                     :(FEND)
F       F = X                              :(RETURN)
FEND    DEFINE('TF()N')                    :(TFEND)
TF      N = .F
        OUTPUT = 'in trace F=' $N          :(RETURN)
TFEND   TRACE('F','RETURN',,'TF')
        &TRACE = 10
        OUTPUT = F(5)
END";
        Assert.AreEqual(@"in trace F=5
5", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }

    [TestMethod]
    public void TEST_ReturnTraceNamesTheFunction_a_call_in_the_trace_function_leaves_the_traced_value_a_value()
    {
        var program = @"        DEFINE('F(X)')                     :(FEND)
F       F = X                              :(RETURN)
FEND    DEFINE('H()')                      :(HEND)
H       H = 'h'                            :(RETURN)
HEND    DEFINE('TF()')                     :(TFEND)
TF      OUTPUT = H()                       :(RETURN)
TFEND   TRACE('F','RETURN',,'TF')
        &TRACE = 10
        SETEXIT(.HANDLER)
        &ERRLIMIT = 5
        N = .F(5)
        OUTPUT = 'after ' &ERRTYPE         :(END)
HANDLER OUTPUT = 'caught ' &ERRTYPE        :(CONTINUE)
END";
        Assert.AreEqual(@"h
caught 21
after 21", SetupTests.MaskListingHead(SetupTests.RunWithInput(program)));
    }
}
