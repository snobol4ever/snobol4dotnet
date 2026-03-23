using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

[assembly: InternalsVisibleTo("TestSnobol4")]

namespace Snobol4.Common;

public partial class Builder : IDisposable
{
    #region Members

    private const int _unexpectedExceptionErrorCode = 1000;
    private readonly CompilerTargets _compilerTarget = new();
    private Stopwatch? _timerBuild;
    private readonly List<AssemblyLoadContext> _loadContexts = new(4);
    private bool _disposed;
    private readonly Dictionary<string, string>? _caseFoldCache;

    public BuilderOptions BuildOptions = new();
    public List<int> ErrorCodeHistory = new(16);
    public List<int> ColumnHistory = new(16);
    public List<string> MessageHistory = new(16);

    internal List<string> IncludeList = new(8);
    internal int StatementCount;
    internal string EntryLabel;
    internal StreamWriter? ListFileWriter;
    public List<string> FilesToCompile = new(4);
    internal string[] Arguments = [];
    public SourceCode Code;
    public Executive? Execute;
    internal List<DeferredExpression> ExpressionList = new(32);
    internal List<List<Token>> ParseExpression = new(32);
    internal bool CodeMode;
    internal int RecordedExpressionCount = 0;

    // Threaded execution tables
    internal List<VariableSlot> VariableSlots = new(64);
    internal Dictionary<string, int> VariableSlotIndex = new(64, StringComparer.Ordinal);
    internal List<FunctionSlot> FunctionSlots = new(64);
    internal Dictionary<string, int> FunctionSlotIndex = new(64, StringComparer.Ordinal);
    internal ConstantPool Constants = new();
    internal int[]? StatementInstructionStarts;

    /// <summary>
    /// True when the compiled thread contains only <c>CallMsil</c> and <c>Halt</c>
    /// opcodes — all control flow has been absorbed into delegates.
    /// Set after <see cref="ThreadedCodeCompiler.Compile"/> / <c>AppendCompile</c>.
    /// When true, <see cref="Executive.ThreadedExecuteLoop"/> can skip the full
    /// switch dispatch and spin in a tight CallMsil-only fast path.
    /// </summary>
    internal bool ThreadIsMsilOnly;

    #endregion

    #region Constructor

    public Builder()
    {
        Code = new SourceCode(this);
        EntryLabel = string.Empty;
        if (BuildOptions.CaseFolding)
            _caseFoldCache = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    #endregion

    #region Static helpers (Lex / Parse)

    internal static bool Lex(Builder parent, int startState = 1)
    {
        Lexer lex = new(parent, startState);
        return lex.Lex();
    }

    internal static void Parse(Builder parent)
    {
        Parser parser = new(parent);
        parser.Parse();
    }

    #endregion

    #region Public entry points

    public void BuildMain()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Execute = new Executive(this);
        ApplyStartupOptions(Execute);
        try
        {
            GetNameSpaceAndClassName(CompileTarget.PROGRAM);
            _timerBuild ??= new Stopwatch();
            _timerBuild.Restart();
            Lex(this);
            Parse(this);
            ResolveSlots();

            var tc = new ThreadedCodeCompiler(this);
            EmitMsilForAllStatements();
            Execute.Thread = tc.Compile();
            CompileStarFunctions(tc);
            PopulateMainMetadata();
            ComputeThreadIsMsilOnly();
            if (BuildOptions.WriteDll) SaveDll();
            _timerBuild.Stop();
            PrintCompilationStatistics();
            Execute.StartTimer();
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
        }
    }

