using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Operator;

// 208 IsKeyword value assigned is not integer                per keyword
// 209 IsKeyword in assignment is protected
// 210 IsKeyword value assigned is negative or too large

// 212 Syntax error: FunctionName used where name is required  X
// 251 IsKeyword operand is not name of defined keyword   X

// 205 String length exceeds value of MAXLNGTH keyword      Implemented by .NET
// 211 FunctionName assigned to keyword ERRTEXT not a string
// 244 Statement count exceeds value of STLIMIT keyword
// 268 Inconsistent value assigned to keyword PROFILE
// 287 FunctionName assigned to keyword MAXLNGTH is too small

[TestClass]
public class Keyword
{

    [TestMethod]
    public void TEST_Alphabet_001()
    {
        var s = @"
        a = &alphabet
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(255, ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data.Length);
    }

    [TestMethod]
    public void TEST_Alphabet_209()
    {
        var s = @"
        &alphabet = 'abcxyz'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(209, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Trim_208()
    {
        var s = @"
        &trim = 'a'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(208, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Unknown_212()
    {
        var s = @"
        a = &1
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(251, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Unknown_251()
    {
        var s = @"
        &unknown = 1
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreNotEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(251, build.ErrorCodeHistory[0]);
    }

    [TestMethod]
    public void TEST_Abend()
    {
        var s = @"
        &abend = 1
        a = &abend
        &abend = '1'
end
";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(1, build.ErrorCodeHistory.Count);
        Assert.AreEqual(1, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("a")]).Data);
        Assert.AreEqual(208, build.ErrorCodeHistory[0]);
    }
}