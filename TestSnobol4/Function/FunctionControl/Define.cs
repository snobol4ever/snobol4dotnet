using Snobol4.Common;
using Test.TestLexer;

namespace Test.FunctionControl;

[TestClass]
public class Define
{
    [TestMethod]
    public void TEST_Function_Bump1()
    {
        var s = @"
        define('bump(v)',.bumpit) :(bumpend)
bumpit  bump = v + 1 :(return)
bumpend
        s = ''
        j = 0
loop    s = s bump(2 * j)
        j = j + 1
        lt(j,10)     :s(loop)
        
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("135791113151719", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("s")]).Data);
    }

    [TestMethod]
    public void TEST_Function_Bump2()
    {
        var s = @"
        define('bump(v)') :(bumpend)
bump    bump = v + 1 :(return)
bumpend
        s = ''
        j = 0
loop    s = s bump(2 * j)
        j = j + 1
        lt(j,10)     :s(loop)
        
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("135791113151719", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("s")]).Data);
    }

    [TestMethod]
    public void TEST_Function_Double1()
    {
        var s = @"				define('double(s)')  :(double_end)
double			double = 2 * s	:(return)
double_end		b = double(5)
				output = b";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(10L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Function_Pythagoras()
    {
        var s = @"

                define('pythagoras(a,b)')  :(double_end)
pythagoras      pythagoras = sqrt(a * a + b * b)  :(return)
double_end      b = pythagoras(4,12)
                output = b
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(Math.Sqrt(160), ((RealVar)build.Execute!.IdentifierTable[build.FoldCase("b")]).Data);
    }

    [TestMethod]
    public void TEST_Function_Fibonacci()
    {
        // Not the way to do fibonacci, but a good stress test of recursion
        var s = @"
                    define('fibonacci(n)')  :(fibonacci_end)
fibonacci           output = n '  (' &fnclevel ')'
                    le(n,1)  :s(next)
                    fibonacci = fibonacci(n - 1) 
                    fibonacci = fibonacci + fibonacci(n - 2)  
                         :(return)
next                fibonacci = n  :(return)    
fibonacci_end       f = fibonacci(12)
                    output = f
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(144L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("f")]).Data);
    }

    [TestMethod]
    public void TEST_Define_001()
    {
        var s = @"
        DEFINE('SHIFT(S,N)FRONT,REST')
        SHIFT_PAT = LEN(*N) . FRONT REM . REST :(SHIFT_END)
SHIFT   S ? SHIFT_PAT :F(FRETURN)
        SHIFT = REST FRONT :(RETURN)
SHIFT_END
        r = SHIFT('COTTON',4)
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ONCOTT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("r")]).Data);
    }

    [TestMethod]
    public void TEST_Define_002()
    {
        var s = @"
        DEFINE('SHIFT(S,N)FRONT,REST')
        SHIFT_PAT = LEN(*N) . FRONT REM . REST :(SHIFT_END)
SHIFT   S ? SHIFT_PAT :F(FRETURN)
        SHIFT = REST FRONT :(RETURN)
SHIFT_END
        r = SHIFT('OAK',4) :S(end)
        s = 'FAILURE'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("FAILURE", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("s")]).Data);
    }

    [TestMethod]
    public void TEST_Define_006()
    {
        var s = @"
        define('add(x,y)front,rest')
        front = 99
        rest = 88
        x = 77
        y = 66             :(addend)
add     add = x + y        
        front = 9
        rest = 8            :(return)
addend  output = a = add(3,4)
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s + ";end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(7L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(99L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("front")]).Data);
        Assert.AreEqual(88L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("rest")]).Data);
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_Define_007_error_inside_function_ends_program()
    {
        // With &ERRLIMIT = 0 a runtime error ends the whole program, not just
        // the function it occurs in (SPITBOL and CSNOBOL4 both stop at the
        // first error 22 and never reach the next statement).
        var s = @"
        define('f()')       :(fend)
f       x = undef()         :(return)
fend    f()
        after = 'ran'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        CollectionAssert.AreEqual(new List<int> { 22 }, build.ErrorCodeHistory);
        var key = build.FoldCase("after");
        Assert.IsFalse(build.Execute!.IdentifierTable.ContainsKey(key)
                       && build.Execute.IdentifierTable[key] is StringVar { Data: "ran" });
    }

    [TestMethod]
    [Timeout(10000)]
    public void TEST_Define_008_error_two_functions_deep_ends_program()
    {
        var s = @"
        define('f()')       :(fend)
f       g()                 :(return)
fend    define('g()')       :(gend)
g       x = undef()         :(return)
gend    f()
        after = 'ran'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        CollectionAssert.AreEqual(new List<int> { 22 }, build.ErrorCodeHistory);
        var key = build.FoldCase("after");
        Assert.IsFalse(build.Execute!.IdentifierTable.ContainsKey(key)
                       && build.Execute.IdentifierTable[key] is StringVar { Data: "ran" });
    }

    [TestMethod]
    public void TEST_Define_the_entry_label_is_needed_only_at_the_call()
    {
        // sbl -b: DEFINE('ZZ(X)') with no ZZ label succeeds, CODE then supplies the label and ZZ(1) is 2;
        // a call while the label is undefined is error 286
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('ZZ(X)')
        CODE('ZZ ZZ = X + 1 :(RETURN)')
        R = ZZ(1)
        DEFINE('YY(X)')
        S = YY(1)
end");
        Assert.AreEqual("2", build.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual(1, build.ErrorCodeHistory.Count);
        Assert.AreEqual(286, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Define_an_nreturn_gives_the_named_value()
    {
        // sbl -b: N = TOP() copies V's value (a STRING), TOP() 'x' and SIZE(TOP()) use the value, TOP() = Y
        // sets V even when V holds a NAME, .TOP() is V's name, and an element's NAME works the same way
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('TOP()')                :(TOP_END)
TOP     TOP = .V                       :(NRETURN)
TOP_END
        V = 'val'
        N = TOP()
        R = N ' ' DATATYPE(N) ' ' TOP() 'x ' SIZE(TOP())
        X = .TOP()
        V = .Z
        TOP() = 'set'
        S = V ' ' Z ' ' DATATYPE(X)
        A = ARRAY(2)
        DEFINE('EL()')                 :(EL_END)
EL      EL = .A<2>                     :(NRETURN)
EL_END
        EL() = 'two'
        T = A<2> ' ' EL()
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("val STRING valx 3", build.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual("set  NAME", build.Execute!.IdentifierTable["S"].ToString());
        Assert.AreEqual("two two", build.Execute!.IdentifierTable["T"].ToString());
    }

    [TestMethod]
    public void TEST_Define_a_return_of_a_name_gives_the_name()
    {
        // sbl -b (rung user_function_datatype_1): G = .DUMMY :(RETURN) gives the NAME, printed DUMMY
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('G()')                  :(GEND)
G       G = .DUMMY                     :(RETURN)
GEND    DUMMY = 'VAL'
        RV = G()
        R = RV ' ' DATATYPE(RV)
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("DUMMY NAME", build.Execute!.IdentifierTable["R"].ToString());
    }

    [TestMethod]
    public void TEST_Define_the_function_variable_survives_a_recursive_call()
    {
        // sbl -b: inside F(1), F keeps v1 across the inner F(2) call, F(1) returns v1, and the global F is
        // global again after the call
        var build = SetupTests.SetupScript("-b", @"
        DEFINE('F(N)')                 :(F_END)
F       F = 'v' N
        LT(N, 2)                       :F(RETURN)
        X = F(N + 1)
        INNER = F ' ' X                :(RETURN)
F_END
        F = 'global'
        R = F(1)
        S = F
end");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("v1 v2", build.Execute!.IdentifierTable["INNER"].ToString());
        Assert.AreEqual("v1", build.Execute!.IdentifierTable["R"].ToString());
        Assert.AreEqual("global", build.Execute!.IdentifierTable["S"].ToString());
    }
}
