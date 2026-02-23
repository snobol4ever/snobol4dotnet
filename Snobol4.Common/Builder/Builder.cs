using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

[assembly: InternalsVisibleTo("TestSnobol4")]

namespace Snobol4.Common;

public partial class Builder : IDisposable
{
    #region Members

    private const int UnexpectedExceptionErrorCode = 1000;

    public BuilderOptions BuildOptions = new();

    private readonly CompilerTargets _compilerTarget = new();
    public static long CreationOrder;

    // Timers for statistics - lazy initialized to avoid overhead
    private Stopwatch? _timerBuild;

    // Error tracking - pre-sized for typical scenarios
    public List<int> ErrorCodeHistory = new(16);
    public List<int> ColumnHistory = new(16);
    public List<string> MessageHistory = new(16);

    // Source code
    internal List<string> IncludeList = new(8);
    internal int StatementCount;
    internal string EntryLabel;

    // List file
    public StreamWriter? ListFileWriter;

    // Command line data
    public List<string> FilesToCompile = new(4);
    public string[] Arguments = [];

    // SNOBOL4 source code
    public SourceCode Code;

    // Run time
    public Executive? Execute;
    internal List<DeferredExpression> ExpressionList = new(32);
    internal List<List<Token>> ParseExpression = new(32);

    // Tracking for whether current build is for CODE or EVAL
    public bool CodeMode;

    // Move into Code generator
    public int RecordedExpressionCount = 0;

    // Track AssemblyLoadContext instances for proper disposal
    private readonly List<AssemblyLoadContext> _loadContexts = new(4);
    private bool _disposed;

    // String pool for case folding to avoid repeated allocations
    private readonly Dictionary<string, string>? _caseFoldCache;

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

    public static string Generate(string nameSpace, string className, bool firstInit, GenerateCSharpCode.CompileTarget compileTarget, Builder parent)
    {
        GenerateCSharpCode cSharp = new(parent);
        return cSharp.GenerateCSharp(nameSpace, className, firstInit, compileTarget, parent);
    }

    public static bool Lex(Builder parent, int startState = 1)
    {
        Lexer lex = new(parent, startState);
        return lex.Lex();
    }

    public static void Parse(Builder parent)
    {
        Parser parser = new(parent);
        parser.Parse();
    }

    #endregion

    #region Public Members

    public void BuildMain()
    {
        Execute = new Executive(this);
        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.PROGRAM);

            // Lazy initialize timer only when needed
            _timerBuild ??= new Stopwatch();
            _timerBuild.Restart();

            Lex(this);
            Parse(this);
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

        Execute.PrintExecutionStatistics();
        Execute.DisplayVariableValues();
        Execute.CloseAllStreams();
        ListFileWriter?.Close();
    }

    public void BuildEval()
    {
        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.EVAL);
            if (!Lex(this) && CodeMode) return;
            Parse(this);
            var cSharpCode = Generate(_compilerTarget.NameSpace, _compilerTarget.ClassName, false, GenerateCSharpCode.CompileTarget.EVAL, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = CreateTrackedLoadContext($"Eval_{_compilerTarget.EvalNum}");
            var dll = Compile(loadContext, _compilerTarget.FileName, cSharpCode);
            dynamic? instance = dll.CreateInstance(_compilerTarget.FullClassName);
            if (instance == null) return;
            instance.Run(Execute);
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

    public bool BuildCode()
    {
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
                return false;



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
        try
        {
            ClearExceptionHistory();
            var loadContext = new AssemblyLoadContext(null, true);
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
***UNEXPECTED EXCEPTION");
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
        if (!BuildOptions.CaseFolding)
            return input;

        // Use cache to avoid repeated ToUpper() calls on same strings
        if (_caseFoldCache != null && _caseFoldCache.TryGetValue(input, out var cached))
            return cached;

        var folded = input.ToUpperInvariant();

        // Cache the result if cache exists and isn't too large
        if (_caseFoldCache != null && _caseFoldCache.Count < UnexpectedExceptionErrorCode)
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