using Test.TestLexer;

namespace Test.TestCommandLine;

/// <summary>
/// A program with compilation errors does not run (-NOERRORS, the default); -k or the -ERRORS control runs
/// it, every error is reported, the statements after one in error compile with their labels, and reaching
/// the statement in error is error 7. Expected results are sbl's for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class CompileErrors
{
    private const string SkipsTheError = """
         OUTPUT = 'first'  :(E)
         X = 'a' (
        E OUTPUT = 'last'
        F OUTPUT = 'f'
        END
        """;

    [TestMethod]
    public void TEST_CompileErrors_suppress_execution()
    {
        // sbl -b prints the two errors and runs nothing; snobol4dotnet ran the program
        var build = SetupTests.SetupScript("-b", """
                 OUTPUT = 'first'
                 X = 'a' (
                 Y = )
                 R = 'ran'
         END
         """);
        CollectionAssert.AreEqual(new[] { 226, 224 }, build.ErrorCodeHistory.ToArray());
        Assert.AreEqual("", build.Execute!.IdentifierTable[build.FoldCase("R")].ToString());
        Assert.AreEqual("", SetupTests.RunWithInput(SkipsTheError));
    }

    [TestMethod]
    public void TEST_CompileErrors_k_and_ERRORS_run()
    {
        // sbl -bk and -ERRORS print "first last f": the label E after the statement in error is defined
        Assert.AreEqual("first\nlast\nf", SetupTests.RunWithInputDirectives("-b -k", SkipsTheError).Replace("\r\n", "\n"));
        Assert.AreEqual("first\nlast\nf", SetupTests.RunWithInput("-ERRORS\n" + SkipsTheError).Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_CompileErrors_reaching_the_statement_is_error_7()
    {
        // sbl -bk: reaching the statement in error is error 7, which SETEXIT catches like any error
        Assert.AreEqual("first\ncaught 7\nafter", SetupTests.RunWithInputDirectives("-b -k", """
                 &ERRLIMIT = 5
                 SETEXIT(.H)
                 OUTPUT = 'first'
                 X = 'a' (
                 OUTPUT = 'after'  :(END)
         H       OUTPUT = 'caught ' &ERRTYPE  :(CONTINUE)
         END
         """).Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void TEST_CompileErrors_misplaced_continuation_is_214()
    {
        // sbl -b: a continuation line after a line closed by a semicolon, after a comment, or with nothing
        // before it is error 214 (testpgms' test2 has one after "... :S(D77) ;"), while one after an open
        // statement continues it; snobol4dotnet joined the first two and skipped the third as a comment
        foreach (var program in new[]
                 {
                     " X = 'a' ;\n+ 'b'\n OUTPUT = X\nEND",
                     "* comment\n+ 'b'\n OUTPUT = 'x'\nEND",
                     "+ 'b'\n OUTPUT = 'x'\nEND"
                 })
        {
            var build = SetupTests.SetupScript("-b", program);
            CollectionAssert.AreEqual(new[] { 214 }, build.ErrorCodeHistory.ToArray(), program);
        }

        Assert.AreEqual("ab", SetupTests.RunWithInput(" X = 'a'\n+ 'b'\n OUTPUT = X\nEND"));
    }
}
