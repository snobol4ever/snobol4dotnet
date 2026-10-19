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

    /// <summary>Absolute path to FSharpOptionLibrary.dll (plain F# — no IExternalLibrary; exercises option/DU coercion).</summary>
    public static string FSharpOptionLibraryPath => LibraryPath("FSharpOptionLibrary", "FSharpOptionLibrary.dll");

    /// <summary>Absolute path to VbLibrary.dll (VB.NET — proves reflect path works from VB.NET).</summary>
    public static string VbLibraryPath => LibraryPath("VbLibrary", "VbLibrary.dll");

    /// <summary>Absolute path to ReflectLibrary.dll (plain C# — no IExternalLibrary).</summary>
    public static string ReflectLibraryPath => LibraryPath("ReflectLibrary", "ReflectLibrary.dll");

    /// <summary>Absolute path to ObjectLifecycleLibrary.dll.</summary>
    public static string ObjectLifecycleLibraryPath => LibraryPath("ObjectLifecycleLibrary", "ObjectLifecycleLibrary.dll");

    public static string NoconvDotNetLibraryPath => LibraryPath("NoconvDotNetLibrary", "NoconvDotNetLibrary.dll");

    public static string NoconvCLibPath
    {
        get
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            if(IsWindows)
                return Path.Combine(dir, "CustomFunction", "SpitbolNoconvLib", "libspitbol_noconv.dll");
            if (IsLinux)
                return Path.Combine(dir, "CustomFunction", "SpitbolNoconvLib", "libspitbol_noconv.so");
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
    }

    public static string XnRtLibPath
    {
        get
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            if (IsWindows)
                return Path.Combine(dir, "CustomFunction", "SpitbolXnLib", "libsnobol4_rt.dll");
            if (IsLinux)
                return Path.Combine(dir, "CustomFunction", "SpitbolXnLib", "libsnobol4_rt.so");
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
    }

    public static string XnCLibPath
    {
        get
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            if (IsWindows)
                return Path.Combine(dir, "CustomFunction", "SpitbolXnLib", "libspitbol_xn.dll");
            if (IsLinux)
                return Path.Combine(dir, "CustomFunction", "SpitbolXnLib", "libspitbol_xn.so");
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
    }

    public static string CreateCLibPath
    {
        get
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            for (var i = 0; i < 4; i++)
                dir = Path.GetDirectoryName(dir) ?? dir;
            if(IsWindows)
                return Path.Combine(dir, "CustomFunction", "SpitbolCreateLib", "libspitbol_create.dll");
            if(IsLinux)
                return Path.Combine(dir, "CustomFunction", "SpitbolCreateLib", "libspitbol_create.so");
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
    }

    private static string LibraryPath(string project, string dll)
    {
        // AppDomain.BaseDirectory = …/TestSnobol4/bin/<Config>/net10.0/
        // Walk up: net10.0 → <Config> → bin → TestSnobol4 → solution root
        var baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var config  = DetectConfiguration(baseDir);
        var dir     = baseDir;
        for (var i = 0; i < 4; i++)
            dir = Path.GetDirectoryName(dir) ?? dir;
        var projectDir = string.IsNullOrEmpty(project)
            ? Path.Combine(dir, "CustomFunction", "bin", config, "net10.0")
            : Path.Combine(dir, "CustomFunction", project, "bin", config, "net10.0");
        return Path.Combine(projectDir, dll);
    }

    /// <summary>
    /// Infer the build configuration (Debug / Release / etc.) from the
    /// test assembly's output path so LibraryPath finds the matching DLLs.
    /// </summary>
    private static string DetectConfiguration(string baseDir)
    {
        var sep  = Path.DirectorySeparatorChar;
        var path = baseDir + sep;
        foreach (var config in new[] { "Release", "Debug" })
            if (path.Contains(sep + config + sep, System.StringComparison.OrdinalIgnoreCase))
                return config;
        return "Debug";
    }

    /// <summary>
    /// Runs a SNOBOL4 script with optional stdin input and returns all OUTPUT lines
    /// as a single string (lines joined by newline, trailing whitespace trimmed).
    ///
    /// Input is fed via <see cref="Executive.ReadLineDelegate"/> — no Console.SetIn
    /// needed. The delegate is always reset to null after the run so parallel tests
    /// are not affected.
    ///
    /// Used by corpus ref tests that compare against .ref oracle files.
    /// </summary>
    internal static string RunWithInput(string script, string? inputText = null) =>
        RunWithInputDirectives("-b", script, inputText);

    // RunWithInput with the command-line switches given (e.g. "-b -o=dir/prog")
    internal static string RunWithInputDirectives(string directives, string script, string? inputText = null)
    {
        var inputLines = inputText?.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();
        var inputIdx   = 0;
        Executive.ReadLineDelegate = inputLines is null ? null
            : () => inputIdx < inputLines.Length ? inputLines[inputIdx++] : null;

        var old = Console.Out;
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms) { AutoFlush = true };
        Console.SetOut(sw);
        try   { SetupScript(directives, script); }
        finally
        {
            Console.SetOut(old);
            Executive.ReadLineDelegate = null;  // always reset — static field, tests run in-process
        }

        ms.Position = 0;
        using var sr = new StreamReader(ms);
        return sr.ReadToEnd().TrimEnd();
    }
}