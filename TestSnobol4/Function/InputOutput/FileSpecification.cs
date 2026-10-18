using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// INPUT and OUTPUT read their third argument as SPITBOL does: a filename, then I/O options after a blank
/// or in a trailing [...]. An existing output file is replaced unless the A option appends to it, and an
/// unknown option is error 160 (OUTPUT) or 116 (INPUT). Each expectation is what sbl -b does.
/// </summary>
[TestClass]
public class FileSpecification
{
    [TestMethod]
    public void TEST_FileSpec_replace_append_and_bracketed_options()
    {
        var dir = Path.Combine(Path.GetTempPath(), "snofilespec_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var replaced = Path.Combine(dir, "replaced.txt");
            var appended = Path.Combine(dir, "appended.txt");
            var bracketed = Path.Combine(dir, "bracketed.txt");
            File.WriteAllText(replaced, "a very long line\nsecond\n");
            File.WriteAllText(appended, "first\n");
            var build = SetupTests.SetupScript("-b", $@"
        OUTPUT('O', 7, '{replaced}')
        O = 'hi'
        ENDFILE(7)
        OUTPUT('P', 8, '{appended} -a')
        P = 'more'
        ENDFILE(8)
        OUTPUT('Q', 9, '{bracketed}[-a]')
        Q = 'q'
        ENDFILE(9)
end");
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("hi\n", File.ReadAllText(replaced).Replace("\r\n", "\n"));
            Assert.AreEqual("first\nmore\n", File.ReadAllText(appended).Replace("\r\n", "\n"));
            Assert.AreEqual("q\n", File.ReadAllText(bracketed).Replace("\r\n", "\n"));
            Assert.AreEqual(3, Directory.GetFiles(dir).Length);
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void TEST_FileSpec_an_unknown_option_is_refused()
    {
        // a FORTRAN format as the third argument, as old SNOBOL4 programs pass it: the text after the
        // first blank is read as options, and T is not one
        var output = SetupTests.SetupScript("-b", @"
        OUTPUT('SUM', 6, ""(' THE RESULT IS: ', I10)"")
end");
        Assert.AreEqual(160, output.ErrorCodeHistory[0]);

        var input = SetupTests.SetupScript("-b", @"
        INPUT('IN', 5, 'some.txt zz')
end");
        Assert.AreEqual(116, input.ErrorCodeHistory[0]);
    }
}
