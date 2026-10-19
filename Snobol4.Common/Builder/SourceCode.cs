using System.Globalization;
using System.Text;

namespace Snobol4.Common;

public class SourceCode
{
    // Constants for special characters and directives
    //private const char _commentChar = '*';
    //private const char _directiveChar = '-';
    private const char _continuationChar = '+';
    private const char _alternateContinuationChar = '.';
    private const char _semicolonChar = ';';
    private const string _specialChars = "+.*";
    private const string _commandChars = "-";
    
    // Magic numbers for parsing
    private const int _endStatementLength = 3;
    private const int _endStatementOffset = 3;
    private const int _caseDirectiveValueOffset = 5;

    // Counter for number of lines in a file
    internal int LineCountFile { get; set; }

    // Counter for blank lines
    internal int BlankLineCount { get; set; }

    // Counter for comments and directives
    internal int CommentContinuationDirectiveCount { get; set; }

    // Counter for numbers of lines in all files
    internal int LineCountTotal { get; set; }

    // Counter for semicolon delimited lines
    internal int SubLineCount { get; set; }

    // List of source lines
    internal List<SourceLine> SourceLines { get; set; } = [];

    // Dictionary associating labels to line numbers
    internal Dictionary<string, int> Labels { get; set; } = [];

    // List of source paths
    internal List<string> PathList { get; set; } = [];

    // True if an end statement was found
    internal bool EndFound { get; set; }

    // Builder invoking this class
    private readonly Builder _parent;

    // Depth of INCLUDE statements
    private int _includeDepth;

    // Continuation line counter
    private int _continuation;
    private string? _lineFileName;   // the file a -LINE control named, for the lines after it
    private string? _staleIncludeName;   // the included file's name, which sbl gives the next statement after it
    private bool _lastLineIsNote;        // the last line read was a comment or a control line

    internal SourceCode(Builder parent)
    {
        _parent = parent;
    }

    #region Public Methods

    /// <summary>
    /// Reads all source files specified in FilesToCompile
    /// </summary>
    public void ReadAllFiles()
    {
        EndFound = false;
        _parent.DisplayListingBanner();

        try
        {
            // Files are read until one holds END; the words after it are the program's arguments
            // (HOST(0), HOST(2,n)), as SPITBOL takes them
            var used = 0;
            foreach (var fileName in _parent.FilesToCompile.Select(AdjustFileExtension))
            {
                // the working directory stays where the program was started: relative file names and
                // includes resolve from there, as in SPITBOL, not from the source file's directory
                var fullPath = Path.GetFullPath(fileName);
                PathList.Add(fullPath);
                using StreamReader sourceFileReader = new(fullPath, Encoding.UTF8);
                ReadFile(sourceFileReader, fullPath);
                used++;
                if (EndFound)
                    break;
            }

            _parent.FilesRead = used;
            _parent.ProgramArguments = _parent.FilesToCompile.Skip(used).ToList();
            _parent.FilesToCompile.RemoveRange(used, _parent.FilesToCompile.Count - used);
            if (!_parent.HostParameterGiven)
                _parent.BuildOptions.HostParameter = string.Join(" ", _parent.ProgramArguments);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine(e.Message);
            return;
        }

        if (PathList.Count > 0 && EndFound)
            return;

        _parent.LogCompilerException(216);
    }

    /// <summary>
    /// Reads a test script from a memory stream (for testing purposes)
    /// </summary>
    public void ReadTestScript(MemoryStream scriptStream)
    {
        EndFound = false;
        Labels = [];
        var script = _parent.FilesToCompile[0];
        var fileInfo = new FileInfo(AdjustFileExtension(script));

        if (fileInfo.DirectoryName is not null)
            Directory.SetCurrentDirectory(fileInfo.DirectoryName);

        PathList.Add(fileInfo.FullName);

        using var reader = new StreamReader(scriptStream, Encoding.UTF8);
        ReadFile(reader, fileInfo.FullName);

        if (EndFound)
            return;

        _parent.LogCompilerException(216);
    }

    #endregion

    #region File Reading

