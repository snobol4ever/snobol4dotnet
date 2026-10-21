using Test.TestLexer;

namespace Test.Miscellaneous;

/// <summary>
/// INPUT and OUTPUT need a channel to open a file on and a file for a channel nothing is open on, and ignore what they are given after
/// the file specification, as SPITBOL's do: OUTPUT(.F, 10) and OUTPUT(.F, 10, , 'x') are error 160, OUTPUT(.F, , 'o1.txt') too, and
/// the same of INPUT is 116. With no channel and no file a variable is standard output. The expected text is &amp;ERRTYPE after each
/// call, as sbl -b answers the same program; the calls are the cases, not their numbers.
/// </summary>
[DoNotParallelize]
[TestClass]
public class FileArguments
{
    private static void Check(string call, string expected)
    {
        var s = "        &ERRLIMIT = 5\n        " + call + "\n        F = 'hello'\n        OUTPUT = 'err ' &ERRTYPE\nEND";
        Assert.AreEqual(expected, SetupTests.RunWithInput(s), call);
    }

    [TestMethod]
    public void TEST_FileArguments_output_with_nothing_is_standard_output()
    {
        Check("OUTPUT(.F)", "hello\nerr 0");
    }

    [TestMethod]
    public void TEST_FileArguments_output_with_null_arguments_is_standard_output()
    {
        Check("OUTPUT(.F,,)", "hello\nerr 0");
    }

    [TestMethod]
    public void TEST_FileArguments_output_of_a_channel_nothing_is_open_on_is_160()
    {
        Check("OUTPUT(.F,10)", "err 160");
    }

    [TestMethod]
    public void TEST_FileArguments_output_of_a_channel_and_a_null_file_is_160()
    {
        Check("OUTPUT(.F,10,)", "err 160");
    }

    [TestMethod]
    public void TEST_FileArguments_an_argument_after_the_file_is_ignored_so_this_is_160()
    {
        Check("OUTPUT(.F,10,,'x')", "err 160");
    }

    [TestMethod]
    public void TEST_FileArguments_output_of_a_file_without_a_channel_is_160()
    {
        Check("OUTPUT(.F,,'o1.txt')", "err 160");
    }

    [TestMethod]
    public void TEST_FileArguments_output_of_a_named_channel_nothing_is_open_on_is_160()
    {
        Check("OUTPUT(.F,'abc')", "err 160");
    }

    [TestMethod]
    public void TEST_FileArguments_input_of_a_channel_nothing_is_open_on_is_116()
    {
        Check("INPUT(.F,10)", "err 116");
    }

    [TestMethod]
    public void TEST_FileArguments_input_of_a_file_without_a_channel_is_116()
    {
        Check("INPUT(.F,,'test1.txt')", "err 116");
    }

    [TestMethod]
    public void TEST_FileArguments_input_of_a_channel_and_a_null_file_is_116()
    {
        Check("INPUT(.F,10,'')", "err 116");
    }
}
