using Snobol4.Common;
using Test.TestLexer;

namespace Test.Griswold;

[TestClass]
public class RemoveLine
{
    [TestMethod]
    public void RemoveLine001()
    {
        var s = @"

        LIST = 'LEFT,HALT,LEFT,CANCEL,RIGHT,'
        NEXTI = BREAK(',') . ITEM LEN(1)
GETI    LIST NEXTI =          :F(END)
        RESULT = ITEM RESULT :(GETI)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("RIGHTCANCELLEFTHALTLEFT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }


    [TestMethod]
    public void RemoveLine002()
    {
        var s = @"

        LIST = 'LEFT,HALT,LEFT,CANCEL,RIGHT'
        NEXTI = BREAK(',') . ITEM LEN(1) | (LEN(1) REM) . ITEM
GETI    LIST NEXTI =          :F(END)
        RESULT = ITEM RESULT  :(GETI)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("RIGHTCANCELLEFTHALTLEFT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void Word001()
    {
        var s = @"
        TEXT = 'EVEN IF HE SAW ME, I WILL DENY IT.'
        LETTERS = 'ABCDEFGHIJKLNMNOPQRSTUVWXYZ'
        NEXTW = SPAN(LETTERS) . WORD
GETW    TEXT NEXTW =          :F(END)
        RESULT = RESULT WORD         :(GETW)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EVENIFHESAWMEIWILLDENYIT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void Word002()
    {
        var s = @"
        TEXT = 'EVEN IF HE SAW ME, I WILL DENY IT.'
        LETTERS = 'ABCDEFGHIJKLNMNOPQRSTUVWXYZ'
        NEXTW = BREAK(LETTERS) SPAN(LETTERS) . WORD
GETW    TEXT NEXTW =          :F(END)
        RESULT = RESULT WORD         :(GETW)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EVENIFHESAWMEIWILLDENYIT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }


    [TestMethod]
    public void Word003()
    {
        var s = @"
        TEXT = 'EVEN IF HE SAW ME, I WILL DENY IT.'
        LETTERS = 'ABCDEFGHIJKLNMNOPQRSTUVWXYZ'
        NEXTW = BREAK(LETTERS) (SPAN(LETTERS) (('-' SPAN(LETTERS)) | NULL)) . WORD
GETW    TEXT NEXTW =          :F(END)
        RESULT = RESULT WORD         :(GETW)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EVENIFHESAWMEIWILLDENYIT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }

    [TestMethod]
    public void Word004()
    {
        var s = @"
        TEXT = 'EVEN IF HE SAW MY WORK-SHEET, I WILL DENY IT.'
        LETTERS = 'ABCDEFGHIJKLNMNOPQRSTUVWXYZ'
        NEXTW = BREAK(LETTERS) (SPAN(LETTERS) (('-' SPAN(LETTERS)) | NULL)) . WORD
GETW    TEXT NEXTW =          :F(END)
        RESULT = RESULT WORD         :(GETW)
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual("EVENIFHESAWMYWORK-SHEETIWILLDENYIT", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }
    [TestMethod]
    public void Numbers001()
    {
        var s = @"
        SIGN = '+' | '-' | NULL
        DIGITS = SPAN('0123456789')
        INTEGER = DIGITS
        DFORM = DIGITS '.' (DIGITS | NULL)
        EFORM = 'E' SIGN DIGITS
        REAL = DFORM EFORM
        NUMBER = SIGN (REAL | INTEGER)

        '1234' NUMBER . O
        RESULT = RESULT ' ' O
        '-1234' NUMBER . O
        RESULT = RESULT ' ' O
        '+1234' NUMBER . O
        RESULT = RESULT ' ' O
        '1234.5678' NUMBER . O
        RESULT = RESULT ' ' O
        '-1234.5678' NUMBER . O
        RESULT = RESULT ' ' O
        '1234E+10' NUMBER . O
        RESULT = RESULT ' ' O
        '-1234E+10' NUMBER . O
        RESULT = RESULT ' ' O
        '1234.5678E-10' NUMBER . O
        RESULT = RESULT ' ' O
        '-1234.5678E-10' NUMBER . O
        RESULT = RESULT ' ' O
END";
        var directives = "-b";
        var build = SetupTests.SetupScript(directives, s);
        Assert.AreEqual(0, build.ErrorCodeHistory.Count);
        Assert.AreEqual(" 1234 -1234 +1234 1234.5678 -1234.5678 1234 -1234 1234.5678E-10 -1234.5678E-10", ((StringVar)build.Execute!.IdentifierTable[build.FoldCase("RESULT")]).Data);
    }


}