    /// <summary>
    /// Reads source code from a string (used for inline code processing)
    /// </summary>
    /// <param name="source">The source code string to process</param>
    /// <param name="pathName">The logical path name for this source</param>
    internal void ReadCodeInString(string source, string pathName)
    {
        InitializeLineCounters();
        // CODE text keeps its trailing blanks: CODE(' A = 1 ; ') ends in a blank statement, as in SPITBOL
        var lines = SplitLineByDelimiter(source, '\r', '\n');

        foreach (var line in lines)
        {
            LineCountFile++;
            ProcessLineWithSubLines(line, pathName, true);
        }
    }

    /// <summary>
    /// Reads and processes a source file
    /// </summary>
    /// <param name="reader">Stream reader for the source file</param>
    /// <param name="pathName">Full path to the source file</param>
    private void ReadFile(StreamReader reader, string pathName)
    {
        // An included file counts its own lines from 1, and the including file resumes its count after
        // it, as &LINE reads under sbl; a -LINE file name lasts to the end of the file that set it
        var (callerLineCount, callerContinuation, callerLineFileName, callerStaleName) = (LineCountFile, _continuation, _lineFileName, _staleIncludeName);
        LineCountFile = 0;
        _continuation = 0;
        _lineFileName = null;
        _staleIncludeName = null;

        while (!EndFound && !reader.EndOfStream)
        {
            LineCountFile += _continuation;
            LineCountTotal++;
            var currentLine = ReadLineAndContinuations(reader, out var continuation);
            _continuation = continuation;
            ListSource(currentLine);
            
            ProcessLineWithSubLines(currentLine, _staleIncludeName ?? _lineFileName ?? pathName);

            // A comment or control line leaves a carried-over included name in place for the next
            // statement; a statement or a blank (null statement) line uses it up
            _lastLineIsNote = currentLine.Length > 0 && currentLine[0] is '*' or '-';
            if (!_lastLineIsNote)
                _staleIncludeName = null;
        }

        // When an included file ends on a comment or control line, sbl names the including file's next
        // statement after the included file, keeping its own line number (Budne's include and line:
        // line2.sno ends with *)
        if (_includeDepth > 0)
            (LineCountFile, _continuation, _lineFileName, _staleIncludeName) =
                (callerLineCount, callerContinuation, callerLineFileName, _lastLineIsNote ? pathName : null);
        else
            _staleIncludeName = callerStaleName;

        // The lines after END are data: -r gives them to INPUT, as in SPITBOL
        if (EndFound && _includeDepth == 0 && _parent.LinesAfterEnd.Count == 0)
        {
            while (reader.ReadLine() is { } afterEnd)
                _parent.LinesAfterEnd.Add(afterEnd);
        }
    }

    /// <summary>
    /// Initializes line counters for processing
    /// </summary>
    private void InitializeLineCounters()
    {
        LineCountFile = 1;
        BlankLineCount = 0;
        CommentContinuationDirectiveCount = 0;
        LineCountTotal++;
        SubLineCount = 0;
    }

    /// <summary>
    /// Reads a line and concatenates any continuation lines (starting with '.' or '+')
    /// </summary>
    /// <param name="reader">Stream reader to read from</param>
    /// <param name="continuationLines">Output: number of lines concatenated</param>
    /// <returns>The complete line including any continuations</returns>
    private static string ReadLineAndContinuations(StreamReader reader, out int continuationLines)
    {
        var line = reader.ReadLine();
        continuationLines = 1;

        if (line is null)
            return string.Empty;

        var currentLine = line;

        while (!reader.EndOfStream && reader.Peek() is _continuationChar or _alternateContinuationChar)
        {
            currentLine += reader.ReadLine()?[1..];
            continuationLines++;
        }

        return currentLine;
    }

    /// <summary>
    /// Adjusts file extension if not present, trying common SNOBOL4 extensions
    /// </summary>
    /// <param name="file">The file path to adjust</param>
    /// <returns>File path with appropriate extension</returns>
    private static string AdjustFileExtension(string file)
    {
        var extension = Path.GetExtension(file);

        if (!string.IsNullOrEmpty(extension))
            return file;

        foreach (var ext in _extensions.Where(ext => new FileInfo(file + ext).Exists))
            return Path.ChangeExtension(file, ext);

        return file;
    }

