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
    public void TEST_Define_locals_and_parameters_keep_their_associations()
    {
        // sbl -b: a local F associated with a file still reads it ([a,1] inside G, [b,2] after), and assigning a
        // parameter O associated with an output file writes there (aisnobol's LOADEX reads its library through
        // a local LIB.FILE); snobol4dotnet nulled the local's association and looped
        var dir = Path.Combine(Path.GetTempPath(), "snolocal_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var input = Path.Combine(dir, "idx.txt");
            var output = Path.Combine(dir, "o.txt");
            File.WriteAllText(input, "a,1\nb,2\n");
            var build = SetupTests.SetupScript("-b", $@"
        INPUT(.F, 15, '{input}')
        DEFINE('G()F')                 :(GE)
G       G = F                          :(RETURN)
GE      R = '[' G() ']'
        S = '[' F ']'
        OUTPUT('O', 16, '{output}')
        DEFINE('H(O)')                 :(HE)
H       O = 'via param'                :(RETURN)
HE      H('x')
        ENDFILE(16)
end");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("[a,1] [b,2]", $"{build.Execute!.IdentifierTable["R"]} {build.Execute!.IdentifierTable["S"]}");
            Assert.AreEqual("via param\n", File.ReadAllText(output).Replace("\r\n", "\n"));
        }
        finally { Directory.Delete(dir, true); }
    }

}