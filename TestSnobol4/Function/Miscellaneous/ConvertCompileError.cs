using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// CONVERT of a text that does not compile to EXPRESSION or CODE is the compile error of EVAL or CODE, as in SPITBOL: &amp;ERRLIMIT counts it
/// down and an armed SETEXIT handler takes it (CONVERT('(', 'EXPRESSION') is 226 in the handler), where it failed quietly; and the null
/// string converts to the expression whose value is the null string, where it failed. Found by the infinite_snobol4 generator
/// (... ? ITEM($a[0], 9) (+CONVERT('[]', 'EXPRESSION') $m ? '4p'): ERROR 222 in sbl, FAIL here). Every expected line is what sbl -b printed.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ConvertCompileError
{
    [TestMethod]
    public void TEST_ConvertCompileError_is_the_error_of_eval_and_code()
    {
        var program = @"        &ERRLIMIT = 10
        V = CONVERT('[]', 'EXPRESSION')             :S(Y1)
        OUTPUT = '1 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E1)
Y1      OUTPUT = '1 S ' DATATYPE(V)
E1      V = EVAL('[]')                              :S(Y2)
        OUTPUT = '2 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E2)
Y2      OUTPUT = '2 S ' DATATYPE(V)
E2      V = CONVERT('', 'EXPRESSION')               :S(Y3)
        OUTPUT = '3 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E3)
Y3      OUTPUT = '3 S ' DATATYPE(V) ' [' EVAL(V) ']'
E3      V = CONVERT('1 +', 'EXPRESSION')            :S(Y4)
        OUTPUT = '4 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E4)
Y4      OUTPUT = '4 S ' DATATYPE(V)
E4      SETEXIT('TRAP')
        V = CONVERT('(', 'EXPRESSION')              :S(Y5)
        OUTPUT = '5 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E5)
Y5      OUTPUT = '5 S ' DATATYPE(V)                 :(E5)
TRAP    OUTPUT = '5 TRAP t=' &ERRTYPE ' lim=' &ERRLIMIT :(E5)
E5      V = CODE(' X = [')                          :S(Y6)
        OUTPUT = '6 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(E6)
Y6      OUTPUT = '6 S'
E6      V = CONVERT(' X = [', 'CODE')               :S(Y7)
        OUTPUT = '7 F t=' &ERRTYPE ' lim=' &ERRLIMIT :(END)
Y7      OUTPUT = '7 S'
END";
        Assert.AreEqual(@"1 F t=222 lim=9
2 F t=222 lim=8
3 S EXPRESSION []
4 F t=221 lim=7
5 TRAP t=226 lim=6
6 F t=222 lim=5
7 F t=222 lim=4", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    [TestMethod]
    public void TEST_ConvertCompileError_a_goto_or_a_semicolon_fails_quietly()
    {
        // sbl -b: a colon or a semicolon outside every bracket and quote ends the expression CONVERT takes, and the conversion just
        // fails, leaving &ERRTYPE and &ERRLIMIT as they were, whatever comes before it ('4 :(L)', '4 + ;', '0:35', the prototype of an
        // array); inside parentheses it is the compile error 226. The console compiled the text inside parentheses, error 226 counted
        // each time (found by measuring the CODE road of DESIGN-PLAN random-codegen-scope step 11, CONVERT(PROTOTYPE(a), 'EXPRESSION'))
        var program = @"        &ERRLIMIT = 10
        V = CONVERT('4 +', 'EXPRESSION')            :S(Y1)
        OUTPUT = '1 F t=' &ERRTYPE ' lim=' &ERRLIMIT
Y1      V = CONVERT('4 :(L)', 'EXPRESSION')         :S(Y2)
        OUTPUT = '2 F t=' &ERRTYPE ' lim=' &ERRLIMIT  :(E2)
Y2      OUTPUT = '2 S'
E2      V = CONVERT('4 + ;', 'EXPRESSION')          :S(Y3)
        OUTPUT = '3 F t=' &ERRTYPE ' lim=' &ERRLIMIT  :(E3)
Y3      OUTPUT = '3 S'
E3      V = CONVERT('(4 :(L))', 'EXPRESSION')       :S(Y4)
        OUTPUT = '4 F t=' &ERRTYPE ' lim=' &ERRLIMIT  :(E4)
Y4      OUTPUT = '4 S'
E4      V = CONVERT('0:35', 'EXPRESSION')           :S(Y5)
        OUTPUT = '5 F t=' &ERRTYPE ' lim=' &ERRLIMIT  :(END)
Y5      OUTPUT = '5 S'
END";
        Assert.AreEqual(@"1 F t=221 lim=9
2 F t=221 lim=9
3 F t=221 lim=9
4 F t=226 lim=8
5 F t=226 lim=8", SetupTests.RunWithInput(program).Replace("\r\n", "\n").TrimEnd('\n'));
    }

    // &ERRTYPE and &ERRLIMIT after one call of `function` on `text` alone, from &ERRLIMIT 10, or S for success
    private static string OneCall(string function, string text) => SetupTests.RunWithInput($@"        &ERRLIMIT = 10
        V = {function}('{text}'{(function == "CONVERT" ? ", 'EXPRESSION'" : "")})     :S(Y1)
        OUTPUT = 'F t=' &ERRTYPE ' lim=' &ERRLIMIT   :(END)
Y1      OUTPUT = 'S'
END").Replace("\r\n", "\n").TrimEnd('\n');

    [TestMethod]
    public void TEST_ConvertCompileError_a_syntax_error_before_a_mark_is_recorded()
    {
        // sbl -bf: before a colon or a semicolon that is not the text's last character, SPITBOL scans the expression for a syntax error
        // and goes no further: CONVERT records it ('4 + ; ', '4 + : x', '4 + ;5' are 221 and count &ERRLIMIT) and otherwise fails quietly
        // ('4 + ;', '1 / 0 ; 5', 'X Y = ; 5', '; 4'); EVAL gives it, and 103 for anything else, though checking the expression would find
        // an error ('1 / 0 ; 5' is 103, not 14; 'LEN(-1) ; 5' not 121; 'X Y = ; 5' not 212). The console's CONVERT failed quietly at every
        // mark and its EVAL gave the checker's error (the master's code_eval_eval_convert_replace_branch_1, CONVERT('4 + ; '))
        Assert.AreEqual("F t=221 lim=9", OneCall("CONVERT", "4 + ; "));
        Assert.AreEqual("F t=221 lim=9", OneCall("CONVERT", "4 + : x"));
        Assert.AreEqual("F t=221 lim=9", OneCall("CONVERT", "4 + ;5"));
        Assert.AreEqual("F t=0 lim=10", OneCall("CONVERT", "4 + ;"));
        Assert.AreEqual("F t=0 lim=10", OneCall("CONVERT", "1 / 0 ; 5"));
        Assert.AreEqual("F t=0 lim=10", OneCall("CONVERT", "X Y = ; 5"));
        Assert.AreEqual("F t=0 lim=10", OneCall("CONVERT", "; 4"));
        Assert.AreEqual("F t=221 lim=9", OneCall("EVAL", "4 + ; "));
        Assert.AreEqual("F t=103 lim=9", OneCall("EVAL", "1 / 0 ; 5"));
        Assert.AreEqual("F t=103 lim=9", OneCall("EVAL", "LEN(-1) ; 5"));
        Assert.AreEqual("F t=103 lim=9", OneCall("EVAL", "X Y = ; 5"));
        Assert.AreEqual("F t=103 lim=9", OneCall("EVAL", "; 4"));
    }

    [TestMethod]
    public void TEST_ConvertCompileError_an_open_bracket_at_the_end_is_229()
    {
        // sbl -bf: a text whose innermost bracket still open at its end is [ or < is 229, "missing right array bracket", in EVAL and
        // CONVERT as in CODE ('a[', 'a<', 'a[1, '); an open parenthesis is 226 ('f('), and a closer that meets the wrong opener is its own
        // error ('f(a[1)' 224, 'a[(1]' 225). The console's EVAL and CONVERT said 224 for an open bracket: the parenthesis closing the text
        // they compile met it first (found by the infinite_snobol4 campaign at scope 12, seed0 20261109, size 1000, batch 7 of kind=stmt,
        // line 582, CONVERT('a[', 'EXPRESSION'))
        Assert.AreEqual("F t=229 lim=9", OneCall("EVAL", "a["));
        Assert.AreEqual("F t=229 lim=9", OneCall("CONVERT", "a["));
        Assert.AreEqual("F t=229 lim=9", OneCall("EVAL", "a<"));
        Assert.AreEqual("F t=229 lim=9", OneCall("CONVERT", "a[1, "));
        Assert.AreEqual("F t=226 lim=9", OneCall("EVAL", "f("));
        Assert.AreEqual("F t=224 lim=9", OneCall("EVAL", "f(a[1)"));
        Assert.AreEqual("F t=225 lim=9", OneCall("CONVERT", "a[(1]"));
    }
}