    #endregion

    #region Line Processing

    /// <summary>
    /// Processes a line and its semicolon-delimited sub-lines
    /// </summary>
    /// <param name="line">The line to process</param>
    /// <param name="pathName">Path to the source file</param>
    private void ProcessLineWithSubLines(string line, string pathName, bool codeText = false)
    {
        SubLineCount = 0;

        // In SNOBOL4, a comment line has '*' in column 1. Such lines must be
        // skipped BEFORE semicolon splitting — otherwise "* comment; key/value"
        // splits into "* comment" (skipped) and "key/value" (parsed as code, error 233).
        if (line.Length > 0 && line[0] == '*')
        {
            CommentContinuationDirectiveCount++;
            return;
        }

        // Each segment between semicolons is a statement, a blank one included, but nothing after a final
        // semicolon is none; a source line's trailing blanks are trimmed first (X = 1 ;  ends at X = 1), while
        // CODE text keeps them (CODE(' X = 1 ; ') has a blank second statement), as SPITBOL numbers them
        var subLines = SplitLineByDelimiter(line, _semicolonChar);
        if (subLines.Count > 1 && (codeText ? subLines[^1].Length == 0 : string.IsNullOrWhiteSpace(subLines[^1])))
            subLines.RemoveAt(subLines.Count - 1);

        foreach (var subLine in subLines)
        {
            ProcessSubLine(subLine.TrimEnd(), pathName);
        }
    }

    /// <summary>
    /// Processes a single sub-line (part of a semicolon-delimited line)
    /// </summary>
    /// <param name="subLine">The sub-line to process</param>
    /// <param name="pathName">Path to the source file</param>
    private void ProcessSubLine(string subLine, string pathName)
    {
        if (string.IsNullOrWhiteSpace(subLine))
        {
            BlankLineCount++;
            return;
        }

        SubLineCount++;

        if (IsCommentOrContinuation(subLine))
        {
            CommentContinuationDirectiveCount++;
            return;
        }

        if (IsCommand(subLine))
        {
            CommentContinuationDirectiveCount++;
            ExecuteDirectives(subLine[1..], pathName);
            return;
        }

        SourceLines.Add(new SourceLine(pathName, _includeDepth, subLine, this, _parent.BuildOptions.ErrorOnUnhandledFail));

        if (!IsEndStatement(subLine))
            return;

        _parent.EntryLabel = ProcessEntry(subLine);
        EndFound = true;
    }

    /// <summary>
    /// Determines if a line is a comment or continuation marker
    /// </summary>
    private static bool IsCommentOrContinuation(string line) 
        => line.Length > 0 && _specialChars.Contains(line[0]);

    /// <summary>
    /// Determines if a line is a directive command
    /// </summary>
    private static bool IsCommand(string line) 
        => line.Length > 0 && _commandChars.Contains(line[0]);

    /// <summary>
    /// Displays source line in listing output if enabled
    /// </summary>
    /// <param name="line">The line to display</param>
    private void ListSource(string line)
    {
        if (_parent.BuildOptions.ListSource)
            _parent.ListingWriter.WriteLine($"{LineCountTotal:0000} {LineCountFile:0000} {_includeDepth:00} {line}");
    }

    #endregion

    #region Statement Analysis

    /// <summary>
    /// Determines if a line contains an END statement
    /// </summary>
    /// <param name="line">The line to check</param>
    /// <returns>True if the line is an END statement</returns>
    private bool IsEndStatement(string line)
    {
        var m = CompiledRegex.EndPattern().Match(line);
        if (!m.Success)
            return false;

        // SNOBOL4 END statement keyword is always uppercase, regardless of
        // -F (fold) / -f (no-fold). Per RULES.md "Always uppercase END":
        // .sno files always write END in uppercase. Under fold mode the
        // source is also uppered before comparison so OrdinalIgnoreCase is
        // safe; under no-fold mode the source case is preserved and only
        // exact "END" is the keyword.
        var trimmedValue = m.Value.TrimEnd();
        return _parent.BuildOptions.CaseFolding
            ? trimmedValue.Equals("END", StringComparison.OrdinalIgnoreCase)
            : trimmedValue == "END";
    }

