using Snobol4.Common;
using Test.TestLexer;

namespace Test.TestSpitbolSwitches;

/// <summary>
/// Tests for all SPITBOL command-line switches.
/// Covers: -d -e -g -i -m -p -s -t -y -z  and channel -N=file association.
/// Previously-existing switches (-a -b -c -cs -f -F -h -k -l -n -o -r -u -v -w -x -?)
/// are tested by their respective existing test classes; this file adds the new ones.
/// M-NET-SPITBOL-SWITCHES milestone: all tests here must pass.
/// </summary>
[TestClass]
public class SpitbolSwitchTests
{
    // ── Helper ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Parse only the switch string (no script execution).
    /// Appends a dummy filename so ParseCommandLine exits command mode.
    /// </summary>
    private static Builder Parse(string switches)
    {
        var args = switches.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Append("dummy.sno")
                           .ToArray();
        var b = new Builder();
        b.ParseCommandLine(args);
        return b;
    }

    // ── -e  ErrorsToStdout ───────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_e_SetsErrorsToStdout()
    {
        var b = Parse("-b -e");
        Assert.IsTrue(b.BuildOptions.ErrorsToStdout, "-e should set ErrorsToStdout");
    }

    [TestMethod]
    public void TEST_Switch_e_DefaultIsFalse()
    {
        var b = Parse("-b");
        Assert.IsFalse(b.BuildOptions.ErrorsToStdout, "ErrorsToStdout should default to false");
    }

