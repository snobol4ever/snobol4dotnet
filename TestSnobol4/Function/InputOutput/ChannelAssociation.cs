using Test.TestLexer;

namespace Test.InputOutput;

/// <summary>
/// A variable given a channel with INPUT or OUTPUT is associated with that channel whatever it was associated with before, as in
/// SPITBOL (its old association of the kind is dropped); a channel open for output cannot be given to INPUT (error 117) nor one open
/// for input to OUTPUT (error 161). The console refused the first as "channel in use" (290 and 289) and allowed the second. The
/// expectations are what sbl -bf printed for the same programs.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ChannelAssociation
{
    private static string Run(string body)
    {
        var dir = Path.Combine(Path.GetTempPath(), "channels_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            File.WriteAllText(Path.Combine(dir, "in1.txt"), "line1\nline2\n");
            File.WriteAllText(Path.Combine(dir, "in2.txt"), "other\n");
            var program = $"        &ERRLIMIT = 10\n{body.Replace("DIR", dir.Replace("\\", "/"))}END\n";
            return SetupTests.RunWithInput(program);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void TEST_ChannelAssociation_a_variable_can_move_to_another_output_channel()
    {
        Assert.AreEqual("all ok", Run(
            "        OUTPUT(.A,5,'DIR/o1.txt')    :F(E1)\n        OUTPUT(.B,6,'DIR/o2.txt')    :F(E2)\n" +
            "        OUTPUT(.A,6)                 :F(E3)\n        OUTPUT(.A,5)                 :F(E4)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n" +
            "E3      OUTPUT = 'e3 ' &ERRTYPE :(END)\nE4      OUTPUT = 'e4 ' &ERRTYPE :(END)\n"));
    }

    [TestMethod]
    public void TEST_ChannelAssociation_a_variable_can_move_to_another_input_channel()
    {
        Assert.AreEqual("all ok", Run(
            "        INPUT(.A,5,'DIR/in1.txt')    :F(E1)\n        INPUT(.B,6,'DIR/in2.txt')    :F(E2)\n" +
            "        INPUT(.A,6)                  :F(E3)\n        INPUT(.A,5)                  :F(E4)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n" +
            "E3      OUTPUT = 'e3 ' &ERRTYPE :(END)\nE4      OUTPUT = 'e4 ' &ERRTYPE :(END)\n"));
    }

    [TestMethod]
    public void TEST_ChannelAssociation_an_input_channel_cannot_be_written()
    {
        Assert.AreEqual("e2 161", Run(
            "        INPUT(.A,5,'DIR/in1.txt')    :F(E1)\n        OUTPUT(.B,5)                 :F(E2)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n"));
    }

    [TestMethod]
    public void TEST_ChannelAssociation_an_output_channel_cannot_be_read()
    {
        Assert.AreEqual("e2 117", Run(
            "        OUTPUT(.A,5,'DIR/o1.txt')    :F(E1)\n        INPUT(.B,5)                  :F(E2)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n"));
    }

    [TestMethod]
    public void TEST_ChannelAssociation_a_file_on_a_channel_in_use_is_still_289_and_290()
    {
        Assert.AreEqual("e2 290", Run(
            "        OUTPUT(.A,5,'DIR/o1.txt')    :F(E1)\n        OUTPUT(.B,5,'DIR/o2.txt')    :F(E2)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n"));
        Assert.AreEqual("e2 289", Run(
            "        INPUT(.A,5,'DIR/in1.txt')    :F(E1)\n        INPUT(.B,5,'DIR/in2.txt')    :F(E2)\n" +
            "        OUTPUT = 'all ok'            :(END)\nE1      OUTPUT = 'e1 ' &ERRTYPE :(END)\nE2      OUTPUT = 'e2 ' &ERRTYPE :(END)\n"));
    }
}
