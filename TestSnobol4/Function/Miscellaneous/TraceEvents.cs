using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// TRACE's CALL, RETURN, FUNCTION, LABEL and KEYWORD events, and &amp;FTRACE, print SPITBOL's lines on
/// standard output; only program-defined functions and &amp;ERRTYPE, &amp;FNCLEVEL and &amp;STCOUNT can be
/// traced; the name is folded. Each expectation is sbl -b's output for the same program.
/// </summary>
[DoNotParallelize]
[TestClass]
public class TraceEvents
{
    [TestMethod]
    public void TEST_Trace_Functions_call_return_freturn_nreturn()
    {
        Assert.AreEqual("****8*******  F(1)\n****15******  RETURN F = 1\n****9*******  F(-1)\n****14******  FRETURN F\n****10******  G()\n****16******  NRETURN G = .Z\n****17******  RETURN H = ''\n****12******  F(1.5)\n****15******  RETURN F = 1.5", SetupTests.RunWithInput(@"        &TRACE = 100
        DEFINE('F(X)')
        DEFINE('G()')
        DEFINE('H()')
        TRACE('F', 'FUNCTION')
        TRACE('G', 'FUNCTION')
        TRACE('H', 'RETURN')
        F(1)
        F(-1)
        G()
        H()
        F(1.5)
        :(END)
F       GT(X, 0)                 :F(FRETURN)
        F = X                    :(RETURN)
G       G = .Z                   :(NRETURN)
H                                :(RETURN)
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_Label_and_stcount()
    {
        Assert.AreEqual("****5*******  &STCOUNT = 5\n****5*******  :(L)\n****6*******  &STCOUNT = 6\n****6*******  :(M)\n****7*******  &STCOUNT = 7\n****8*******  &STCOUNT = 8", SetupTests.RunWithInput(@"        &TRACE = 100
        TRACE('L', 'LABEL')
        TRACE('M', 'LABEL')
        TRACE('STCOUNT', 'KEYWORD')
        X = 1                    :S(L)F(M)
L       Y = 2                    :F(L)S(M)
M       &ANCHOR = 1
        STOPTR('STCOUNT', 'KEYWORD')
        N = 0
LOOP    N = LT(N, 2) N + 1       :S(LOOP)
        &ANCHOR = 0
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_Fnclevel_and_errtype()
    {
        Assert.AreEqual("****7******* i &FNCLEVEL = 1\n****6*******  &FNCLEVEL = 0\n****9*******  &ERRTYPE = 14\nafter", SetupTests.RunWithInput(@"        &TRACE = 100
        TRACE('FNCLEVEL', 'KEYWORD')
        TRACE('ERRTYPE', 'KEYWORD')
        &ERRLIMIT = 5
        DEFINE('F()')  :(EF)
F       :(RETURN)
EF      F()
        D = 0
        X = 1 / D
        OUTPUT = 'after'
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_A_lower_case_name_is_folded()
    {
        Assert.AreEqual("****3*******  BAR = 2", SetupTests.RunWithInput(@"        &TRACE = 10
        TRACE('bar')
        BAR = 2
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_A_builtin_cannot_be_traced()
    {
        Assert.AreEqual("198", SetupTests.RunWithInput(@"        &ERRLIMIT = 1
        TRACE('SIZE', 'CALL')
        OUTPUT = &ERRTYPE
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_Fourth_argument_calls_the_trace_function()
    {
        // A trace function replaces the trace line, as in SPITBOL: it is called with the NAME of the traced
        // item and TRACE's third argument, &TRACE reads 0 inside it, and a name given as a string or as .H works
        Assert.AreEqual("H L1 lab 0 NAME \nH F fn 0 NAME \nH FNCLEVEL kw 0 NAME \nH FNCLEVEL kw 0 NAME \nH F fn 0 NAME 6\nH V val 0 NAME 6\n7 44", SetupTests.RunWithInput(@"        &TRACE = 50
        DEFINE('H(N,T)')               :(EH)
H       OUTPUT = 'H ' N ' ' T ' ' &TRACE ' ' DATATYPE(N) ' ' $N    :(RETURN)
EH      DEFINE('F(X)')                 :(EF)
F       F = X * 2                      :(RETURN)
EF      TRACE('L1', 'LABEL', 'lab', 'H')
        TRACE('F', 'FUNCTION', 'fn', .H)
        TRACE('V', 'VALUE', 'val', 'H')
        TRACE('FNCLEVEL', 'KEYWORD', 'kw', 'H')
                                       :(L1)
L1      V = F(3)
        STOPTR('V', 'VALUE')
        V = 7
        OUTPUT = V ' ' &TRACE
END").Replace("\r\n", "\n"));
    }
}
