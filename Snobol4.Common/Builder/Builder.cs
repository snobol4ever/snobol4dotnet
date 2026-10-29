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
    internal List<string> LinesAfterEnd = [];   // the source lines after END, which -r gives to INPUT
    internal int StatementCount;
    internal string EntryLabel;
    internal StreamWriter? ListFileWriter;
    private bool _listingOpened;

    // The listing goes to the -o=file named on the command line (file.lst when it has no extension), as
    // SPITBOL's does; under -l, -a, -p or -z with no -o to the first source file's name with .lst for its
    // extension, as SPITBOL's does; and otherwise to standard output, as SPITBOL's does. A file that cannot
    // be opened falls back to standard error
    internal string ListingPath =>
        BuildOptions.ListFileName != "" ? BuildOptions.ListFileName
        : BuildOptions.ShowListing && FilesToCompile.Count > 0 && FilesToCompile[0] != "" ? Path.ChangeExtension(FilesToCompile[0], ".lst")
        : "";

    internal TextWriter ListingWriter
    {
        get
        {
            if (ListingPath == "")
                return Console.Out;
            try
            {
                // the first open replaces the file; a reopen after the run's close adds to it
                ListFileWriter ??= new StreamWriter(ListingPath, append: _listingOpened) { AutoFlush = true };
                _listingOpened = true;
                return ListFileWriter;
            }
            catch (Exception)
            {
                return Console.Error;
            }
        }
    }
    public List<string> FilesToCompile = new(4);

    // The line index of the last line compiled: the program's last, then one more for each CODE call, as SPITBOL takes a line for
    // each text CODE compiles (one that does not compile too; EVAL takes none)
    internal int CodeTextLine;
    internal string[] Arguments = [];
    // The command line as given (without the program's own path), the index in it of the first source
    // file, and the words after the files that held the program, which are the program's arguments
    internal string[] CommandWords = [];
    internal int FirstFileWord = -1;
    internal List<string> ProgramArguments = [];
    internal int FilesRead;
    internal bool HostParameterGiven;
    public SourceCode Code;
    public Executive? Execute;
    internal List<DeferredExpression> ExpressionList = new(32);
    internal List<List<Token>> ParseExpression = new(32);
    internal bool CodeMode;
    internal int RecordedExpressionCount = 0;

    // The status the console exits with when the run did not get as far as running the program, as sbl's does: 231 when execution
    // was suppressed (a compile error, or -n), 1 when the source could not be read (a file or an -INCLUDE that cannot be opened,
    // no END). Null leaves the exit to &CODE.
    public int? ExitStatus;

    // The source could not be read: the run ends before it starts, whatever -k says
    internal bool SourceFailed;

    // An -INCLUDE could not be opened (see ReportNoEndAfterFailedInclude)
    internal bool IncludeFailed;

    // Threaded execution tables
    internal List<VariableSlot> VariableSlots = new(64);
    internal Dictionary<string, int> VariableSlotIndex = new(64, StringComparer.Ordinal);
    internal List<FunctionSlot> FunctionSlots = new(64);
    internal Dictionary<string, int> FunctionSlotIndex = new(64, StringComparer.Ordinal);
    internal ConstantPool Constants = new();
    internal int[]? StatementInstructionStarts;
    // Each statement's failure exit (its :F label, else its unconditional label; null for none or an
    // indirect goto), by global statement index
    internal Dictionary<int, string?> FailureExits = new();

    // A failure exit that is an expression naming a label (:F($'RETURN'), or such an unconditional goto when there is no :F),
    // by statement: :(CONTINUE) evaluates it when it resumes there, as the goto would be
    internal Dictionary<int, List<Token>> FailureExitExpressions = new();

    // A statement's failure exit when it is a direct goto (:<C>, :F<C>): the expression whose value is the CODE :(CONTINUE) resumes at
    internal Dictionary<int, List<Token>> FailureExitCodes = new();

    // The statements whose body holds a negation or a choice, a failure point of its own inside the statement where :(CONTINUE) may
    // resume after an error: a SETEXIT handler of an error in one runs at the error until it continues (RunHandlerAtError)
    internal HashSet<int> FailurePointStatements = [];

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
            ReportNoEndAfterFailedInclude();
            ReportMissingEnd();

            // When -CASE changed folding part way, the lexer already folded each line's names as its
            // line required, so the passes after it take the names as spelled; the program then runs
            // with the folding the last -CASE left
            var finalFolding = BuildOptions.CaseFolding;
            if (Code.SourceLines.Any(line => line.CaseFolding != finalFolding))
                BuildOptions.CaseFolding = false;
            Parse(this);
            ResolveSlots();

            var tc = new ThreadedCodeCompiler(this);
            EmitMsilForAllStatements();
            Execute.Thread = tc.Compile();
            CompileStarFunctions(tc);
            PopulateMainMetadata();
            ComputeThreadIsMsilOnly();
            BuildOptions.CaseFolding = finalFolding;
            if (BuildOptions.WriteDll) SaveDll();
            _timerBuild.Stop();
            PrintCompilationStatistics();
            // -n or -NOEXECUTE: compile (and write -w's file) but do not run, as in SPITBOL; nor does a
            // program with compilation errors run, unless -k or -ERRORS asks for it
            if (!BuildOptions.SuppressExecution && !SourceFailed && (ErrorCodeHistory.Count == 0 || BuildOptions.RunDespiteCompileErrors))
            {
                Execute.StartTimer();
                Execute.ExecuteLoop(0);
            }
            else
                ExitStatus ??= 231;
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
            // an EVAL'd expression is no statement: it takes no statement number, as in SPITBOL
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
            EmitMsilForAllStatements(stmtOffset);
            var tc = new ThreadedCodeCompiler(this);
            Execute!.Thread = tc.AppendCompile(Execute.Thread!, stmtOffset);
            CompileStarFunctions(tc);
            PopulateCodeMetadata(stmtOffset);
            StatementCount += Code.SourceLines.Count + 1;   // the block's statements and its implicit end
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
        // -m: seed &MAXLNGTH from the command-line value before any user code runs
        exec.AmpMaxLength = BuildOptions.MaxObjectBytes;
        exec.MaxObjectLength = BuildOptions.MaxObjectBytes;

        // -f: CaseFolding OFF → seed &CASE=0 so EVAL/CODE compile with the same
        // case-sensitivity setting as the main program.  Without this, EVAL would
        // temporarily restore CaseFolding=true (because AmpCaseFolding defaults to 1),
        // folding identifiers to UPPER and causing "undefined function" errors when the
        // user-defined name was registered under its original mixed-case form.
        if (!BuildOptions.CaseFolding)
            exec.AmpCaseFolding = 0;
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
        // the text of each CODE is a line of the program's file after its last, as SPITBOL counts it (CodeTextLine)
        if (Code.SourceLines.Count > 0)
            CodeTextLine = Code.SourceLines[^1].LineCountFile;

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
                // Pre-bake the wire-emission STNO so runtime LABEL emit doesn't
                // depend on Parent.Code at execution time. See SourceStno
                // docstring on Executive — EVAL/CODE swap out Parent.Code,
                // and a runtime BlankLineCount lookup against the swapped
                // SourceLines silently returns 0, producing wrong stno on
                // the wire.  GOAL-NET-BEAUTY-SELF session #58.
                Execute.SourceStno.Add((long)(stmtNumber + 1 + line.BlankLineCount));
                if (line.CompileError)
                    Execute.ErroneousStatements.Add(stmtNumber);
            }
            stmtNumber++;
        }

        stmtNumber = StatementCount;
        foreach (var line in Code.SourceLines)
        {
            if (line.Label.Length > 0 && !line.Compiled)
            {
                var lbl = line.CaseFolding ? line.Label.ToUpper() : line.Label;
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

        // :(END) goes to the END statement, which runs like any other: it is counted in &STCOUNT and is the
        // statement &STNO and &LINE name at the end of the run, as in SPITBOL (falling into END did so already)
        if (Code.EndFound && Code.SourceLines.Count > 0)
            Execute.LabelTable["END"] = Execute.LabelTable["end"] = StatementCount + Code.SourceLines.Count - 1;

        StatementCount += Code.SourceLines.Count;
    }

    private void PopulateCodeMetadata(int stmtOffset)
    {
        if (Execute == null) return;
        var stmtNumber = stmtOffset;
        // CODE's statements are numbered on from the last statement number recorded (the program's END,
        // or the previous block's implicit end), as SPITBOL numbers them; a blank statement (CODE(' A ; ; B'))
        // takes a number too, though it compiles to nothing
        var stnoBase = Execute.SourceStno.Count > 0 ? Execute.SourceStno[^1] : 0;
        // a text that does not compile defines the labels of the statements before the one in error and no other, as SPITBOL's compiler
        // has defined them when it meets the error (CODE(' X = 1;L2 Y = SQRT(-0.5);L3 Z = 1') leaves L2 and L3 as they were)
        var inError = false;
        foreach (var line in Code.SourceLines)
        {
            inError |= line.CompileError;
            if (!line.Compiled)
            {
                var codeCount = 1 + line.LineCountFile - line.BlankLineCount - line.CommentContinuationDirectiveCount;
                var listCount = 1 + line.LineCountFile - line.CommentContinuationDirectiveCount;
                Execute.SourceCode.Add($"{Path.GetFileName(line.PathName)}:{codeCount}/{listCount})\n{line.Text.Replace('\t', ' ')}");
                Execute.SourceFiles.Add(line.PathName);
                Execute.SourceLineNumbers.Add(line.LineCountFile);
                Execute.SourceStatementNumbers.Add(stmtNumber + 1);
                Execute.SourceStno.Add(stnoBase + (stmtNumber - stmtOffset) + 1 + line.BlankLineCount);
                if (line.Label.Length > 0 && !inError)
                {
                    var folded = line.CaseFolding ? line.Label.ToUpper() : line.Label;
                    if (folded is not "END")
                        Execute.LabelTable[folded] = stmtNumber;
                }
            }
            stmtNumber++;
        }

        // SPITBOL ends each CODE block with an implicit statement that takes the next number, so the
        // following block numbers on after it (CODE(' A = &STNO ; B = &STNO') after a 12-statement program
        // gives 13 and 14, and the next block starts at 16); its entry keeps the indices aligned. A block that ends in its own END
        // has no implicit statement: the entry takes the END's number, so the next block numbers on from the END (CODE('END') after a
        // seven-statement program is 8, and the next block begins at 9)
        if (Code.SourceLines.Count > 0)
        {
            var last = Code.SourceLines[^1];
            Execute.SourceCode.Add($"{Path.GetFileName(last.PathName)}:0/0)\n");
            Execute.SourceFiles.Add(last.PathName);
            Execute.SourceLineNumbers.Add(last.LineCountFile);
            Execute.SourceStatementNumbers.Add(stmtNumber + 1);
            Execute.SourceStno.Add(stnoBase + Code.SourceLines.Count + Code.BlankLineCount + (Code.EndFound ? 0 : 1));
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
            Executive.DeferredCode function = x => x.RunExpressionThread(subThread);
            Execute.StarFunctionList.Add(function);
            if (ParseExpression[i] is [{ TokenType: Token.Type.IDENTIFIER } variable])
                Execute.SimpleExpressionNames[function] = FoldCase(variable.MatchedString);
            if (ParseExpression[i] is [.., var root] && GivesValueOnly(root))
                Execute.ValueExpressions.Add(function);
        }
        Execute.PreviousStarFunctionCount = Execute.StarFunctionList.Count;
    }

    // An expression whose last operation (its root, last in postfix) can only make a value: a literal, an assignment, a
    // concatenation, arithmetic, a pattern operator, a match, or the name, negation, interrogation or cursor operator.  A
    // variable, $X, &K, a subscript and a call can give a name (a call only when it returns one), and so can an operator a
    // program may define with OPSYN, as SPITBOL compiles an expression by name when it can (sbl -bf: . *(.X) and
    // . *('Z' 'Z') fail the match, . *$'ZZ', . *A<1>, . *ITEM(A,2) and . *VAL(N) assign)
    private static bool GivesValueOnly(Token root) => root.TokenType switch
    {
        Token.Type.STRING or Token.Type.INTEGER or Token.Type.REAL or Token.Type.NULL => true,
        Token.Type.BINARY_EQUAL or Token.Type.BINARY_CONCAT or Token.Type.BINARY_PLUS or Token.Type.BINARY_MINUS
            or Token.Type.BINARY_STAR or Token.Type.BINARY_SLASH or Token.Type.BINARY_CARET or Token.Type.BINARY_PIPE
            or Token.Type.BINARY_PERIOD or Token.Type.BINARY_DOLLAR or Token.Type.BINARY_QUESTION => true,
        Token.Type.UNARY_OPERATOR => root.MatchedString is "." or "-" or "+" or "~" or "?" or "@",
        _ => false
    };

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

        switch (target)
        {
            case CompileTarget.CODE:
                fileNameBuilder.Append("_CODE").Append(_compilerTarget.CodeNum++);
                break;
            case CompileTarget.EVAL:
                fileNameBuilder.Append("_EVAL").Append(_compilerTarget.EvalNum++);
                break;
            case CompileTarget.PROGRAM:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        _compilerTarget.FileName      = fileNameBuilder.ToString();
        _compilerTarget.ClassName     = "C" + _compilerTarget.FileName;
        _compilerTarget.NameSpace     = "N" + _compilerTarget.FileName;
        _compilerTarget.FullClassName = $"N{_compilerTarget.FileName}.C{_compilerTarget.FileName}";
        _compilerTarget.FileName     += ".cs";
    }

    #endregion

    #region Statistics

    // -c: the compiler statistics on the listing stream, as sbl lays them out: the head if nothing has been listed (without a page
    // line), three empty lines after what was listed, five rows labelled to column 21, and three empty lines
    private void PrintCompilationStatistics()
    {
        // a source that could not be read ends the run before there is a compilation to report
        if (!BuildOptions.ShowCompilerStatistics || _timerBuild == null || SourceFailed) return;
        var memoryUsed = Process.GetCurrentProcess().WorkingSet64;
        var memInfo    = GC.GetGCMemoryInfo();
        var writer = ListingWriter;
        if (_listingPage == 0)
        {
            WriteListingHead(writer);
            _listingPage = -1;   // the head is out; the page line is not
        }
        else
            writer.Write("\n\n\n");
        writer.Write($"{"memory used (bytes)",-21}{memoryUsed}\n");
        writer.Write($"{"memory left (bytes)",-21}{memInfo.TotalAvailableMemoryBytes}\n");
        writer.Write($"{"comp errors",-21}{ErrorCodeHistory.Count}\n");
        writer.Write($"{"REGENERATIONS",-21}{GC.CollectionCount(2)}\n");
        writer.Write($"{"comp time (microsec)",-21}{_timerBuild.Elapsed.Ticks / 10}\n");
        writer.Write("\n\n\n");
        writer.Flush();
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
