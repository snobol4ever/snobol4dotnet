using Test.Miscellaneous;

namespace Test.Compilation;

/// <summary>
/// The statements of a line, split at its semicolons, as SPITBOL reads them: the rest of a line after a bad label is skipped, the
/// statement after one in error at its semicolon is taken for the rest of it and is none (it has no number), and a second error on a
/// line that a page has shown has an empty line where the line stood. The expected texts are what sbl -bf printed for the same file
/// under -LIST with the streams merged (2&gt;&amp;1), with the head's identity and clock masked.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SemicolonStatements
{
    [TestMethod]
    public void TEST_SemicolonStatements_a_bad_label_skips_the_rest_of_the_line()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n\tA = 1\n%* XXX SIGH; SPITBOL will output 1\n\tB = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1       \tA = 1\n2       %* XXX SIGH; SPITBOL will output 1\n2       %* XXX SIGH; SPITBOL will output 1\n\n\nmain.sno(3) : ERROR 214 -- bad label or misplaced continuation line\nmain.sno(3) : ERROR 214 -- bad label or misplaced continuation line\n\n\n3       \tB = 2\n4       END", IncludeInSourceOrder.Run(files));
    }

    [TestMethod]
    public void TEST_SemicolonStatements_the_statement_after_an_error_at_a_semicolon_has_no_number()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n\tA = 1\n\tENDFILE(9;8)\n\n\tB = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1       \tA = 1\n2       \tENDFILE(9;8)\n2       \tENDFILE(9;8)\n        \t         !\n        \t         !\nmain.sno(3,11) : ERROR 226 -- syntax error: missing right paren\nmain.sno(3,11) : ERROR 226 -- syntax error: missing right paren\n\n\n3       \n4       \tB = 2\n5       END", IncludeInSourceOrder.Run(files));
    }

    [TestMethod]
    public void TEST_SemicolonStatements_a_second_error_on_a_line_has_an_empty_line_for_the_line()
    {
        var files = new Dictionary<string, string> { ["main.sno"] = "-LIST\n\tA = 1\n\tENDFILE(9;8;C = 3)\n\tB = 2\nEND\n" };
        Assert.AreEqual("\n\n\n<identity>\n<clock>\n\n\n\n                                                                                                                page 1\n\n        -LIST\n1       \tA = 1\n2       \tENDFILE(9;8;C = 3)\n2       \tENDFILE(9;8;C = 3)\n        \t         !\n        \t         !\nmain.sno(3,11) : ERROR 226 -- syntax error: missing right paren\nmain.sno(3,11) : ERROR 226 -- syntax error: missing right paren\n\n\n\n\n        \t              !\n        \t              !\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand\nmain.sno(3,16) : ERROR 221 -- syntax error: missing operand\n\n\n4       \tB = 2\n5       END", IncludeInSourceOrder.Run(files));
    }
}
