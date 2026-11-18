using Test.TestLexer;

namespace Test.ProgramControl;

/// <summary>
/// A TRACE with a trace function (TRACE('F', 'CALL', 'tg', .CB)) calls that function when the trace fires, &amp;FTRACE on or off, as in
/// SPITBOL: the function runs first, then &amp;FTRACE prints its own line, and after a RETURN trace's function that line is the caller's
/// statement. snobol4dotnet's &amp;FTRACE path answered the event with its printed line alone, so the function was never called. Every
/// expected output is sbl -bf's (gate_cases_snobol4 test_gate_sno_a_trace_callback_is_ftraced_like_any_called_function__call; a probe of
/// the RETURN form).
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceFunctionUnderFtrace
{
    private const string Head = @"        &TRACE = 100
        &FTRACE = 100000
        DEFINE('CB(NAME,TAG)')  :(S)
CB      OUTPUT = 'cb ' NAME ' ' TAG  :(RETURN)
S       DEFINE('F(A)')  :(S2)
F       F = A  :(RETURN)
";

    private static string Run(string program) => SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd();

    [TestMethod]
    public void TEST_TraceFunctionUnderFtrace_a_call_trace_calls_its_function_first()
    {
        Assert.AreEqual(@"****8*******  CB(.F,'tg')
cb F tg
****4*******  RETURN CB = ''
****8*******  F(1)
****6*******  RETURN F = 1
done", Run(Head + @"S2      TRACE('F', 'CALL', 'tg', .CB)
        X = F(1)
        OUTPUT = 'done'
END"));
    }

    // A RETURN trace's function that assigns the function's variable changes what the call returns (sbl -bf: R=changed-by-tf;
    // gate_cases_snobol4 test_gate_sno_a_traced_call_keeps_its_procedure_and_its_value_across_a_collection__wb)
    [TestMethod]
    public void TEST_TraceFunctionUnderFtrace_a_return_trace_function_sets_the_value_returned()
    {
        Assert.AreEqual(@"TF F F=abcdefabcdef
R=changed-by-tf
F after=", Run(@" DEFINE('F(X,Y)A')
 DEFINE('TF(N,T)')  :(TFE)
TF OUTPUT = 'TF ' N ' F=' F
 F = IDENT(T,'RET') 'changed-by-tf'  :(RETURN)
TFE
 TRACE('F','RETURN','RET','TF')
 &TRACE = 10000
 OUTPUT = 'R=' F('abc', 'def')
 OUTPUT = 'F after=' F    :(END)
F A = X Y
 F = A A                            :(RETURN)
END"));
    }

    [TestMethod]
    public void TEST_TraceFunctionUnderFtrace_a_return_trace_calls_its_function_then_the_callers_line()
    {
        Assert.AreEqual(@"****8*******  F(1)
****6*******  CB(.F,'tg')
cb F tg
****4*******  RETURN CB = ''
****8*******  RETURN F = 1
done", Run(Head + @"S2      TRACE('F', 'RETURN', 'tg', .CB)
        X = F(1)
        OUTPUT = 'done'
END"));
    }
}
