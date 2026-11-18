using Test.TestLexer;

namespace Test.Compiilation;

/// <summary>
/// After a control's name sbl's scanner reads the next token, and a character no token begins with is error 230, illegal character: a
/// carriage return ending -LIST (col 6), a form feed after -NOLIST, a backquote, a backslash, a brace, an underscore, a byte above 126;
/// a name, an operator or a comma is read as before. A control that reads the rest of its line (-TITLE, -EJECT, -INCLUDE) keeps the carriage
/// return as a character of it. snobol4dotnet trimmed every white space character off a control and read any other character as the end of
/// the card, so snolib's g360.spt, a CR LF file, listed no error at its -LIST-style cards and a title one character short. The answers are
/// sbl -bf's.
/// </summary>
[TestClass]
public class AnIllegalCharacterAfterAControlIs230
{
    private static string Errors(string control) =>
        string.Join(" ", SetupTests.SetupScript("-bf", control + "\n        X = 1\nEND\n").ErrorCodeHistory);

    [TestMethod]
    public void TEST_AnIllegalCharacterAfterAControlIs230()
    {
        Assert.AreEqual("230", Errors("-LIST\r"));
        Assert.AreEqual("230", Errors("-NOLIST\f"));
        Assert.AreEqual("230", Errors("-LIST `"));
        Assert.AreEqual("230", Errors("-LIST _"));
        Assert.AreEqual("", Errors("-LIST X"));
        Assert.AreEqual("", Errors("-LIST !"));
        Assert.AreEqual("", Errors("-EJECT\r"));
    }
}
