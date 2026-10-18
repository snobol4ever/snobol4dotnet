using Snobol4.Common;
using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// NULL is an ordinary variable, as in SPITBOL: unassigned, its value is the null string.
/// Expected results are SPITBOL's (sbl -bf).
/// </summary>
[TestClass]
public class NullIsUnassigned
{
    private static string Str(Builder b, string name) =>
        ((StringVar)b.Execute!.IdentifierTable[b.FoldCase(name)]).Data;

    [TestMethod]
    public void TEST_NULL_is_the_null_string()
    {
        var s = @"
        s = 'x'
        s 'x' =
        a = IDENT(s, NULL) 'ident'
        c = SIZE(NULL)
end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("ident", Str(build, "a"));
        Assert.AreEqual(0L, ((IntegerVar)build.Execute!.IdentifierTable[build.FoldCase("c")]).Data);
    }

    [TestMethod]
    public void TEST_NULL_still_matches_as_the_null_pattern()
    {
        var s = @"
        'abc' ? NULL 'a'   :f(n)
        r = 'S'            :(end)
n       r = 'F'
end";
        var build = SetupTests.SetupScript("-b", s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("S", Str(build, "r"));
    }
}
