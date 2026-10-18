using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text;

namespace Snobol4.Common;

/// <summary>
/// net-save-dll track: persist a compiled threaded-mode program to a .dll
/// and reload it later without the original source file.
///
/// DLL format: a PersistedAssemblyBuilder assembly containing a sentinel
/// type <c>Snobol4ThreadedDll</c> with three public static literal string fields:
///   __source__   — SNOBOL4 source text (SourceLine.Text lines joined with \n)
///   __filename__ — logical source filename (for error messages)
///   __options__  — semicolon-separated compiler flags (e.g. "CaseFolding=true")
///
/// On load (RunDll): detect sentinel → extract fields → ReadCodeInString →
/// full lex/parse/emit/compile pipeline → ExecuteLoop(0).
/// MsilDelegates are re-JIT'd transparently; no Instruction[] serialization needed.
/// </summary>
public partial class Builder
{
    internal const string SentinelTypeName  = "Snobol4ThreadedDll";
    internal const string FieldSource       = "__source__";
    internal const string FieldFilename     = "__filename__";
    internal const string FieldOptions      = "__options__";

    // -----------------------------------------------------------------------
    // net-save-dll-1: SaveDll — write the threaded DLL to disk
    // -----------------------------------------------------------------------

    /// <summary>
    /// Persists the compiled program as a threaded DLL.
    /// Called from BuildMain() after PopulateMainMetadata() when WriteDll is set.
    /// Output path: FilesToCompile[0] with extension replaced by .dll.
    /// </summary>
    internal void SaveDll()
    {
        if (FilesToCompile.Count == 0 || string.IsNullOrWhiteSpace(FilesToCompile[0]))
            return;

        var outputPath = Path.ChangeExtension(
            Path.GetFullPath(FilesToCompile[0]), ".dll");

        // Reconstruct source text from the already-parsed source lines.
        // SourceLine.Text preserves leading whitespace (column-1 = label field),
        // so joining with \n faithfully reconstructs the original format.
        var sourceText = string.Join("\n",
            Code.SourceLines.Select(l => l.Text));

        // Encode compiler options that affect re-compilation behaviour.
        var options = BuildOptionsToString();

        // Logical filename — used by RunDll to restore FilesToCompile[0]
        var filename = FilesToCompile[0];

        try
        {
            EmitThreadedDll(outputPath, sourceText, filename, options);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"snobol4: warning — could not write DLL '{outputPath}': {ex.Message}");
        }
    }

    /// <summary>
    /// Emits the PersistedAssemblyBuilder DLL to <paramref name="outputPath"/>.
    /// Separated from SaveDll() so tests can call it directly.
    /// </summary>
    private static void EmitThreadedDll(
        string outputPath, string sourceText, string filename, string options)
    {
        var asmName   = new AssemblyName(
            Path.GetFileNameWithoutExtension(outputPath));
        var pab = new PersistedAssemblyBuilder(
            asmName, typeof(object).Assembly);
        var mod = pab.DefineDynamicModule(asmName.Name!);

        // ── Sentinel type ────────────────────────────────────────────────
        var tb = mod.DefineType(
            SentinelTypeName,
            TypeAttributes.Public | TypeAttributes.Sealed |
            TypeAttributes.Class | TypeAttributes.BeforeFieldInit);

        DefineStringLiteral(tb, FieldSource,   sourceText);
        DefineStringLiteral(tb, FieldFilename, filename);
        DefineStringLiteral(tb, FieldOptions,  options);

        // Stub Run(object x) : int — RunDll detects the sentinel type and
        // bypasses this method entirely (re-compiles from __source__ instead).
        var run = tb.DefineMethod(
            "Run",
            MethodAttributes.Public | MethodAttributes.HideBySig,
            typeof(int),
            [typeof(object)]);
        var il = run.GetILGenerator();
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ret);

        tb.CreateType();

        // ── Save to disk ─────────────────────────────────────────────────
        using var fs = new FileStream(
            outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        pab.Save(fs);
    }

    private static void DefineStringLiteral(TypeBuilder tb, string name, string value)
    {
        var f = tb.DefineField(
            name,
            typeof(string),
            FieldAttributes.Public | FieldAttributes.Static |
            FieldAttributes.Literal | FieldAttributes.HasDefault);
        f.SetConstant(value);
    }

    // -----------------------------------------------------------------------
    // net-save-dll-2: RunDll (threaded detection) — implemented here so the
    // full sentinel detection + re-compile path lives alongside SaveDll.
    // Builder.cs calls this from RunDll() once it detects a .dll argument.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Loads a threaded DLL written by SaveDll() and executes it.
    /// Returns true if the DLL was a threaded (sentinel) DLL and was handled;
    /// returns false if it's an unrecognised format (caller should report error).
    /// </summary>
    internal bool TryRunThreadedDll(string dllPath)
    {
        var loadContext = CreateTrackedLoadContext("RunThreadedDll");
        Assembly dll;
        try
        {
            dll = loadContext.LoadFromAssemblyPath(dllPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"snobol4: error loading '{dllPath}': {ex.Message}");
            return false;
        }

        var sentinel = dll.GetType(SentinelTypeName);
        if (sentinel == null)
            return false;   // Not a threaded DLL — caller handles legacy path

        // Extract embedded fields
        var sourceText = ReadLiteralField(sentinel, FieldSource)   ?? string.Empty;
        var filename   = ReadLiteralField(sentinel, FieldFilename) ?? dllPath;
        var optionsStr = ReadLiteralField(sentinel, FieldOptions)  ?? string.Empty;

        // We no longer need the load context — source is embedded
        loadContext.Unload();

        // Restore compiler options
        ApplyOptionsFromString(optionsStr);

        // Set up FilesToCompile so lex/parse/error reporting work correctly
        FilesToCompile.Clear();
        FilesToCompile.Add(filename);

        // Feed embedded source through the full compile pipeline.
        // ReadCodeInString preserves column structure (label field in col 1).
        Execute = new Executive(this);
        try
        {
            GetNameSpaceAndClassName(CompileTarget.PROGRAM);
            Code.ReadCodeInString(sourceText, filename);
            Lex(this);
            Parse(this);
            ResolveSlots();

            var tc = new ThreadedCodeCompiler(this);
            EmitMsilForAllStatements();
            Execute.Thread = tc.Compile();
            CompileStarFunctions(tc);
            PopulateMainMetadata();
            ComputeThreadIsMsilOnly();

            Execute._timerExecute.Restart();
            Execute.ExecuteLoop(0);
        }
        catch (CompilerException) { }
        catch (Exception e) { ReportProgrammingError(e); }
        finally
        {
            Execute?.PrintExecutionStatistics();
            Execute?.DisplayVariableValues();
            Execute?.CloseAllStreams();
            ListFileWriter?.Close();
            ListFileWriter = null;
        }

        return true;
    }

    private static string? ReadLiteralField(Type type, string fieldName)
    {
        var f = type.GetField(
            fieldName, BindingFlags.Public | BindingFlags.Static);
        return f?.GetRawConstantValue() as string;
    }

    // -----------------------------------------------------------------------
    // Options serialisation helpers
    // -----------------------------------------------------------------------

    private string BuildOptionsToString()
    {
        // Only persist options that affect re-compilation (not runtime display flags).
        var sb = new StringBuilder();
        Append(sb, "CaseFolding",    BuildOptions.CaseFolding);
        Append(sb, "DebugSymbols",   BuildOptions.GenerateDebugSymbols);
        return sb.ToString();

        static void Append(StringBuilder b, string key, bool val)
        {
            if (b.Length > 0) b.Append(';');
            b.Append(key).Append('=').Append(val ? "true" : "false");
        }
    }

    private void ApplyOptionsFromString(string options)
    {
        foreach (var part in options.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq < 0) continue;
            var key = part[..eq].Trim();
            var val = part[(eq + 1)..].Trim().Equals(
                "true", StringComparison.OrdinalIgnoreCase);
            switch (key)
            {
                case "CaseFolding":  BuildOptions.CaseFolding          = val; break;
                case "DebugSymbols": BuildOptions.GenerateDebugSymbols = val; break;
            }
        }
    }
}