    /// <summary>Lex, parse, and compile only — do not execute. For use by compiler structure tests.</summary>
    public void BuildMainCompileOnly()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Execute = new Executive(this);
        ApplyStartupOptions(Execute);
        try
        {
            GetNameSpaceAndClassName(CompileTarget.PROGRAM);
            Lex(this);
            Parse(this);
            ResolveSlots();
            var tc = new ThreadedCodeCompiler(this);
            EmitMsilForAllStatements();
            Execute.Thread = tc.Compile();
            CompileStarFunctions(tc);
            PopulateMainMetadata();
            ComputeThreadIsMsilOnly();
        }
        catch (CompilerException) { }
        catch (Exception e) { ReportProgrammingError(e); }
    }

    internal void BuildEval()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            GetNameSpaceAndClassName(CompileTarget.EVAL);
            if (!Lex(this))
            {
                Code.SourceLines[0].Text = " *('')";
                Lex(this);
            }
            Parse(this);
            ResolveSlots();
            EmitMsilForAllStatements();
            CompileStarFunctions(new ThreadedCodeCompiler(this));
            StatementCount += Code.SourceLines.Count;
        }
        catch (CompilerException) { }
        catch (Exception e) { ReportProgrammingError(e); }
    }

    internal bool BuildCode()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            GetNameSpaceAndClassName(CompileTarget.CODE);
            Lex(this);
            Parse(this);
            var stmtOffset = StatementCount;
            ResolveSlots();
            EmitMsilForAllStatements();
            var tc = new ThreadedCodeCompiler(this);
            Execute!.Thread = tc.AppendCompile(Execute.Thread!, stmtOffset);
            CompileStarFunctions(tc);
            PopulateCodeMetadata(stmtOffset);
            StatementCount += Code.SourceLines.Count;
            ComputeThreadIsMsilOnly();
            return true;
        }
        catch (CompilerException) { }
        catch (Exception e) { ReportProgrammingError(e); }
        return false;
    }

    /// <summary>Load a pre-compiled DLL and run it. Handles both threaded (sentinel) and legacy formats.</summary>
    public void RunDll(string dllFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dllFileName);
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            ClearExceptionHistory();
            // Threaded format: detect Snobol4ThreadedDll sentinel → re-compile from embedded source
            if (TryRunThreadedDll(dllFileName))
                return;
            // Legacy format: Roslyn-generated DLL with a Run(Executive) method
            var loadContext = CreateTrackedLoadContext("RunDll");
            var dll = loadContext.LoadFromAssemblyPath(dllFileName);
            Execute = new Executive(this);
            ApplyStartupOptions(Execute);
            var className = Path.GetFileNameWithoutExtension(dllFileName);
            Execute.Execute(dll, loadContext, $"{className}0.{className}");
        }
        catch (CompilerException) { }
        catch (Exception e) { ReportProgrammingError(e); }
    }

    /// <summary>
    /// Applies BuildOptions that take effect at Executive creation time:
    /// -e (errors to stdout), -m (max object size → &amp;MAXLNGTH).
    /// Called once per Executive instance, immediately after construction.
    /// </summary>
    private void ApplyStartupOptions(Executive exec)
    {
        // -e: redirect Console.Error to Console.Out so errors can be piped
        if (BuildOptions.ErrorsToStdout)
            Console.SetError(Console.Out);

        // -m: seed &MAXLNGTH from the command-line value before any user code runs
        exec.AmpMaxLength = BuildOptions.MaxObjectBytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FoldCase(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!BuildOptions.CaseFolding) return input;
        if (_caseFoldCache != null && _caseFoldCache.TryGetValue(input, out var cached))
            return cached;
        var folded = input.ToUpperInvariant();
        if (_caseFoldCache != null && _caseFoldCache.Count < _unexpectedExceptionErrorCode)
            _caseFoldCache[input] = folded;
        return folded;
    }

    #endregion

    #region Metadata population

    private void PopulateMainMetadata()
    {
        if (Execute == null) return;

        Execute.Parent.BuildOptions.SuppressListingHeader   = BuildOptions.SuppressListingHeader;
        Execute.Parent.BuildOptions.ListFileName            = BuildOptions.ListFileName;
        Execute.Parent.BuildOptions.ShowExecutionStatistics = BuildOptions.ShowExecutionStatistics;
        if (FilesToCompile.Count > 0)
            Execute.Parent.FilesToCompile.Add(FilesToCompile[^1]);

        var stmtNumber = StatementCount;
        foreach (var line in Code.SourceLines)
        {
            if (!line.Compiled)
            {
                var codeCount = 1 + line.LineCountFile - line.BlankLineCount - line.CommentContinuationDirectiveCount;
                var listCount = 1 + line.LineCountFile - line.CommentContinuationDirectiveCount;
                Execute.SourceCode.Add($"{Path.GetFileName(line.PathName)}:{codeCount}/{listCount})\n{line.Text.Replace('\t', ' ')}");
                Execute.SourceFiles.Add(line.PathName);
                Execute.SourceLineNumbers.Add(line.LineCountFile);
                Execute.SourceStatementNumbers.Add(stmtNumber + 1);
            }
            stmtNumber++;
        }

        stmtNumber = StatementCount;
        foreach (var line in Code.SourceLines)
        {
            if (line.Label.Length > 0 && !line.Compiled)
            {
                var lbl = BuildOptions.CaseFolding ? line.Label.ToUpper() : line.Label;
                if (lbl is not "END" and not "end")
                    Execute.LabelTable[lbl] = stmtNumber;
            }
            stmtNumber++;
        }

        foreach (var (key, val) in new (string, int)[]
        {
            ("end",-1), ("return",-2), ("freturn",-3), ("nreturn",-4),
            ("abort",-5), ("continue",-6), ("scontinue",-7),
            ("END",-1), ("RETURN",-2), ("FRETURN",-3), ("NRETURN",-4),
            ("ABORT",-5), ("CONTINUE",-6), ("SCONTINUE",-7),
        })
            Execute.LabelTable[key] = val;

        StatementCount += Code.SourceLines.Count;
    }

    private void PopulateCodeMetadata(int stmtOffset)
    {
        if (Execute == null) return;
        var stmtNumber = stmtOffset;
        foreach (var line in Code.SourceLines)
        {
            if (!line.Compiled)
            {
                var codeCount = 1 + line.LineCountFile - line.BlankLineCount - line.CommentContinuationDirectiveCount;
                var listCount = 1 + line.LineCountFile - line.CommentContinuationDirectiveCount;
                Execute.SourceCode.Add($"{Path.GetFileName(line.PathName)}:{codeCount}/{listCount})\n{line.Text.Replace('\t', ' ')}");
                Execute.SourceFiles.Add(line.PathName);
                Execute.SourceLineNumbers.Add(line.LineCountFile);
                Execute.SourceStatementNumbers.Add(stmtNumber + 1);
                if (line.Label.Length > 0)
                {
                    var folded = BuildOptions.CaseFolding ? line.Label.ToUpper() : line.Label;
                    if (folded is not "END")
                        Execute.LabelTable[folded] = stmtNumber;
                }
            }
            stmtNumber++;
        }
    }

    #endregion

    #region ThreadIsMsilOnly analysis

    /// <summary>
    /// Inspects the compiled thread and sets <see cref="ThreadIsMsilOnly"/> to
    /// <c>true</c> iff every instruction is either <c>CallMsil</c> or <c>Halt</c>.
    /// Called immediately after each compile path finishes building the thread.
    /// </summary>
    private void ComputeThreadIsMsilOnly()
    {
        if (Execute?.Thread == null) { ThreadIsMsilOnly = false; return; }
        foreach (var instr in Execute.Thread)
        {
            if (instr.Op != OpCode.CallMsil && instr.Op != OpCode.Halt)
            {
                ThreadIsMsilOnly = false;
                return;
            }
        }
        ThreadIsMsilOnly = true;
    }

    #endregion

    #region Star-function compilation

    private void CompileStarFunctions(ThreadedCodeCompiler tc)
    {
        if (Execute == null) return;
        for (int i = Execute.StarFunctionList.Count; i < ParseExpression.Count; i++)
        {
            var subThread = tc.CompileSubExpression(ParseExpression[i]);
            Execute.StarFunctionList.Add(x => x.RunExpressionThread(subThread));
        }
        Execute.PreviousStarFunctionCount = Execute.StarFunctionList.Count;
    }

    #endregion

    #region Private helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AssemblyLoadContext CreateTrackedLoadContext(string? name = null)
    {
        var ctx = new AssemblyLoadContext(name, isCollectible: true);
        _loadContexts.Add(ctx);
        return ctx;
    }

    private void GetNameSpaceAndClassName(CompileTarget target)
    {
        if (FilesToCompile.Count == 0 || string.IsNullOrWhiteSpace(FilesToCompile[0]))
            throw new InvalidOperationException("No source files specified for compilation.");

        var baseFileName    = Path.GetFileNameWithoutExtension(FilesToCompile[0]);
        var fileNameBuilder = new StringBuilder(baseFileName);
        string type;

        switch (target)
        {
            case CompileTarget.CODE:
                fileNameBuilder.Append("_CODE").Append(_compilerTarget.CodeNum++);
                type = "CODE";
                break;
            case CompileTarget.EVAL:
                fileNameBuilder.Append("_EVAL").Append(_compilerTarget.EvalNum++);
                type = "EVAL";
                break;
            case CompileTarget.PROGRAM:
                type = string.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        _compilerTarget.FileName      = fileNameBuilder.ToString();
        _compilerTarget.ClassName     = "C" + _compilerTarget.FileName;
        _compilerTarget.NameSpace     = "N" + _compilerTarget.FileName;
        _compilerTarget.FullClassName = $"N{_compilerTarget.FileName}.C{_compilerTarget.FileName}";
        _compilerTarget.FileName     += ".cs";

        if (target != CompileTarget.PROGRAM)
            FilesToCompile.Add($"{type}{FilesToCompile.Count}");
    }

    #endregion

    #region Statistics

    private void PrintCompilationStatistics()
    {
        if (!BuildOptions.ShowCompilerStatistics || _timerBuild == null) return;
        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo    = GC.GetGCMemoryInfo();
        Console.Error.WriteLine($"memory used (bytes)  {memoryUsed}");
        Console.Error.WriteLine($"memory left (bytes)  {memInfo.TotalAvailableMemoryBytes}");
        Console.Error.WriteLine($"comp errors          {ErrorCodeHistory.Count}");
        Console.Error.WriteLine($"comp time (sec)      {_timerBuild.Elapsed}");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            ListFileWriter?.Dispose();
            foreach (var ctx in _loadContexts)
                try { ctx.Unload(); } catch { }
            _loadContexts.Clear();
            _caseFoldCache?.Clear();
        }
        _disposed = true;
    }

    ~Builder() => Dispose(false);

    #endregion

}
