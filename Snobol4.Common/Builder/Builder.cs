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
    
    // Timers for statistics - lazy initialized to avoid overhead
    private Stopwatch? _timerBuild;

    // Track AssemblyLoadContext instances for proper disposal
    private readonly List<AssemblyLoadContext> _loadContexts = new(4);
    private bool _disposed;

    // String pool for case folding to avoid repeated allocations
    private readonly Dictionary<string, string>? _caseFoldCache;

    // Build parameters and options
    public BuilderOptions BuildOptions = new();
    
    // Error tracking - pre-sized for typical scenarios
    public List<int> ErrorCodeHistory = new(16);
    public List<int> ColumnHistory = new(16);
    public List<string> MessageHistory = new(16);

    // Source code
    internal List<string> IncludeList = new(8);
    internal int StatementCount;
    internal string EntryLabel;
    
    // List file
    internal StreamWriter? ListFileWriter;
    
    // Command line data
    public List<string> FilesToCompile = new(4);
    internal string[] Arguments = [];
    
    // SNOBOL4 source code
    public SourceCode Code;

    // Run time
    public Executive? Execute;
    internal List<DeferredExpression> ExpressionList = new(32);
    internal List<List<Token>> ParseExpression = new(32);

    // Tracking for whether current build is for CODE or EVAL
    internal bool CodeMode;

    // Move into Code generator
    internal int RecordedExpressionCount = 0;

    // -----------------------------------------------------------------------
    // Pre-resolution tables for threaded execution (Phase 2)
    //
    // Populated by ResolveSlots() after Parse() completes.
    // The C# generation path does not use these — additive, no behaviour change.
    // -----------------------------------------------------------------------

    /// <summary>All distinct variable names, indexed by slot number.</summary>
    internal List<VariableSlot> VariableSlots = new(64);
    internal Dictionary<string, int> VariableSlotIndex = new(64, StringComparer.Ordinal);

    /// <summary>All distinct function/operator call sites, indexed by slot number.</summary>
    internal List<FunctionSlot> FunctionSlots = new(64);
    internal Dictionary<string, int> FunctionSlotIndex = new(64, StringComparer.Ordinal);

    /// <summary>Interned constant literals (strings, integers, reals).</summary>
    internal ConstantPool Constants = new();

    /// <summary>
    /// Maps statement index → first instruction index in Thread[].
    /// Populated by ThreadedCodeCompiler.Compile() and used by
    /// ThreadedExecuteLoop to resolve label-table gotos at runtime.
    /// </summary>
    internal int[]? StatementInstructionStarts;

    #endregion

    #region Constructor

    public Builder()
    {
        Code = new SourceCode(this);
        EntryLabel = string.Empty;

        // Only create cache if case folding is enabled
        if (BuildOptions.CaseFolding)
        {
            _caseFoldCache = new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    #endregion

    #region Static Factory Methods

    internal static string Generate(string nameSpace, string className, bool firstInit, GenerateCSharpCode.CompileTarget compileTarget, Builder parent)
    {
        GenerateCSharpCode cSharp = new(parent);
        return cSharp.GenerateCSharp(nameSpace, className, firstInit, compileTarget, parent);
    }

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

    #region Public Members

    public void BuildMain()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Execute = new Executive(this);
        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.PROGRAM);

            // Lazy initialize timer only when needed
            _timerBuild ??= new Stopwatch();
            _timerBuild.Restart();
            Lex(this);
            Parse(this);
            ResolveSlots();
            if (BuildOptions.UseThreadedExecution)
            {
                var tc = new ThreadedCodeCompiler(this);
                Execute.Thread = tc.Compile();
            }
            var cSharpCode = Generate(_compilerTarget.NameSpace, _compilerTarget.ClassName, true, GenerateCSharpCode.CompileTarget.PROGRAM, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = CreateTrackedLoadContext($"Main_{_compilerTarget.ClassName}");
            var dll = Compile(loadContext, _compilerTarget.FileName, cSharpCode);
            _timerBuild.Stop();
            PrintCompilationStatistics();

            if (MessageHistory.Count == 0 || !BuildOptions.SuppressExecution)
            {
                Execute.Execute(dll, loadContext, _compilerTarget.FullClassName);
            }
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            ReportProgrammingError(e);
        }

        Execute?.PrintExecutionStatistics();
        Execute?.DisplayVariableValues();
        Execute?.CloseAllStreams();
        ListFileWriter?.Close();
    }

    internal void BuildEval()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.EVAL);
    
            if (!Lex(this))
            {
                // If Lex fails, substitute a null that is guaranteed to parse, so the lex error is not fatal
                Code.SourceLines[0].Text = " *('')";
                Lex(this);
            }
            
            Parse(this);
            var cSharpCode = Generate(_compilerTarget.NameSpace, _compilerTarget.ClassName, false, GenerateCSharpCode.CompileTarget.EVAL, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = CreateTrackedLoadContext($"Eval_{_compilerTarget.EvalNum}");
            var dll = Compile(loadContext, _compilerTarget.FileName, cSharpCode);
            dynamic? instance = dll.CreateInstance(_compilerTarget.FullClassName);
            instance?.Run(Execute);
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            ReportProgrammingError(e);
        }
    }

    internal bool BuildCode()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.CODE);
            Lex(this);
            Parse(this);
            var cSharpCode = Generate(_compilerTarget.NameSpace, _compilerTarget.ClassName, false, GenerateCSharpCode.CompileTarget.CODE, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = CreateTrackedLoadContext($"Code_{_compilerTarget.CodeNum}");
            var dll = Compile(loadContext, _compilerTarget.FileName, cSharpCode);
            dynamic? instance = dll.CreateInstance(_compilerTarget.FullClassName);

            if (instance == null)
            {
                return false;
            }

            instance.Run(Execute);
            return true;
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            ReportProgrammingError(e);
        }

        return false;
    }

    public void RunDll(string dllFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dllFileName);
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            ClearExceptionHistory();
            var loadContext = CreateTrackedLoadContext("RunDll");
            var dll = loadContext.LoadFromAssemblyPath(dllFileName);
            Execute = new Executive(this);
            var className = Path.GetFileNameWithoutExtension(dllFileName);
            var fullClassName = $"{className}0.{className}";
            Execute.Execute(dll, loadContext, fullClassName);
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(@"
***UNEXPECTED EXCEPTION
");
            Console.Error.WriteLine(@$"
{e.StackTrace}");
            SaveException(e);
            WriteException(e);
            if (e.InnerException != null)
            {
                SaveException(e.InnerException);
                WriteException(e.InnerException);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string FoldCase(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!BuildOptions.CaseFolding)
            return input;

        // Use cache to avoid repeated ToUpper() calls on same strings
        if (_caseFoldCache != null && _caseFoldCache.TryGetValue(input, out var cached))
        {
            return cached;
        }

        var folded = input.ToUpperInvariant();

        // Cache the result if cache exists and isn't too large
        if (_caseFoldCache != null && _caseFoldCache.Count < _unexpectedExceptionErrorCode)
        {
            _caseFoldCache[input] = folded;
        }

        return folded;
    }
    
    #endregion

    #region Private Members

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AssemblyLoadContext CreateTrackedLoadContext(string? name = null)
    {
        var loadContext = new AssemblyLoadContext(name, isCollectible: true);
        _loadContexts.Add(loadContext);
        return loadContext;
    }

    private void GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget target)
    {
        if (FilesToCompile.Count == 0 || string.IsNullOrWhiteSpace(FilesToCompile[0]))
            throw new InvalidOperationException("No source files specified for compilation.");

        // Use StringBuilder for efficient string building
        var baseFileName = Path.GetFileNameWithoutExtension(FilesToCompile[0]);
        var fileNameBuilder = new StringBuilder(baseFileName);
        string type;

        switch (target)
        {
            case GenerateCSharpCode.CompileTarget.CODE:
                fileNameBuilder.Append("_CODE").Append(_compilerTarget.CodeNum++);
                type = "CODE";
                break;

            case GenerateCSharpCode.CompileTarget.EVAL:
                fileNameBuilder.Append("_EVAL").Append(_compilerTarget.EvalNum++);
                type = "EVAL";
                break;

            case GenerateCSharpCode.CompileTarget.PROGRAM:
                type = string.Empty;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        _compilerTarget.FileName = fileNameBuilder.ToString();
        _compilerTarget.ClassName = string.Concat("C", _compilerTarget.FileName);
        _compilerTarget.NameSpace = string.Concat("N", _compilerTarget.FileName);
        _compilerTarget.FullClassName = $"N{_compilerTarget.FileName}.C{_compilerTarget.FileName}";
        _compilerTarget.FileName = string.Concat(_compilerTarget.FileName, ".cs");

        if (target != GenerateCSharpCode.CompileTarget.PROGRAM)
            FilesToCompile.Add($"{type}{FilesToCompile.Count}");
    }

    #endregion

    #region Display Statistics

    private void PrintCompilationStatistics()
    {
        if (!BuildOptions.ShowCompilerStatistics || _timerBuild == null)
            return;

        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo = GC.GetGCMemoryInfo();
        var memoryLeft = memInfo.TotalAvailableMemoryBytes;

        Console.Error.WriteLine();
        Console.Error.WriteLine();
        Console.Error.WriteLine($@"memory used (bytes)  {memoryUsed}");
        Console.Error.WriteLine($@"memory left (bytes)  {memoryLeft}");
        Console.Error.WriteLine($@"comp errors          {ErrorCodeHistory.Count}");
        Console.Error.WriteLine($@"regenerations        {GC.CollectionCount(memInfo.Generation)}");
        Console.Error.WriteLine($@"comp time (sec)      {_timerBuild.Elapsed}");
        Console.Error.WriteLine();
        Console.Error.WriteLine();
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            ListFileWriter?.Dispose();

            // Unload and dispose all assembly load contexts
            foreach (var context in _loadContexts)
            {
                try
                {
                    context.Unload();
                }
                catch (Exception ex)
                {
                    // Log but don't throw during disposal
                    Console.Error.WriteLine($"Warning: Failed to unload context: {ex.Message}");
                }
            }
            _loadContexts.Clear();

            // Clear cache
            _caseFoldCache?.Clear();
        }

        _disposed = true;
    }

    ~Builder()
    {
        Dispose(false);
    }

    #endregion
}