using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

[assembly: InternalsVisibleTo("TestSnobol4")]

namespace Snobol4.Common;

public class DeferredExpression
{
    internal List<Token> ExpressionList;
    public bool Parsed;

    internal DeferredExpression(List<Token> tokens)
    {
        ExpressionList = tokens;
        Parsed = false;
    }
}

/// <summary> 
/// Class for build operation in aggregate. Includes command
/// line processing, source file reading and preprocessing,
/// lexical analysis, parsing, compilation, and execution.
/// </summary>
public partial class Builder
{
    #region Members

    public static bool TraceStatements = false;

    // Timers for statistics
    private readonly Stopwatch _timerBuild = new();   // Timer for statistics

    // Error tracking
    public List<int> ErrorCodeHistory = [];
    public List<int> ColumnHistory = [];
    public List<string> MessageHistory = [];

    // Source code
    //internal List<string> PathList = [];          // List of source paths
    internal List<string> IncludeList = [];         // List of include paths
    internal int StatementCount;

    // Command line options
    internal bool SuppressSignOnMessage;            // -b
    internal bool ShowCompilerStatistics;           // -c
    internal bool WriteCSharpCode;                  // -cs
    public bool CaseFolding = true;                // -F and -f
    public bool SuppressListingHeader;              // -h
    public bool StopOnRuntimeError;                 // -k
    public bool ShowListing;                        // -l
    internal bool SuppressExecution;                // -n
    internal bool InputAfterEndStatement;           // -r
    internal string HostParameter;                  // -u
    internal bool GenerateDebugSymbols = true;      // -v
    internal bool WriteDll;                         // -w
    public bool ShowExecutionStatistics;            // -x

    public string ListFileName = "";                // Name of list file;
    public StreamWriter? ListFileWriter;

    // Command line data
    public List<string> FilesToCompile = [];        // Files to compile based on command line TODO redundant with PathList[]?
    //internal readonly string[] CommandLine;         // List of command line options
    public string[] Arguments = [];                 // List of command line arguments

    // OptionListing controls
    internal bool ListSource = false;               // Current state of LIST/NOLIST

    // SNOBOL4 source code
    public SourceCode Code;                       // Object containing all source code information

    // Compiler directives
    internal bool ErrorOnUnhandledFail = false;     // FAIL/NOFAIL

    // Run time
    public Executive? Execute;                      // Runtime object
    internal List<DeferredExpression> ExpressionList = [];
    internal List<List<Token>> ParseExpression = [];

    // Error handling for CODE and EVAL
    public bool CodeMode;

    public int EvalNum;
    public int CodeNum;

    private string _fileName = "";
    private string _className = "";
    private string _nameSpace = "";
    private string _fullClassName = "";

    public int RecordedExpressionCount = 0;

    #endregion

    #region Constructor

    public Builder()
    {
        Code = new SourceCode(this);
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

    public void BuildMain()
    {
        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.PROGRAM);
            _timerBuild.Restart();
            Lex(this);
            Parse(this);
            var cSharpCode = Generate(_nameSpace, _className, true, GenerateCSharpCode.CompileTarget.PROGRAM, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = new AssemblyLoadContext(null, true);
            var dll = Compile(loadContext, _fileName, cSharpCode);
            _timerBuild.Stop();
            PrintCompilationStatistics();

            if (MessageHistory.Count > 0 || SuppressExecution)
                return;

            Execute = new Executive(this);
            Execute.Execute(dll, loadContext, _fullClassName);
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            ReportProgrammingError(e);
        }

        ListFileWriter?.Close();
    }

