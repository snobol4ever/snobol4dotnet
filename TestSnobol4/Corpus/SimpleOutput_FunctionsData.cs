using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Corpus;

/// <summary>
/// Corpus functions/ — DEFINE, CALL, RETURN, FRETURN, recursion, entry labels.
/// Corpus data/     — ARRAY, TABLE, DATA type creation and access.
/// </summary>
[TestClass]
public class SimpleOutput_FunctionsData
{
    private static Builder Run(string script) =>
        SetupTests.SetupScript("-b", script);

    // -----------------------------------------------------------------------
    // functions/
    // -----------------------------------------------------------------------

    [TestMethod]
    public void TEST_Corpus_083_define_simple_return()
    {
        var s = @"
        DEFINE('double(s)')                                         :(double_end)
double  double = 2 * s                                             :(RETURN)
double_end
        R1 = double(5)
        R2 = double(21)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(10L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(42L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_084_define_loop_call()
    {
        var s = @"
        DEFINE('bump(v)')                                           :(bumpend)
bump    bump = v + 1                                               :(RETURN)
bumpend
        S = ''
        J = 0
LOOP    S = S bump(2 * J)
        J = J + 1
        LT(J, 5)                                                    :S(LOOP)
        RESULT = S
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // bump(2*J) for J=0..4: bump(0)=1, bump(2)=3, bump(4)=5, bump(6)=7, bump(8)=9
        Assert.AreEqual("13579", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_085_define_two_args()
    {
        var s = @"
        DEFINE('add(a,b)')                                          :(add_end)
add     add = a + b                                                :(RETURN)
add_end
        R1 = add(3, 4)
        R2 = add(10, 32)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(7L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(42L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_086_define_locals()
    {
        var s = @"
        DEFINE('swap(a,b)tmp')                                      :(swap_end)
swap    tmp = a
        a = b
        b = tmp
        SWAPRESULT = a ' ' b                                        :(RETURN)
swap_end
        swap('hello', 'world')
        RESULT = SWAPRESULT
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("world hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_087_define_freturn_positive()
    {
        var s = @"
        DEFINE('ispos(x)')                                          :(ispos_end)
ispos   GT(x, 0)                                                   :S(RETURN)F(FRETURN)
ispos_end
        ispos(5)                                                    :S(A)F(B)
A       RESULT = 'positive'
        :(END)
B       RESULT = 'wrong'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("positive", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_087_define_freturn_negative()
    {
        var s = @"
        DEFINE('ispos(x)')                                          :(ispos_end)
ispos   GT(x, 0)                                                   :S(RETURN)F(FRETURN)
ispos_end
        ispos(-3)                                                   :S(C)F(D)
C       RESULT = 'wrong'
        :(END)
D       RESULT = 'not positive'
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("not positive", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_088_define_recursive_fib()
    {
        var s = @"
        DEFINE('fib(n)')                                            :(fib_end)
fib     LE(n, 1)                                                   :S(base)
        fib = fib(n - 1) + fib(n - 2)                             :(RETURN)
base    fib = n                                                    :(RETURN)
fib_end
        R1 = fib(0)
        R2 = fib(1)
        R3 = fib(6)
        R4 = fib(10)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(0L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(1L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual(8L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
        Assert.AreEqual(55L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R4")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_089_define_in_pattern()
    {
        var s = @"
        DEFINE('upcase(s)')                                         :(upcase_end)
upcase  upcase = REPLACE(s, &LCASE, &UCASE)                       :(RETURN)
upcase_end
        R1 = upcase('hello')
        R2 = upcase('world')
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("HELLO", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("WORLD", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_090_define_entry_label()
    {
        var s = @"
        DEFINE('bumpit(v)', .bumpit)                                :(bumpend)
bumpit  bumpit = v + 1                                             :(RETURN)
bumpend
        RESULT = bumpit(41)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(42L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    // -----------------------------------------------------------------------
    // data/
    // -----------------------------------------------------------------------

    [TestMethod]
    public void TEST_Corpus_091_array_create_access()
    {
        var s = @"
        A = ARRAY(5)
        A<1> = 'first'
        A<3> = 'third'
        A<5> = 'fifth'
        R1 = A<1>
        R2 = A<3>
        R3 = A<5>
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("first", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("third", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("fifth", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_092_array_loop_fill()
    {
        var s = @"
        A = ARRAY(4)
        I = 1
FILL    A<I> = I * I
        I = I + 1
        LE(I, 4)                                                    :S(FILL)
        R1 = A<1>
        R2 = A<2>
        R3 = A<3>
        R4 = A<4>
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(1L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(4L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual(9L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
        Assert.AreEqual(16L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R4")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_093_table_create_access()
    {
        var s = @"
        T = TABLE()
        T['name'] = 'Alice'
        T['age'] = 30
        T['lang'] = 'SNOBOL4'
        R1 = T['name']
        R2 = T['age']
        R3 = T['lang']
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("Alice",   ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(30L,       ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual("SNOBOL4", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_094_data_define_access()
    {
        var s = @"
        DATA('complex(real,imag)')
        X = complex(3, -2)
        R1 = real(X)
        R2 = imag(X)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(3L,  ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(-2L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_095_data_field_set()
    {
        var s = @"
        DATA('point(x,y)')
        P = point(10, 20)
        R1 = x(P)
        R2 = y(P)
        x(P) = 99
        R3 = x(P)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(10L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual(20L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
        Assert.AreEqual(99L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("R3")]).Data);
    }

    [TestMethod]
    public void TEST_Corpus_096_data_datatype_check()
    {
        var s = @"
        DATA('node(val,next)')
        N = node('hello', '')
        R1 = DATATYPE(N)
        R2 = val(N)
end";
        var build = Run(s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        // DOTNET DATATYPE: user-defined types are UPPERCASE, builtins are lowercase
        // SPITBOL MINIMAL folds DATA type names to lowercase (flstg at sdat1 in sbl.min)
        Assert.AreEqual("node",  ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R1")]).Data);
        Assert.AreEqual("hello", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("R2")]).Data);
    }
}
