using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// A real converted to a NAME or an EXPRESSION takes SPITBOL's text of the real, as CONVERT to STRING and PATTERN already did: the name of
/// 100.0 is 100. and of -0.0 is 0., and the expression of 100.0 evaluates to the real 100., where .NET's text (100, -0, 1E-10) gave another
/// name and an expression that evaluated to the integer 100. Found by the infinite_snobol4 generator (... '4' (CONVERT(-0.0, 'NAME')):
/// 1540. in sbl, 154-0 here). Every expected line is what sbl -bf printed (under -b the name is case-folded too, ConvertedNameFolding).
/// </summary>
[DoNotParallelize]
[TestClass]
public class RealNameText
{
    [TestMethod]
    public void TEST_RealNameText_is_spitbols_text_of_the_real()
    {
        var program = @"        OUTPUT = 'x' CONVERT(-0.0, 'NAME')
        OUTPUT = 'x' CONVERT(100.0, 'NAME')
        OUTPUT = 'x' CONVERT(1.0E-10, 'NAME')
        OUTPUT = 'x' CONVERT(1.0E20, 'NAME')
        OUTPUT = 'x' CONVERT(2.5, 'NAME')
        V = EVAL(CONVERT(100.0, 'EXPRESSION'))
        OUTPUT = DATATYPE(V) ' ' V
        V = EVAL(CONVERT(-0.0, 'EXPRESSION'))
        OUTPUT = DATATYPE(V) ' ' V
END";
        Assert.AreEqual(@"x0.
x100.
x0.1e-9
x0.1e+21
x2.5
REAL 100.
REAL 0.", SetupTests.RunWithInputDirectives("-bf", program).Replace("\r\n", "\n").TrimEnd('\n'));
    }
}
