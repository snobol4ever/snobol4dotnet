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
    private const int _caseDirectiveMinLength = 6;
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
            foreach (var fileName in _parent.FilesToCompile.Select(AdjustFileExtension))
            {
                var directoryName = Path.GetDirectoryName(fileName);
                if (directoryName is not null)
                    Directory.SetCurrentDirectory(directoryName);

                var fullPath = Path.GetFullPath(fileName);
                PathList.Add(fullPath);
                using StreamReader sourceFileReader = new(fullPath, Encoding.UTF8);
                ReadFile(sourceFileReader, fullPath);
            }
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
        var lines = SplitLineByDelimiter(source, '\r', '\n');

        foreach (var line in lines)
        {
            LineCountFile++;
            ProcessLineWithSubLines(line, pathName);
        }
    }

    /// <summary>
    /// Reads and processes a source file
    /// </summary>
    /// <param name="reader">Stream reader for the source file</param>
    /// <param name="pathName">Full path to the source file</param>
    private void ReadFile(StreamReader reader, string pathName)
    {
        LineCountFile = 0;
        
        while (!EndFound && !reader.EndOfStream)
        {
            LineCountFile += _continuation;
            LineCountTotal++;
            var currentLine = ReadLineAndContinuations(reader, out var continuation);
            _continuation = continuation;
            ListSource(currentLine);
            
            ProcessLineWithSubLines(currentLine, pathName);
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
    private void ProcessLineWithSubLines(string line, string pathName)
    {
        SubLineCount = 0;
        var subLines = SplitLineByDelimiter(line, _semicolonChar);
        
        foreach (var subLine in subLines)
        {
            ProcessSubLine(subLine, pathName);
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
            Console.Error.WriteLine($"{LineCountTotal:0000} {LineCountFile:0000} {_includeDepth:00} {line}");
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
        
        var trimmedValue = m.Value.TrimEnd();
        return _parent.BuildOptions.CaseFolding 
            ? trimmedValue.Equals("END", StringComparison.OrdinalIgnoreCase) 
            : trimmedValue == "end";
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

            case "list":
                _parent.BuildOptions.ListSource = true;
                break;

            case "nolist":
                _parent.BuildOptions.ListSource = false;
                break;

            case "print":
            case "noprint":
            case "double":
            case "on":
            case "line":
            case "nooptimize":
            case "optimize":
            case "stitl":
            case "title":
                Console.Error.WriteLine($"WARNING: Directive -{match.Trim()} in {pathName} ignored");
                break;

            default:
                _parent.LogCompilerException(247);
                break;
        }
    }

    /// <summary>
    /// Processes an INCLUDE or COPY directive
    /// </summary>
    /// <param name="directive">The directive text</param>
    private void ProcessIncludeDirective(string directive)
    {
        var include = CompiledRegex.QuotePattern().Match(directive.Replace('\'', '\"')).Value.Replace("\"", "");

        if (_parent.IncludeList.Contains(include))
            return;

        _parent.IncludeList.Add(include);
        _includeDepth++;
        
        try
        {
            using var reader = new StreamReader(include, Encoding.UTF8);
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
    /// Processes a CASE directive for case-folding control
    /// </summary>
    /// <param name="directive">The directive text</param>
    private void ProcessCaseDirective(string directive)
    {
        if (directive.Length < _caseDirectiveMinLength || 
            !long.TryParse(directive[_caseDirectiveValueOffset..], out var value) || 
            value < 0)
        {
            _parent.LogCompilerException(247);
            return;
        }

        _parent.BuildOptions.CaseFolding = value != 0;
    }

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
    /// <returns>List of split segments</returns>
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
                        splitLine.Add(line[baseIndex..index].TrimEnd());
                        baseIndex = index + 1;
                    }
                    break;
            }
        }

        splitLine.Add(line[baseIndex..].TrimEnd());
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