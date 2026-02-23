using System.Text;

namespace Snobol4.Common;

public class SourceCode
{
    #region Members

    internal int LineCountFile;                    // Counter for number of lines in a file
    internal int LineCountTotal;                   // Counter for numbers of lines in all files
    internal int SubLineCount;                     // Counter for semicolon delimited lines
    internal List<SourceLine> SourceLines = [];    // ListSource of source lines
    internal Dictionary<string, int> Labels = [];  // Dictionary associating labels to line numbers
    private readonly Builder _parent;              // Builder invoking this class
    private int _includeDepth;                     // Depth of INCLUDE statements
    internal List<string> PathList = [];           // List of source paths
    internal bool EndFound;                        // True if an end statement was found

    #endregion

    #region Constructor

    internal SourceCode(Builder parent)
    {
        _parent = parent;
    }

    #endregion

    #region Internal Methods

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

        _parent.LogCompilerException(216, 0);
    }

    // For testing purposes
    public void ReadTestScript(MemoryStream scriptStream)
    {
        EndFound = false;
        Labels = new Dictionary<string, int>();
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

    internal void ReadCodeInString(string source, string pathName)
    {
        LineCountFile = 1;
        LineCountTotal++;
        SubLineCount = 0;

        var lines = SplitLineByReturns(source);

        foreach (var line in lines)
        {
            LineCountFile++;
            var subLines = SplitLineBySemicolons(line);
            foreach (var subLine in subLines.Where(subLine => subLine.Trim() != ""))
            {
                SubLineCount++;

                switch (subLine[0])
                {
                    // Ignore comments;
                    case '*':
                        continue;

                    // Compiler directive
                    case '-':
                        ExecuteDirectives(subLine[1..], pathName);
                        continue;
                }

                SourceLines.Add(new SourceLine(pathName, _includeDepth, subLine, this, _parent.BuildOptions.ErrorOnUnhandledFail));
            }
        }
    }

    #endregion

    #region Private Methods

    private void ReadFile(StreamReader reader, string pathName)
    {
        LineCountFile = 0;
        while (!EndFound && !reader.EndOfStream)
        {
            LineCountFile++;
            LineCountTotal++;
            var currentLine = ReadLineAndContinuations(reader);
            ListSource(currentLine);
            SubLineCount = 0;
            var subLines = SplitLineBySemicolons(currentLine);
            foreach (var subLine in subLines.Where(subLine => subLine.Trim() != "").Where(subLine => !IsCommentOrContinuation(subLine)))
            {
                if (IsCommand(subLine))
                {
                    ExecuteDirectives(subLine[1..], pathName);
                    continue;
                }

                SourceLines.Add(new SourceLine(pathName, _includeDepth, subLine, this, _parent.BuildOptions.ErrorOnUnhandledFail));

                if (!IsEndStatement(subLine))
                    continue;
                
                _parent.EntryLabel = ProcessEntry(subLine, currentLine);
                return;
            }
        }
    }

    private static readonly List<string> _extensions =
    [
        "",
        ".sno",
        ".sbl",
        ".spt",
        ".spx"
    ];

    private static string AdjustFileExtension(string file)
    {
        var extension = Path.GetExtension(file);

        if (!string.IsNullOrEmpty(extension))
            return file;

        foreach (var ext in _extensions.Where(ext => new FileInfo(file + ext).Exists))
            return Path.ChangeExtension(file, ext);

        return file;
    }

    private bool IsEndStatement(string line)
    {
        var m = CompiledRegex.EndPattern().Match(line);
        if (!m.Success)
            return false;
        return _parent.BuildOptions.CaseFolding ? m.Value.TrimEnd().ToUpper() == "END" : m.Value.TrimEnd() == "end";
    }


    private string ProcessEntry(string subLine, string line)
    {
        EndFound = true;
        if (subLine.Trim().Length == 3)
            return "";

        var m = CompiledRegex.EntryLabelPattern().Match(subLine[3..]);

        if (!m.Success)
            _parent.LogCompilerException(215);

        return m.Groups[2].Value;
    }

    private void ListSource(string line)
    {
        if (_parent.BuildOptions.ListSource)
            Console.Error.WriteLine(@$"{LineCountTotal:0000} {LineCountFile:0000} {_includeDepth:00} {line}");
    }

    private void ExecuteDirectives(string line, string pathName)
    {
        var directiveList = line.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var directive in directiveList)
        {
            var m = CompiledRegex.KeywordPattern().Match(directive);

            if (!m.Success)
                continue;

            ParseDirective(m.Value, directive, pathName);
        }
    }

    private void ParseDirective(string match, string directive, string pathName)
    {
        switch (match.ToLower().Trim())
        {
            case "copy":
            case "include":
                var include = CompiledRegex.QuotePattern().Match(directive.Replace('\'', '\"')).Value.Replace("\"", "");

                // Ignore directive when include file has previously been loaded
                if (_parent.IncludeList.Contains(include))
                    break;

                _parent.IncludeList.Add(include);
                ++_includeDepth;
                try
                {
                    using var reader = new StreamReader(include, Encoding.UTF8);
                    ReadFile(reader, include);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine(e.Message);
                    _parent.LogCompilerException(285, 2);
                    return;
                }

                --_includeDepth;
                break;

            case "case":
                if (directive.Length < 6 || !long.TryParse(directive[5..], out var i) || i < 0)
                {
                    _parent.LogCompilerException(247, 0);
                    return;
                }

                _parent.BuildOptions.CaseFolding = i != 0;
                break;

            case "fail":
                _parent.BuildOptions.ErrorOnUnhandledFail = true;
                break;

            case "list":
                _parent.BuildOptions.ListSource = true;
                break;

            case "nolist":
                _parent.BuildOptions.ListSource = false;
                break;

            case "nofail":
                _parent.BuildOptions.ErrorOnUnhandledFail = false;
                break;

            case "print":
            case "noprint":
            case "double": // Can be managed by editor
            case "on": // Can be manager by editor
            case "line":
            case "nooptimize":
            case "optimize":
            case "stitl": // Can be managed with comments
            case "title": // Can be managed with comments
                Console.Error.WriteLine($@"WARNING: Directive -{match.Trim()} in {pathName} ignored");
                break;

            default:
                _parent.LogCompilerException(247);
                break;
        }
    }

    private static string ReadLineAndContinuations(StreamReader reader)
    {
        var line = reader.ReadLine();

        if (line == null)
            return "";

        var currentLine = line;

        while (!reader.EndOfStream && (reader.Peek() == '.' || reader.Peek() == '+'))
        {
            line = reader.ReadLine();
            currentLine += line[1..];
        }

        return currentLine;
    }

    private static List<string> SplitLineByReturns(string line)
    {
        List<string> splitLine = [];
        var baseIndex = 0;

        for (var index = 0; index < line.Length; ++index)
        {
            switch (line[index])
            {
                case '"':
                case '\'':
                    // Ignore semicolons in string literals
                    // Unbalanced quotes are detected by lexer
                    index = SkipStringLiteral(line, index);
                    break;

                case '\r':
                case '\n':
                    splitLine.Add(line[baseIndex..index].TrimEnd());
                    index++;
                    baseIndex = index + 1;
                    break;
            }
        }

        splitLine.Add(line[baseIndex..].TrimEnd());
        return splitLine;
    }

    private static List<string> SplitLineBySemicolons(string line)
    {
        List<string> splitLine = [];
        var baseIndex = 0;

        for (var index = 0; index < line.Length; ++index)
        {
            switch (line[index])
            {
                case '"':
                case '\'':
                    // Ignore semicolons in string literals
                    // Unbalanced quotes are detected by lexer
                    index = SkipStringLiteral(line, index);
                    break;

                case ';':
                    splitLine.Add(line[baseIndex..index].TrimEnd());
                    baseIndex = index + 1;
                    break;
            }
        }

        splitLine.Add(line[baseIndex..].TrimEnd());
        return splitLine;
    }

    private static int SkipStringLiteral(string line, int index)
    {
        var m = CompiledRegex.StringLiteralPattern().Match(line[index..]);
        if (!m.Success)
            return index;
        return m.Length + index - 1;
    }

    private static bool IsCommentOrContinuation(string line)
    {
        return line.Length != 0 && "+.*".Contains(line[0]);
    }

    private static bool IsCommand(string line)
    {
        return line.Length != 0 && "-".Contains(line[0]);
    }

    #endregion
}