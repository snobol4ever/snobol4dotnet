using Test.TestLexer;

namespace Test.TestGoto;

/// <summary>
/// A function called in a goto field is called by name: one that returns a value there by RETURN is error 21, function called by name
/// returned a value, at the statement of the goto, as in SPITBOL (sbl -bf; NRETURN of a name or a string goes to that label, FRETURN is
/// error 20, as both engines already answered). snobol4dotnet read the label from the returned value's symbol, the function's own name,
/// and went to the function's entry at level zero, error 242 (gate_cases_snobol4 test_gate_sno_goto_field_call_is_by_name__val).
/// </summary>
[DoNotParallelize]
[TestClass]
public class GotoFieldCallByName
{
    [TestMethod]
    public void TEST_GotoFieldCallByName_a_value_returned_is_error_21()
    {
        var output = SetupTests.RunWithInputDirectives("-bf", @"        DEFINE('GOV()')                 :(MAIN)
GOV     GOV  =  'L3'                    :(RETURN)
MAIN    OUTPUT  =  'go'                  :(GOV())
        OUTPUT  =  'NOPE'
L3      OUTPUT  =  'landed'
END").Replace("\r\n", "\n");
        StringAssert.Contains(output, "Test.sno(3) : ERROR 021 -- function called by name returned a value");
        Assert.IsFalse(output.Contains("landed") || output.Contains("NOPE"), output);
    }
}
