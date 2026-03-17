using Snobol4.Common;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Test.TestLexer;

[TestClass]
public class SetupTests
{
    public static string WindowsOutput = @"..\..\..\..\..\TestSnobol4\Output\";
    public static string LinuxOutput = @"../../../../../TestSnobol4/Output/";

    public static string CurrentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
    public static string WindowsDll = CurrentDir.Replace(@"\TestSnobol4\", @"\CustomFunction\") + @"\AreaLibrary.dll";
    public static string LinuxDll = CurrentDir.Replace(@"/TestSnobol4/", @"/CustomFunction/") + @"/AreaLibrary.dll";

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

    /// <summary>Absolute path to AreaLibrary.dll (legacy: lives directly in CustomFunction/, not a subfolder).</summary>
    public static string AreaLibraryPath => LibraryPath("", "AreaLibrary.dll");

    /// <summary>Absolute path to MathLibrary.dll.</summary>
    public static string MathLibraryPath => LibraryPath("MathLibrary", "MathLibrary.dll");

    /// <summary>Absolute path to FSharpLibrary.dll.</summary>
    public static string FSharpLibraryPath => LibraryPath("FSharpLibrary", "FSharpLibrary.dll");

    private static string LibraryPath(string project, string dll)
    {
        // AppDomain.BaseDirectory = …/TestSnobol4/bin/Release/net10.0/
        // Walk up: net10.0 → Release → bin → TestSnobol4 → solution root
        var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        for (var i = 0; i < 4; i++)
            dir = Path.GetDirectoryName(dir) ?? dir;
        var projectDir = string.IsNullOrEmpty(project)
            ? Path.Combine(dir, "CustomFunction", "bin", "Release", "net10.0")
            : Path.Combine(dir, "CustomFunction", project, "bin", "Release", "net10.0");
        return Path.Combine(projectDir, dll);
    }
}