    public void BuildEval()
    {
        try
        {
            GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget.EVAL);
            Lex(this);
            Parse(this);
            var cSharpCode = Generate(_nameSpace, _className, false, GenerateCSharpCode.CompileTarget.EVAL, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = new AssemblyLoadContext(null, true);
            var dll = Compile(loadContext, _fileName, cSharpCode);
            dynamic? instance = dll.CreateInstance(_fullClassName);

            if (instance == null)
                return;

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
            var cSharpCode = Generate(_nameSpace, _className, false, GenerateCSharpCode.CompileTarget.CODE, this);
            StatementCount += Code.SourceLines.Count;
            var loadContext = new AssemblyLoadContext(null, true);
            var dll = Compile(loadContext, _fileName, cSharpCode);
            dynamic? instance = dll.CreateInstance(_fullClassName);

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

    private void ReportProgrammingError(Exception e)
    {
        Console.Error.WriteLine(@"***UNEXPECTED EXCEPTION");
        Console.Error.WriteLine(@$"{e.StackTrace}");
        SaveException(e);
        WriteException(e);

        if (e.InnerException == null)
            return;

        SaveException(e.InnerException);
        WriteException(e.InnerException);
    }

    private void GetNameSpaceAndClassName(GenerateCSharpCode.CompileTarget target)
    {
        if (FilesToCompile.Count == 0 || string.IsNullOrWhiteSpace(FilesToCompile[0]))
            throw new InvalidOperationException("No source files specified for compilation.");

        _fileName = $"{Path.GetFileNameWithoutExtension(FilesToCompile[0])}";
        string type;

        switch (target)
        {
            case GenerateCSharpCode.CompileTarget.CODE:
                _fileName += $"_CODE{CodeNum++}";
                type = "CODE";
                break;

            case GenerateCSharpCode.CompileTarget.EVAL:
                _fileName += $"_EVAL{EvalNum++}";
                type = "EVAL";
                break;

            case GenerateCSharpCode.CompileTarget.PROGRAM:
                type = "";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        _className = $"C{_fileName}";
        _nameSpace = $"N{_fileName}";
        _fullClassName = $"{_nameSpace}.{_className}";
        _fileName += ".cs";
        if (target != GenerateCSharpCode.CompileTarget.PROGRAM)
            FilesToCompile.Add($"{type}{FilesToCompile.Count}");
    }

    private void ClearExceptionHistory()
    {
        ErrorCodeHistory.Clear();
        ColumnHistory.Clear();
        MessageHistory.Clear();
    }

    internal void Run(Assembly dll, AssemblyLoadContext loadContext, string fullClassName)
    {
        if (SuppressExecution)
            return;

        Execute = new Executive(this);

        Execute.Execute(dll, loadContext, fullClassName);
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
            var fullClassName = className + "0." + className;
            Execute.Execute(dll, loadContext, fullClassName);
        }
        catch (CompilerException)
        {
            // Already handled
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(@"***UNEXPECTED EXCEPTION");
            Console.Error.WriteLine(@$"{e.StackTrace}");
            SaveException(e);
            WriteException(e);
            if (e.InnerException != null)
            {
                SaveException(e.InnerException);
                WriteException(e.InnerException);
            }
        }
    }

    #region Display Statistics

    private void PrintCompilationStatistics()
    {
        if (!ShowCompilerStatistics)
            return;

        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo = GC.GetGCMemoryInfo();
        var memoryLeft = memInfo.TotalAvailableMemoryBytes;

        Console.Error.WriteLine(@"");
        Console.Error.WriteLine(@"");
        Console.Error.WriteLine($@"memory used (bytes)  {memoryUsed}");
        Console.Error.WriteLine($@"memory left (bytes)  {memoryLeft}");
        Console.Error.WriteLine($@"comp errors          {ErrorCodeHistory.Count}");
        Console.Error.WriteLine($@"regenerations        {GC.CollectionCount(memInfo.Generation)}");
        Console.Error.WriteLine($@"comp time (sec)      {_timerBuild.Elapsed}");
        Console.Error.WriteLine(@"");
        Console.Error.WriteLine(@"");
    }

    #endregion

    #region Exception Handling

    public void LogCompilerException(int code, int cursorCurrent, string message = "")
    {
        if (Execute != null)
            Execute.Failure = true;

        var ce = new CompilerException(code)
        {
            Message = message + Environment.NewLine
        };

        ErrorCodeHistory.Add(code);
        ColumnHistory.Add(cursorCurrent);
        MessageHistory.Add(ce.Message);
        Console.Error.WriteLine(ce.Message);
    }

    public void LogCompilerException(int code, int cursorCurrent, SourceLine source)
    {
        if (Execute != null)
            Execute.Failure = true;

        var ce = new CompilerException(code, cursorCurrent);
        ErrorCodeHistory.Add(code);
        ColumnHistory.Add(cursorCurrent);
        var fi = new FileInfo(source.PathName);
        ce.Message = $"{Environment.NewLine}{source.Text.Replace('\t', ' ')}{Environment.NewLine}";  // Changed
        if (cursorCurrent > 0)
            ce.Message += $"{new string(' ', cursorCurrent)}!{Environment.NewLine}";  // Changed
        ce.Message += $"{fi.Name}({source.LineCountFile},{cursorCurrent + 1}) : error {code} -- {CompilerException.ErrorMessage[code]}";
        MessageHistory.Add(ce.Message);
        Console.Error.WriteLine(ce.Message);
    }

    public void SaveException(Exception e)
    {
        if (e is CompilerException)
            return;

        ErrorCodeHistory.Add(1000);
        ColumnHistory.Add(0);
        MessageHistory.Add(e.Message);
    }

    public void WriteException(Exception e)
    {
        if (e is CompilerException ce)
        {
            Console.Error.WriteLine(@"");
            Console.Error.WriteLine(ce.Message);
            Console.Error.WriteLine(@"");
            return;
        }

        Console.Error.WriteLine(e.Message);
    }

    #endregion
}