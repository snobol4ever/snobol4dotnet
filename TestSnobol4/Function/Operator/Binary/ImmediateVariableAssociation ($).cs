using Snobol4.Common;
using Test.TestLexer;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace Test.Pattern;

[TestClass]
public class ImmediateVariableAssignment
{

    [TestMethod]
    public void TEST_ImmediateVariableAssociation_001()
    {
        var s = @"
	subject = 'mississippi'
	pattern = 'is' $ temp1  'si' $ temp2
	subject pattern :s(y)f(n)
y	result = 'success' 
	temp4 = temp3 = temp1 '  ' temp2  :(end)
n	result = 'fail'

end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("is  si", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp4")]).Data);
    }

    /* P = 'BE' I 'BEA' I 'BEAR'
       Q = 'RO' I 'ROO' 1 'ROOS'
       R = 'DS' 1 'D'
       S = ITS' I IT'
       PAT = PRlQs8
    */

    [TestMethod]
    public void TEST_ImmediateVariableAssociation_002()
    {
        var s = @"
        subject = 'BEARROOSDSITS'
        P = 'BE'  | 'BEA' | 'BEAR'
        Q = 'RO'  | 'ROO' | 'ROOS'
        R = 'D'  | 'DS'
        S = 'ITA' | 'ITS'
        PAT = P $ temp1  Q $ temp2  R $ temp3  S $ temp4
        subject PAT
end";

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("BEAR", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
        Assert.AreEqual("ROOS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp2")]).Data);
        Assert.AreEqual("DS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp3")]).Data);
        Assert.AreEqual("ITS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp4")]).Data);
    }

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
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","result")]).Data);
        Assert.AreEqual("is  si", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp4")]).Data);
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
        Assert.AreEqual("BEAR", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
        Assert.AreEqual("ROOS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp2")]).Data);
        Assert.AreEqual("DS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp3")]).Data);
        Assert.AreEqual("ITS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp4")]).Data);
    }

    [TestMethod]
    public void TEST_ConditionalVariableAssociation_003()
    {
        // ReSharper disable StringLiteralTypo
        var s = @"
        subject = 'BEARROOSDSITS'
        P = 'BE'  | 'BEA' | 'BEAR'
        Q = 'RO'  | 'ROO' | 'ROOS'
        R = 'D'  | 'DS'
        S = 'ITA' | 'ITT'
        PAT = P . temp1  Q . temp2  R . temp3  S . temp4
        subject PAT
end";
        // ReSharper restore StringLiteralTypo

        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp2")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp3")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("","temp4")]).Data);
    }

}