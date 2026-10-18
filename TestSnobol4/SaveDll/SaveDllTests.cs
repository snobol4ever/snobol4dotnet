using Snobol4.Common;
using System.Text;
using Test.TestLexer;

namespace Test.SaveDll;

/// <summary>
/// Tests for net-save-dll track (sprints 1-3).
/// Verifies that -w produces a .dll and that running the .dll
/// produces identical output to running the .sno directly.
/// </summary>
[DoNotParallelize]
[TestClass]
public class SaveDllTests
{
    private static readonly object s_lock = new();
    private static readonly string s_tempDir =
        Path.Combine(Path.GetTempPath(), "snobol4_savedll_tests");

    [ClassInitialize]
    public static void ClassInit(TestContext _)
        => Directory.CreateDirectory(s_tempDir);

    [ClassCleanup]
    public static void ClassCleanup()
    {
        try { Directory.Delete(s_tempDir, recursive: true); } catch { }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Runs a SNOBOL4 script with optional flags; captures Console.Error output.
    /// DOTNET sends OUTPUT to Console.Error.
    /// </summary>
    private static string RunScript(string script, string flags = "-b")
    {
        lock (s_lock)
        {
            var old = Console.Out;
            using var ms  = new MemoryStream();
            using var sw  = new StreamWriter(ms, Encoding.UTF8) { AutoFlush = true };
            Console.SetOut(sw);
            try   { SetupTests.SetupScript(flags, script); }
            finally { Console.SetOut(old); }
            ms.Position = 0;
            return new StreamReader(ms, Encoding.UTF8).ReadToEnd();
        }
    }

    /// <summary>
    /// Saves a script as a DLL via WriteDll, returns the DLL path.
    /// The source is written to a temp .sno file so FilesToCompile[0] is valid.
    /// </summary>
    private static string SaveScriptAsDll(string script, string baseName)
    {
        var snoPath = Path.Combine(s_tempDir, baseName + ".sno");
        var dllPath = Path.Combine(s_tempDir, baseName + ".dll");

        // Write source to disk so ReadTestScript has a valid path
        File.WriteAllText(snoPath, script, Encoding.UTF8);

        lock (s_lock)
        {
            // ParseCommandLine with -b -w and the .sno path
            var builder = new Builder();
            builder.ParseCommandLine(["-b", "-w", snoPath]);
            // Override FilesToCompile so output DLL lands in our temp dir
            builder.FilesToCompile.Clear();
            builder.FilesToCompile.Add(snoPath);
            builder.Code.ReadTestScript(
                new MemoryStream(Encoding.UTF8.GetBytes(script)));
            builder.BuildMain();
        }

        return dllPath;
    }

    /// <summary>
    /// Loads a threaded DLL and captures its Console.Error output.
    /// </summary>
    private static string RunDll(string dllPath)
    {
        lock (s_lock)
        {
            var old = Console.Out;
            using var ms  = new MemoryStream();
            using var sw  = new StreamWriter(ms, Encoding.UTF8) { AutoFlush = true };
            Console.SetOut(sw);
            try
            {
                var builder = new Builder();
                builder.ParseCommandLine(["-b"]);
                builder.RunDll(dllPath);
            }
            finally { Console.SetOut(old); }
            ms.Position = 0;
            return new StreamReader(ms, Encoding.UTF8).ReadToEnd();
        }
    }

    // -----------------------------------------------------------------------
    // net-save-dll-3 tests
    // -----------------------------------------------------------------------

    /// <summary>
    /// WriteDll_HelloWorld: -w on a simple OUTPUT program produces a .dll file
    /// that contains the Snobol4ThreadedDll sentinel type.
    /// </summary>
    [TestMethod]
    public void WriteDll_HelloWorld_DllExists()
    {
        var script = "  OUTPUT = 'hello world'"+ Environment.NewLine + "  END"+ Environment.NewLine + "";
        var dllPath = SaveScriptAsDll(script, "hello_world");

        Assert.IsTrue(File.Exists(dllPath),
            $"Expected DLL at {dllPath}");

        // Verify sentinel type is present
        var alc = new System.Runtime.Loader.AssemblyLoadContext(
            "verify_sentinel", isCollectible: true);
        try
        {
            var asm      = alc.LoadFromAssemblyPath(dllPath);
            var sentinel = asm.GetType(Builder.SentinelTypeName);
            Assert.IsNotNull(sentinel,
                $"Expected type '{Builder.SentinelTypeName}' in DLL");
            var srcField = sentinel.GetField(Builder.FieldSource,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(srcField, "__source__ field missing");
            var stored = srcField.GetRawConstantValue() as string;
            Assert.IsTrue(stored?.Contains("hello world") == true,
                $"__source__ should contain 'hello world', got: {stored}");
        }
        finally { alc.Unload(); }
    }

    /// <summary>
    /// WriteDll_HelloWorld_RunProducesOutput: RunDll on the saved DLL
    /// produces the expected output line.
    /// </summary>
    [TestMethod]
    public void WriteDll_HelloWorld_RunProducesOutput()
    {
        var script  = "  OUTPUT = 'hello world'"+ Environment.NewLine + "  END"+ Environment.NewLine + "";
        var dllPath = SaveScriptAsDll(script, "hello_world_run");

        Assert.IsTrue(File.Exists(dllPath), $"DLL not found: {dllPath}");

        var output = RunDll(dllPath);
        Assert.IsTrue(output.Contains("hello world"),
            $"Expected 'hello world' in output, got: {output}");
    }

    /// <summary>
    /// WriteDll_OutputMatchesDirect: output from RunDll matches output
    /// from running the script directly — the DLL is a faithful round-trip.
    /// </summary>
    [TestMethod]
    public void WriteDll_OutputMatchesDirect()
    {
        // Script uses variables and arithmetic — exercises the full compile path
        var script = string.Join(""+ Environment.NewLine + "",
            "  N = 6",
            "  RESULT = N * 7",
            "  OUTPUT = 'answer=' RESULT",
            "  END",
            "");

        // Direct run
        var direct = RunScript(script);

        // DLL round-trip
        var dllPath = SaveScriptAsDll(script, "match_direct");
        Assert.IsTrue(File.Exists(dllPath), $"DLL not found: {dllPath}");
        var viadll = RunDll(dllPath);

        // Both should contain the answer
        Assert.IsTrue(direct.Contains("answer=42"),
            $"Direct run: expected 'answer=42', got: {direct}");
        Assert.IsTrue(viadll.Contains("answer=42"),
            $"DLL run: expected 'answer=42', got: {viadll}");
    }
}
