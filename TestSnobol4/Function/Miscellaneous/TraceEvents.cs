using Snobol4.Common;
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

    [TestMethod]
    public void TEST_Trace_Value_fires_for_a_cursor_assignment()
    {
        // sbl -b: @C in a match is an assignment to C and its VALUE trace prints
        Assert.AreEqual("****3*******  C = 4\n4", SetupTests.RunWithInput(@"        &TRACE = 10
        TRACE('C')
        'WORLD' 'RL' @C
        OUTPUT = C
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Trace_Access_check_takes_an_empty_name()
    {
        // Growing the variable slot array reads every slot through the identifier table, an empty-named one
        // included, and the access-trace check indexed name[0]; snoflake's eliza-modernized crashed there
        // (IndexOutOfRangeException) where sbl -bf runs to the end
        var build = SetupTests.SetupScript("-b", @"
        X = 1
end");
        build.Execute!.TraceIdentifierAccess("");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
    }

    [TestMethod]
    public void TEST_Trace_Value_fires_for_a_replacement()
    {
        // sbl -b: B P = replaces the matched part of B, an assignment to B, and its VALUE trace prints
        Assert.AreEqual("****3*******  B = 'yes!!'\n****5*******  B = '!!'\n!!", SetupTests.RunWithInput(@"        &TRACE = 10
        TRACE('B')
        B = 'yes!!'
        P = 'yes'
        B P =
        OUTPUT = B
END").Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_Stoptr_returns_null()
    {
        // sbl -b: STOPTR returns the null string, so STOPTR(F,'CALL') STOPTR(F,'RETURN') concatenates to ''
        // (aisnobol's LTRACE does this); snobol4dotnet pushed nothing and crashed with an empty stack
        var build = SetupTests.SetupScript("-b", """
                 DEFINE('F()')                  :(FE)
         F                                      :(RETURN)
         FE      TRACE('F','CALL')
                 TRACE('F','RETURN')
                 X = STOPTR('F','CALL') STOPTR('F','RETURN')
                 R = DATATYPE(X) '[' X ']'
         end
         """);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("STRING[]", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R")]).Data);
    }

    [TestMethod]
    public void TEST_TraceEvents_values_print_as_prtvl()
    {
        // sbl -b: a trace line prints a value as prtvl does (CONS #1, ARRAY(2,3) #2, TABLE(11) #3, PATTERN, .X,
        // NAME for a keyword's name, *X for a simple expression, EXPRESSION, CODE), and a function OPSYN'd as G
        // traces under F, its own name. snobol4dotnet printed <CONS:CONS>, TABLE(0), <len>, <expression>,
        // <code@n> and the called name G
        Assert.AreEqual("""
            ****6*******  F(CONS #1)
            ****3*******  RETURN F = CONS #1
            ****7*******  F(ARRAY(2,3) #2)
            ****3*******  RETURN F = ARRAY(2,3) #2
            ****8*******  F(TABLE(11) #3)
            ****3*******  RETURN F = TABLE(11) #3
            ****9*******  F(PATTERN)
            ****3*******  RETURN F = PATTERN
            ****10******  F(.X)
            ****3*******  RETURN F = .X
            ****11******  F(NAME)
            ****3*******  RETURN F = NAME
            ****12******  F(*X)
            ****3*******  RETURN F = *X
            ****13******  F(EXPRESSION)
            ****3*******  RETURN F = EXPRESSION
            ****14******  F(CODE)
            ****3*******  RETURN F = CODE
            ****15******  F('via g')
            ****3*******  RETURN F = 'via g'
            """, SetupTests.RunWithInput("""
                    DATA('CONS(CAR,CDR)')
                    DEFINE('F(X)')                 :(FE)
            F       F = X                          :(RETURN)
            FE      OPSYN('G', 'F')
                    &FTRACE = 1000
                    F(CONS(1,2))
                    F(ARRAY('2,3'))
                    F(TABLE())
                    F(LEN(1))
                    F(.X)
                    F(.&ANCHOR)
                    F(*X)
                    F(*(X + 1))
                    F(CODE(' X = 1'))
                    G('via g')
            END
            """).Replace("\r\n", "\n"));
    }
}