    // ── -gN  LinesPerPage ────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_g_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(60, b.BuildOptions.LinesPerPage, "Default LinesPerPage should be 60");
    }

    [TestMethod]
    public void TEST_Switch_g_Bare()
    {
        var b = Parse("-b -g80");
        Assert.AreEqual(80, b.BuildOptions.LinesPerPage);
    }

    [TestMethod]
    public void TEST_Switch_g_WithEquals()
    {
        var b = Parse("-b -g=55");
        Assert.AreEqual(55, b.BuildOptions.LinesPerPage);
    }

    [TestMethod]
    public void TEST_Switch_g_WithColon()
    {
        var b = Parse("-b -g:40");
        Assert.AreEqual(40, b.BuildOptions.LinesPerPage);
    }

    // ── -tN  PageWidth ───────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_t_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(120, b.BuildOptions.PageWidth, "Default PageWidth should be 120");
    }

    [TestMethod]
    public void TEST_Switch_t_Bare()
    {
        var b = Parse("-b -t80");
        Assert.AreEqual(80, b.BuildOptions.PageWidth);
    }

    [TestMethod]
    public void TEST_Switch_t_WithEquals()
    {
        var b = Parse("-b -t=132");
        Assert.AreEqual(132, b.BuildOptions.PageWidth);
    }

    // ── -p  PrinterListing ───────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_p_SetsPrinterListingAndShowListing()
    {
        var b = Parse("-b -p");
        Assert.IsTrue(b.BuildOptions.PrinterListing, "-p should set PrinterListing");
        Assert.IsTrue(b.BuildOptions.ShowListing,    "-p should imply ShowListing");
    }

    [TestMethod]
    public void TEST_Switch_p_DefaultIsFalse()
    {
        var b = Parse("-b");
        Assert.IsFalse(b.BuildOptions.PrinterListing);
    }

    // ── -z  FormFeedListing ──────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_z_SetsFormFeedAndShowListing()
    {
        var b = Parse("-b -z");
        Assert.IsTrue(b.BuildOptions.FormFeedListing, "-z should set FormFeedListing");
        Assert.IsTrue(b.BuildOptions.ShowListing,     "-z should imply ShowListing");
    }

    [TestMethod]
    public void TEST_Switch_z_DefaultIsFalse()
    {
        var b = Parse("-b");
        Assert.IsFalse(b.BuildOptions.FormFeedListing);
    }

    // ── -dN  HeapMaxBytes ────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_d_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(64L * 1024 * 1024, b.BuildOptions.HeapMaxBytes,
            "Default HeapMaxBytes should be 64m");
    }

    [TestMethod]
    public void TEST_Switch_d_MegaSuffix()
    {
        var b = Parse("-b -d128m");
        Assert.AreEqual(128L * 1024 * 1024, b.BuildOptions.HeapMaxBytes);
    }

    [TestMethod]
    public void TEST_Switch_d_KiloSuffix()
    {
        var b = Parse("-b -d512k");
        Assert.AreEqual(512L * 1024, b.BuildOptions.HeapMaxBytes);
    }

    [TestMethod]
    public void TEST_Switch_d_Plain()
    {
        var b = Parse("-b -d1000000");
        Assert.AreEqual(1_000_000L, b.BuildOptions.HeapMaxBytes);
    }

    // ── -iN  HeapIncrementBytes ──────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_i_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(128L * 1024, b.BuildOptions.HeapIncrementBytes,
            "Default HeapIncrementBytes should be 128k");
    }

    [TestMethod]
    public void TEST_Switch_i_KiloSuffix()
    {
        var b = Parse("-b -i256k");
        Assert.AreEqual(256L * 1024, b.BuildOptions.HeapIncrementBytes);
    }

    [TestMethod]
    public void TEST_Switch_i_MegaSuffix()
    {
        var b = Parse("-b -i1m");
        Assert.AreEqual(1L * 1024 * 1024, b.BuildOptions.HeapIncrementBytes);
    }

    // ── -mN  MaxObjectBytes / &MAXLNGTH ──────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_m_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(4L * 1024 * 1024, b.BuildOptions.MaxObjectBytes,
            "Default MaxObjectBytes should be 4m");
    }

    [TestMethod]
    public void TEST_Switch_m_MegaSuffix()
    {
        var b = Parse("-b -m8m");
        Assert.AreEqual(8L * 1024 * 1024, b.BuildOptions.MaxObjectBytes);
    }

    [TestMethod]
    public void TEST_Switch_m_KiloSuffix()
    {
        var b = Parse("-b -m9000");
        Assert.AreEqual(9000L, b.BuildOptions.MaxObjectBytes);
    }

    [TestMethod]
    public void TEST_Switch_m_WiresAmpMaxLength()
    {
        // -m must seed &MAXLNGTH at runtime startup
        const string script = """
                OUTPUT = &MAXLNGTH
            END
            """;
        var b = SetupTests.SetupScript("-b -m2m", script);
        Assert.AreEqual(0, b.ErrorCodeHistory.Count);
        var val = b.Execute!.AmpMaxLength;
        Assert.AreEqual(2L * 1024 * 1024, val,
            "&MAXLNGTH should equal the -m value at program start");
    }

    // ── -sN  StackSizeBytes ──────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_s_Default()
    {
        var b = Parse("-b");
        Assert.AreEqual(32 * 1024, b.BuildOptions.StackSizeBytes,
            "Default StackSizeBytes should be 32k");
    }

    [TestMethod]
    public void TEST_Switch_s_KiloSuffix()
    {
        var b = Parse("-b -s64k");
        Assert.AreEqual(64 * 1024, b.BuildOptions.StackSizeBytes);
    }

    [TestMethod]
    public void TEST_Switch_s_Plain()
    {
        var b = Parse("-b -s65536");
        Assert.AreEqual(65536, b.BuildOptions.StackSizeBytes);
    }

    // ── -y  WriteSpx ─────────────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_y_SetsWriteSpx()
    {
        var b = Parse("-b -y");
        Assert.IsTrue(b.BuildOptions.WriteSpx, "-y should set WriteSpx");
    }

    [TestMethod]
    public void TEST_Switch_y_DefaultIsFalse()
    {
        var b = Parse("-b");
        Assert.IsFalse(b.BuildOptions.WriteSpx);
    }

    // ── -N=file  ChannelFiles ────────────────────────────────────────────────

    [TestMethod]
    public void TEST_Switch_Channel_AssociatesFile()
    {
        var b = Parse("-b -23=infile.dat");
        Assert.IsTrue(b.BuildOptions.ChannelFiles.ContainsKey(23));
        Assert.AreEqual("infile.dat", b.BuildOptions.ChannelFiles[23]);
    }

    [TestMethod]
    public void TEST_Switch_Channel_WithOptions()
    {
        var b = Parse("-b -5=data.txt[-r10]");
        Assert.IsTrue(b.BuildOptions.ChannelFiles.ContainsKey(5));
        Assert.AreEqual("data.txt[-r10]", b.BuildOptions.ChannelFiles[5]);
    }

    [TestMethod]
    public void TEST_Switch_Channel_MultipleChannels()
    {
        var b = Parse("-b -1=in.dat -2=out.dat");
        Assert.AreEqual(2, b.BuildOptions.ChannelFiles.Count);
        Assert.AreEqual("in.dat",  b.BuildOptions.ChannelFiles[1]);
        Assert.AreEqual("out.dat", b.BuildOptions.ChannelFiles[2]);
    }

    [TestMethod]
    public void TEST_Switch_Channel_DefaultIsEmpty()
    {
        var b = Parse("-b");
        Assert.AreEqual(0, b.BuildOptions.ChannelFiles.Count);
    }

    [TestMethod]
    public void TEST_Switch_Channel_ColonSeparator()
    {
        var b = Parse("-b -7:myfile.sno");
        Assert.IsTrue(b.BuildOptions.ChannelFiles.ContainsKey(7));
        Assert.AreEqual("myfile.sno", b.BuildOptions.ChannelFiles[7]);
    }

    // ── k/m suffix parser edge cases ─────────────────────────────────────────

    [TestMethod]
    public void TEST_KmSuffix_UppercaseK()
    {
        var b = Parse("-b -s32K");
        Assert.AreEqual(32 * 1024, b.BuildOptions.StackSizeBytes,
            "Uppercase K suffix should be accepted");
    }

    [TestMethod]
    public void TEST_KmSuffix_UppercaseM()
    {
        var b = Parse("-b -m4M");
        Assert.AreEqual(4L * 1024 * 1024, b.BuildOptions.MaxObjectBytes,
            "Uppercase M suffix should be accepted");
    }

    // ── Combination: -a still works correctly ────────────────────────────────

    [TestMethod]
    public void TEST_Switch_a_ImpliesLCX()
    {
        var b = Parse("-a");
        Assert.IsTrue(b.BuildOptions.ShowListing,             "-a should imply -l");
        Assert.IsTrue(b.BuildOptions.ShowCompilerStatistics,  "-a should imply -c");
        Assert.IsTrue(b.BuildOptions.ShowExecutionStatistics, "-a should imply -x");
    }
}
