using System.Runtime.InteropServices;
using System.Text;
using Snobol4.Common;

namespace Test.TestLexer;

[TestClass]
public class SetupTests
{
    public static string WindowsOutput = @"C:\Users\jcooper\Documents\Visual Studio 2022\Snobol4.Net\TestSnobol4\Output\";
    public static string LinuxOutput = @"/mnt/c/Users/jcooper/Documents/Visual Studio 2022/Snobol4.Net/TestSnobol4/Output/";

    internal static Builder SetupScript(string directives, string script, bool compileOnly = false)
    {
        // Get array of commands and source files
        var commands = new List<string>(
            directives.Split(" ",
                StringSplitOptions.RemoveEmptyEntries));

        // Get cross-platform test file path
        var testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestSnobol4");
        var testFilePath = Path.Combine(testDirectory, "Test.sno");

        // Ensure directory exists
        Directory.CreateDirectory(testDirectory);

        List<string> files = [testFilePath];

        var args = commands.Concat(files).ToArray();

        Builder builder = new();
        builder.ParseCommandLine(args);
        builder.BuildOptions.UseThreadedExecution = true;
        builder.Code.ReadTestScript(new MemoryStream(Encoding.UTF8.GetBytes(script)));
        if (compileOnly)
            builder.BuildMainCompileOnly();
        else
            builder.BuildMain();
        return builder;
    }

    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOs => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Absolute path to AreaLibrary.dll, built from the CustomFunction project.
    /// Walks up from the test assembly's bin directory to find the solution root.
    /// </summary>
    public static string AreaLibraryPath
    {
        get
        {
            // AppDomain.BaseDirectory = …/TestSnobol4/bin/Release/net10.0/
            // Walk up: net10.0 → Release → bin → TestSnobol4 → solution root
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            return Path.Combine(dir, "CustomFunction", "bin", "Release", "net10.0", "AreaLibrary.dll");
        }
    }
}