using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A VALUE trace prints one line on standard output after each assignment the program makes to the
/// traced variable, the value in its image (strings quoted), never for a parameter's binding or its
/// restore on return. Expected results are SPITBOL's (sbl -b; rungs keyword_44).
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceValue
{
    [TestMethod]
    public void TEST_TraceValue_one_line_per_assignment_in_spitbol_form()
    {
        Assert.AreEqual("****3*******  N = 'first'\n****4*******  N = 5\n****5*******  N = 'BC'", SetupTests.RunWithInput(@"        &TRACE = 10
        TRACE('N', 'VALUE')
        N = 'first'
        N = 2 + 3
        'ABCD' 'BC' . N
end").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_TraceValue_not_for_parameter_binding_and_with_the_function_level()
    {
        Assert.AreEqual("****4*******  A = 'outer'\n****6******* i L = 'inx'", SetupTests.RunWithInput(@"        &TRACE = 10
        TRACE('A', 'VALUE')
        TRACE('L', 'VALUE')
        A = 'outer'
        DEFINE('F(A)L')  :(EF)
F       L = A 'x'        :(RETURN)
EF      F('in')
end").Replace("\r\n", "\n"));
    }
}
