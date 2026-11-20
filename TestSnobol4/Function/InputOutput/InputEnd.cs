using Snobol4.Common;
using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// At end of input an assignment from INPUT fails and assigns nothing, as in SPITBOL: OUTPUT = INPUT
/// writes no empty line and X = INPUT leaves X as it was.
/// </summary>
[DoNotParallelize]
[TestClass]
public class InputEnd
{

    [TestMethod]
    public void TEST_InputEnd_a_files_byte_order_mark_is_read()
    {
        // sbl -bf reads a file's bytes as they are: a UTF-8 byte-order mark stays part of the first line, so
        // the first line's key differs from the same name later (the dotnet package's 1brc counts station001
        // twice); snobol4dotnet's reader dropped the mark
        var file = Path.Combine(Path.GetTempPath(), "snobom_" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllBytes(file, [0xEF, 0xBB, 0xBF, (byte)'a', (byte)'b', (byte)'\n', (byte)'a', (byte)'b', (byte)'\n']);
        try
        {
            var build = SetupTests.SetupScript("-bf", $"""
                         INPUT(.F, 3, '{file}')
                         X = F
                         Y = F
                         R = DIFFER(X, Y) 'differ'
                 END
                 """);
            Assert.AreEqual(0, build.ErrorCodeHistory.Count);
            Assert.AreEqual("differ", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
            Assert.AreEqual("\uFEFFab", ((StringVar)build.Execute!.IdentifierTable["X"]).Data);
        }
        finally { File.Delete(file); }
    }

    [TestMethod]
    public void TEST_InputEnd_missing_file_fails_and_is_not_created()
    {
        // sbl -bf: INPUT of a file that does not exist fails and creates nothing; snobol4dotnet opened it with
        // OpenOrCreate, made an empty file and succeeded
        var file = Path.Combine(Path.GetTempPath(), "snomissing_" + Guid.NewGuid().ToString("N") + ".txt");
        var build = SetupTests.SetupScript("-bf", $"""
                         INPUT(.X, 7, '{file}')   :F(F)
                         R = 'opened'             :(END)
                 F       R = 'failed'
                 END
                 """);
        Assert.AreEqual("failed", ((StringVar)build.Execute!.IdentifierTable["R"]).Data);
        Assert.IsFalse(File.Exists(file));
    }
}
