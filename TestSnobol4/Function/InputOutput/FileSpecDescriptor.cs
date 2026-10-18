using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// OUTPUT with a bracket-only spec naming a descriptor writes to standard output (-f1) or standard
/// error (-f2), and raw mode (-r1) writes no line end, as SPITBOL does for code.sno's '[-f2 -r1]' prompt.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FileSpecDescriptor
{
    [TestMethod]
    public void TEST_FileSpec_descriptor_and_raw_mode()
    {
        // sbl -b: standard output holds rawline\n, standard error holds err with no line end
        var oldError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            var output = SetupTests.RunWithInput(@"        OUTPUT('E', 9, '[-f2 -r1]')
        E = 'err'
        OUTPUT('O', 8, '[-f1 -r1]')
        O = 'raw'
        OUTPUT = 'line'
END");
            Assert.AreEqual("rawline", output);
            Assert.AreEqual("err", error.ToString());
        }
        finally { Console.SetError(oldError); }
    }
}
