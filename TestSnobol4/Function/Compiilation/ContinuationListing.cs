using Test.Miscellaneous;

namespace Test.Compilation;

/// <summary>
/// SPITBOL lists a line when it reads it and reads the lines of a statement as its scanner needs them, so the lines of a continued
/// statement up to the one an error is found in come before the page of the error and the rest after it; an error found with the whole
/// statement read (a missing right paren) stands after all its lines. The expected texts are what sbl -bf printed for the same file
/// under -LIST with the streams merged (2&gt;&amp;1), with the head's identity and clock masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ContinuationListing
{
    [TestMethod]
    public void TEST_ContinuationListing_an_error_in_the_first_line()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n        A = 1\n        OUTPUT = REPLAC?E(INPUT,\n+       1,\n+       2)\n        B = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1               A = 1\n2               OUTPUT = REPLAC?E(INPUT,\n2               OUTPUT = REPLAC?E(INPUT,\n                               !\n                               !\nmain.sno(3,24) : ERROR 233 -- syntax error: invalid use of operator\nmain.sno(3,24) : ERROR 233 -- syntax error: invalid use of operator\n\n\n        +       1,\n        +       2)\n3               B = 2\n4       END", IncludeInSourceOrder.Run(files));
    }

    [TestMethod]
    public void TEST_ContinuationListing_an_error_in_a_middle_line()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n        A = 1\n        OUTPUT = REPLACE(INPUT,\n+       1 ?,\n+       2)\n        B = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1               A = 1\n2               OUTPUT = REPLACE(INPUT,\n        +       1 ?,\n        +       1 ?,\n                   !\n                   !\nmain.sno(4,12) : ERROR 221 -- syntax error: missing operand\nmain.sno(4,12) : ERROR 221 -- syntax error: missing operand\n\n\n        +       2)\n3               B = 2\n4       END", IncludeInSourceOrder.Run(files));
    }

    [TestMethod]
    public void TEST_ContinuationListing_an_error_at_the_end_of_the_statement()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n        A = 1\n        OUTPUT = REPLACE(INPUT,\n+       1,\n+       2\n        B = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1               A = 1\n2               OUTPUT = REPLACE(INPUT,\n        +       1,\n        +       2\n        +       2\n                !\n                !\nmain.sno(5,9) : ERROR 226 -- syntax error: missing right paren\nmain.sno(5,9) : ERROR 226 -- syntax error: missing right paren\n\n\n3               B = 2\n4       END", IncludeInSourceOrder.Run(files));
    }
}
