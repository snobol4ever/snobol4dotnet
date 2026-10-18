using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo

namespace Test.Pattern;

[TestClass]
public class ConditionalAssignment
{
    [TestMethod]
    public void TEST_ConditionalVariableAssociation_001()
    {
        var s = @"
	subject = 'mississippi'
	pattern = 'is' . temp1  'si' . temp2
	subject pattern :s(y)f(n)
y	result = 'success' 
	temp4 = temp3 = temp1 '  ' temp2  :(end)
n	result = 'fail'

end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("is  si", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_002()
    {
        var s = @"
        subject = 'BEARROOSDSITS'
        P = 'BE'  | 'BEA' | 'BEAR'
        Q = 'RO'  | 'ROO' | 'ROOS'
        R = 'D'  | 'DS'
        S = 'ITA' | 'ITS'
        PAT = P . temp1  Q . temp2  R . temp3  S . temp4
        subject PAT
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("BEAR", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
        Assert.AreEqual("ROOS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp2")]).Data);
        Assert.AreEqual("DS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp3")]).Data);
        Assert.AreEqual("ITS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_003()
    {
        var s = @"
        subject = 'BEARROOSDSITS'
        P = 'BE'  | 'BEA' | 'BEAR'
        Q = 'RO'  | 'ROO' | 'ROOS'
        R = 'D'  | 'DS'
        S = 'ITA' | 'ITT'
        PAT = P . temp1  Q . temp2  R . temp3  S . temp4
        subject PAT
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp2")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp3")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
    }
    [TestMethod]
    public void TEST_ConditionalVariableAssociation_004()
    {
        // .var capture inside ARB — gets the matched span
        var s = @"
        subject = 'hello world'
        subject arb . captured 'world'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("hello ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("captured")]).Data);
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_005()
    {
        // .var with LEN: captures exact span
        var s = @"
        subject = 'abcdef'
        subject len(3) . first len(3) . second
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("abc", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("first")]).Data);
        Assert.AreEqual("def", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("second")]).Data);
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_006()
    {
        // .var not assigned on match failure (variable stays empty)
        var s = @"
        subject = 'xyz'
        subject 'abc' . captured   :s(matched)
        result = 'no-match'        :(end)
matched result = 'matched'
end";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("no-match", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("captured")]).Data);
    }

    // *X . V defers X to match time, as SPITBOL does (sbl -b; Budne's uneval2)
    private static string Captured(string body)
    {
        var build = SetupTests.SetupScript("-b", body + "\nend");
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        return ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("str")]).Data;
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_of_a_deferred_pattern()
    {
        Assert.AreEqual("HEL", Captured(@"
        'HELLO' *LEN(3) . str"));
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_of_a_deferred_pattern_reads_its_variables_at_match_time()
    {
        Assert.AreEqual("HE", Captured(@"
        p = *BREAK(q)
        q = 'L'
        'HELLO' p . str"));
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_of_a_deferred_argument_set_earlier_in_the_same_match()
    {
        Assert.AreEqual("HELLO HELLO", Captured(@"
        line = 'say ""HELLO HELLO"" now'
        line (""'"" | '""') $ quote *BREAK(quote) . str *quote"));
    }
}