    /// <summary>
    /// Processes the entry point label from an END statement
    /// </summary>
    /// <param name="subLine">The END statement line</param>
    /// <returns>The entry label, or empty string if none specified</returns>
    private string ProcessEntry(string subLine)
    {
        if (subLine.Trim().Length == _endStatementLength)
            return string.Empty;

        var m = CompiledRegex.EntryLabelPattern().Match(subLine[_endStatementOffset..]);

        if (!m.Success)
            _parent.LogCompilerException(215);

        return m.Groups[2].Value;
    }

    #endregion

    #region Directive Processing

    /// <summary>
    /// Executes compiler directives found in the source
    /// </summary>
    /// <param name="line">The directive line (without the leading '-')</param>
    /// <param name="pathName">Path to the source file</param>
    private void ExecuteDirectives(string line, string pathName)
    {
        var directiveList = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var directive in directiveList)
        {
            var m = CompiledRegex.KeywordPattern().Match(directive);

            if (m.Success)
                ParseDirective(m.Value, directive, pathName);
        }
    }

    /// <summary>
    /// Parses and executes a specific directive
    /// </summary>
    /// <param name="match">The matched keyword</param>
    /// <param name="directive">The full directive text</param>
    /// <param name="pathName">Path to the source file</param>
    private void ParseDirective(string match, string directive, string pathName)
    {
        // A control in lower case is a control only while names fold, as sbl reads it (Budne's case1:
        // -case 1 after -case is ignored)
        if (!_parent.BuildOptions.CaseFolding && match.Any(char.IsLower))
            return;

        var keyword = match.ToLower().Trim();
        
        switch (keyword)
        {
            case "copy":
            case "include":
                ProcessIncludeDirective(directive);
                break;

            case "case":
                ProcessCaseDirective(directive);
                break;

            case "fail":
                _parent.BuildOptions.ErrorOnUnhandledFail = true;
                break;

            case "nofail":
                _parent.BuildOptions.ErrorOnUnhandledFail = false;
                break;

            // -NOEXECUTE compiles and does not run, as the -n switch; the last of the two in the
            // program wins, as in SPITBOL
            case "execute":
                _parent.BuildOptions.SuppressExecution = false;
                break;

            case "noexecute":
                _parent.BuildOptions.SuppressExecution = true;
                break;

            case "list":
                _parent.BuildOptions.ListSource = true;
                break;

            case "nolist":
                _parent.BuildOptions.ListSource = false;
                break;

            case "line":
                ProcessLineDirective(directive);
                break;

            case "print":
            case "noprint":
            case "double":
            case "on":
            case "nooptimize":
            case "optimize":
            case "stitl":
            case "title":
                Console.Error.WriteLine($"WARNING: Directive -{match.Trim()} in {pathName} ignored");
                break;

            // SPITBOL ignores a control it does not know (the manual; sbl is silent on -HIDE, -PLUSOPS)
            default:
                break;
        }
    }

    /// <summary>
    /// -LINE n ["file"]: the next line is line n (of file, when named), the lines after it following on,
    /// as &LINE and &FILE read under sbl
    /// </summary>
    private void ProcessLineDirective(string directive)
    {
        var m = System.Text.RegularExpressions.Regex.Match(directive, @"^\s*LINE\s+(\d+)(?:\s+[""']([^""']*)[""'])?",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success)
        {
            _parent.LogCompilerException(247);
            return;
        }

        LineCountFile = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) - 1 - _continuation;
        if (m.Groups[2].Success)
        {
            _lineFileName = m.Groups[2].Value;
            _staleIncludeName = null;
        }
    }

    /// <summary>
    /// Processes an INCLUDE or COPY directive
    /// </summary>
    /// <param name="directive">The directive text</param>
    private void ProcessIncludeDirective(string directive)
    {
        // sbl skips a name it has already included, comparing the name as written, and opens it with a
        // blank before the closing quote dropped (a leading one kept): "line2.sno " is a second include
        var written = CompiledRegex.QuotePattern().Match(directive.Replace('\'', '\"')).Value.Replace("\"", "");
        var include = written.TrimEnd(' ');
        _staleIncludeName = null;

        if (_parent.IncludeList.Contains(written))
            return;

        _parent.IncludeList.Add(written);
        _includeDepth++;
        
        try
        {
            using var reader = new StreamReader(ResolveIncludePath(include), Encoding.UTF8);
            ReadFile(reader, include);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine(e.Message);
            _parent.LogCompilerException(285, 2);
        }
        finally
        {
            _includeDepth--;
        }
    }

    /// <summary>
    /// Finds an include file as SPITBOL does: the name as given (relative to the current
    /// directory), else each directory the SNOLIB environment variable lists, in order
    /// ("The environment variable SNOLIB may be set to a list of sub-directory paths to be
    /// searched for files given in an -INCLUDE control statement", SPITBOL manual). LOAD
    /// searches the same list.
    /// </summary>
    /// <param name="include">The include file name as written</param>
    /// <returns>The path to open; the name itself when no listed directory holds it</returns>
    private static string ResolveIncludePath(string include)
    {
        if (Path.IsPathRooted(include) || File.Exists(include))
            return include;

        var snolib = Environment.GetEnvironmentVariable("SNOLIB");
        if (string.IsNullOrEmpty(snolib))
            return include;

        foreach (var dir in snolib.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir, include);
            if (File.Exists(candidate))
                return candidate;
        }

        return include;
    }

    /// <summary>
    /// Processes a CASE directive for case-folding control
    /// </summary>
    /// <param name="directive">The directive text</param>
    // -CASE N: N nonzero folds names from this line on; N zero or absent keeps upper and lower apart
    private void ProcessCaseDirective(string directive)
    {
        var value = 0L;
        if (directive.Trim().Length > _caseDirectiveValueOffset - 1 &&
            (!long.TryParse(directive[(_caseDirectiveValueOffset - 1)..].Trim(), out value) || value < 0))
        {
            _parent.LogCompilerException(247);
            return;
        }

        _parent.BuildOptions.CaseFolding = value != 0;
    }

    internal bool CaseFoldingNow => _parent.BuildOptions.CaseFolding;

    #endregion

    #region String Parsing Utilities

    private static readonly List<string> _extensions =
    [
        "",
        ".sno",
        ".sbl",
        ".spt",
        ".spx"
    ];

    /// <summary>
    /// Splits a line by the specified delimiter characters, respecting string literals
    /// </summary>
    /// <param name="line">The line to split</param>
    /// <param name="delimiters">The delimiter characters</param>
    /// <returns>List of split segments, each keeping its trailing blanks (whether a blank last segment is a
    /// statement depends on the caller)</returns>
    private static List<string> SplitLineByDelimiter(string line, params char[] delimiters)
    {
        List<string> splitLine = [];
        var baseIndex = 0;

        for (var index = 0; index < line.Length; ++index)
        {
            switch (line[index])
            {
                case '"':
                case '\'':
                    index = SkipStringLiteral(line, index);
                    break;

                default:
                    if (delimiters.Contains(line[index]))
                    {
                        splitLine.Add(line[baseIndex..index]);
                        baseIndex = index + 1;
                    }
                    break;
            }
        }

        splitLine.Add(line[baseIndex..]);
        return splitLine;
    }

    /// <summary>
    /// Skips over a string literal in the line
    /// </summary>
    /// <param name="line">The line being processed</param>
    /// <param name="index">Current position in the line</param>
    /// <returns>Index after the string literal</returns>
    private static int SkipStringLiteral(string line, int index)
    {
        var m = CompiledRegex.StringLiteralPattern().Match(line[index..]);
        return m.Success ? m.Length + index - 1 : index;
    }

    #endregion
}