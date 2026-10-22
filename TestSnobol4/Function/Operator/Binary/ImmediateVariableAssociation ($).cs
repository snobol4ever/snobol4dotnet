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
        Assert.AreEqual("success", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("result")]).Data);
        Assert.AreEqual("is  si", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
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
        Assert.AreEqual("BEAR", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
        Assert.AreEqual("ROOS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp2")]).Data);
        Assert.AreEqual("DS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp3")]).Data);
        Assert.AreEqual("ITS", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
    }

    // P $ V where P holds *P: the start of each activation of P is its own, as sbl -bf gives, not the start of the
    // innermost one (was GRP = (3,45),78), V = XY)) and G = <>>)
    [TestMethod]
    public void TEST_ImmediateVariableAssociation_Recursive_001()
    {
        var s = @"
        ITEM = SPAN('0123456789') $ LAST | *LIST
        LIST = ('(' ITEM ARBNO(',' ITEM) ')') $ GRP
        '(12,(3,45),78)' ? POS(0) LIST RPOS(0)
        E = ('(' *E ')' | 'X' | 'XY') $ V
        '((XY))' ? POS(0) E $ W RPOS(0)
        L = ('<' ARBNO(*L) '>') $ G
        '<<<>><>>' ? POS(0) L RPOS(0)
END";

        var build = SetupTests.SetupScript("-bf", s);
        Assert.AreEqual("78", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("LAST")]).Data);
        Assert.AreEqual("(12,(3,45),78)", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("GRP")]).Data);
        Assert.AreEqual("((XY))", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("V")]).Data);
        Assert.AreEqual("((XY))", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("W")]).Data);
        Assert.AreEqual("<<<>><>>", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("G")]).Data);
    }

    // Every firing at every depth, in order, as sbl -bf gives (was b} c} d} d}})
    [TestMethod]
    public void TEST_ImmediateVariableAssociation_Recursive_002()
    {
        var s = @"
        T = ('{' ARBNO(*T | NOTANY('{}')) '}') $ X *DIFFER(L = L X ' ')
        '{a{b}{{c}d}}' ? POS(0) T RPOS(0)
END";

        var build = SetupTests.SetupScript("-bf", s);
        Assert.AreEqual("{b} {c} {{c}d} {a{b}{{c}d}} ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("L")]).Data);
    }

    // Backup into P after its $ fired, inside a recursion and through FENCE: each firing reads its own start again,
    // as sbl -bf gives
    [TestMethod]
    public void TEST_ImmediateVariableAssociation_Recursive_003()
    {
        var s = @"
        E2 = ('[' *E2 ']' | 'Q' | 'QR') $ X *DIFFER(L = L X ' ')
        '[[QR]]' ? POS(0) E2 RPOS(0)
        'AXYC' ? POS(0) (FENCE(('A' 'X') $ Y) ('Y' | 'YC') $ Z) $ W RPOS(0)
END";

        var build = SetupTests.SetupScript("-bf", s);
        Assert.AreEqual("Q QR [QR] [[QR]] ", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("L")]).Data);
        Assert.AreEqual("AX", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("Y")]).Data);
        Assert.AreEqual("YC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("Z")]).Data);
        Assert.AreEqual("AXYC", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("W")]).Data);
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
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp1")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp2")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp3")]).Data);
        Assert.AreEqual("", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("temp4")]).Data);
    }

}