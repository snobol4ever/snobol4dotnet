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

    // The unit being read, for the report of a statement in error: its physical lines (a continued statement has several), the
    // number SPITBOL gives the first statement on them, and where the current semicolon-delimited statement begins in the
    // joined text of those lines
    internal List<string> CurrentPhysicalLines { get; set; } = [];
    internal int CurrentFirstStatementNumber { get; set; }
    internal int CurrentSubLineOffset { get; set; }

    // What follows the current statement on its line when a semicolon ends it: the blanks before the semicolon and the semicolon,
    // which are no part of the statement's text but are what SPITBOL's scanner meets after its last element
    internal string CurrentSubLineTail { get; set; } = "";

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
    private bool _controlLineListed;     // an -INCLUDE has listed its own line
    private int _cardLine;               // the line the control statement being executed stands on, which -LINE does not move
    private int _maximumLineLength = int.MaxValue;   // the longest line the source can have, as -INnnn sets it

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
        var current = "";

        try
        {
            // Files are read until one holds END; the words after it are the program's arguments
            // (HOST(0), HOST(2,n)), as SPITBOL takes them
            var used = 0;
            foreach (var fileName in _parent.FilesToCompile.Select(AdjustFileExtension))
            {
                current = fileName;
                // the working directory stays where the program was started: relative file names and
                // includes resolve from there, as in SPITBOL, not from the source file's directory
                var fullPath = Path.GetFullPath(fileName);
                PathList.Add(fullPath);
                using StreamReader sourceFileReader = new(fullPath, Encoding.UTF8);
                // the file is named as it was given: &FILE and the error reports print that spelling, as SPITBOL does
                ReadFile(sourceFileReader, fileName);
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
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _parent.LogCannotOpen(current);
            return;
        }

        if (PathList.Count > 0 && EndFound)
            return;

        _parent.LogMissingEnd();
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
        ReadFile(reader, AdjustFileExtension(script));

        if (EndFound)
            return;

        _parent.LogMissingEnd();
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
            CurrentPhysicalLines = [line];
            CurrentFirstStatementNumber = 0;
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
            var currentLine = ReadLineAndContinuations(reader, out var continuation, out var physicalLines);
            _continuation = continuation;
            var statementsBefore = SourceLines.Count + BlankLineCount;
            var linesBefore = SourceLines.Count;
            CurrentPhysicalLines = physicalLines;
            CurrentFirstStatementNumber = statementsBefore + 1;

            ProcessLineWithSubLines(currentLine, _staleIncludeName ?? _lineFileName ?? pathName);

            // listed after it is read, so -LIST lists itself and -NOLIST does not (an -INCLUDE lists itself before its lines)
            if (_controlLineListed)
                _controlLineListed = false;
            else if (_parent.IncludeFailed && EndFound)
            {
                // the END that follows a failed include is not listed: sbl's source stack is still inside the include, whose end
                // is not the program's
            }
            else
                ListSource(physicalLines, SourceLines.Count + BlankLineCount > statementsBefore ? statementsBefore + 1 : 0, linesBefore);

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
    private string ReadLineAndContinuations(StreamReader reader, out int continuationLines, out List<string> physicalLines)
    {
        // a line is cut to the length -INnnn set, the rest of it is lost
        var line = CutToMaximum(reader.ReadLine());
        continuationLines = 1;
        physicalLines = [line ?? ""];

        if (line is null)
            return string.Empty;

        var currentLine = line;

        // A continuation joins a statement still open: not a comment or control line, nor one a final
        // semicolon has closed, as in SPITBOL, where a continuation after those is misplaced (error 214); nor the END
        // statement, after which SPITBOL reads no more of the program
        while (!reader.EndOfStream && reader.Peek() is _continuationChar or _alternateContinuationChar
               && !IsCommentOrContinuation(currentLine) && !IsCommand(currentLine)
               && !currentLine.TrimEnd().EndsWith(_semicolonChar) && !IsEndStatement(currentLine))
        {
            var continued = CutToMaximum(reader.ReadLine()) ?? "";
            physicalLines.Add(continued);
            currentLine += continued.Length > 0 ? " " + continued[1..] : "";
            continuationLines++;
        }

        return currentLine;
    }

    private string? CutToMaximum(string? line) => line is not null && line.Length > _maximumLineLength ? line[.._maximumLineLength] : line;

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
        CurrentSubLineTail = "";

        // In SNOBOL4, a comment line has '*' in column 1. Such lines must be
        // skipped BEFORE semicolon splitting — otherwise "* comment; key/value"
        // splits into "* comment" (skipped) and "key/value" (parsed as code, error 233).
        if (line.Length > 0 && line[0] == '*')
        {
            CommentContinuationDirectiveCount++;
            return;
        }

        // A control line is read whole, as SPITBOL reads it: a semicolon in it ends nothing
        if (!codeText && IsCommand(line))
        {
            SubLineCount++;
            CommentContinuationDirectiveCount++;
            ExecuteDirectives(line, pathName, false, 0);
            return;
        }

        // A continuation line with no open statement to continue is a statement in error, as in SPITBOL
        if (!codeText && line.Length > 0 && line[0] is _continuationChar or _alternateContinuationChar)
        {
            CurrentSubLineOffset = 0;
            var misplaced = new SourceLine(pathName, _includeDepth, line, this, _parent.BuildOptions.ErrorOnUnhandledFail)
            {
                CompileError = true,
                PendingError = 214
            };
            SourceLines.Add(misplaced);
            return;
        }

        // Each segment between semicolons is a statement, a blank one included, but nothing after a final
        // semicolon is none; a source line's trailing blanks are trimmed first (X = 1 ;  ends at X = 1), while
        // CODE text keeps them (CODE(' X = 1 ; ') has a blank second statement), as SPITBOL numbers them
        var subLines = SplitLineByDelimiter(line, _semicolonChar);
        if (subLines.Count > 1 && (codeText ? subLines[^1].Length == 0 : string.IsNullOrWhiteSpace(subLines[^1])))
            subLines.RemoveAt(subLines.Count - 1);

        var offset = 0;
        foreach (var subLine in subLines)
        {
            CurrentSubLineOffset = offset;
            CurrentSubLineTail = offset + subLine.Length < line.Length ? subLine[subLine.TrimEnd().Length..] + ";" : "";

            // a comment may follow a semicolon in a source file, and runs to the end of the line, semicolons and all
            if (!codeText && offset > 0 && subLine.Length > 0 && subLine[0] == '*')
            {
                CommentContinuationDirectiveCount++;
                return;
            }

            // a control may follow a semicolon in a source file, and is read to the end of the line; what follows it is lost
            if (!codeText && IsCommand(subLine))
            {
                SubLineCount++;
                CommentContinuationDirectiveCount++;
                ExecuteDirectives(line[offset..], pathName, false, offset);
                return;
            }

            ProcessSubLine(subLine.TrimEnd(), pathName, codeText);
            offset += subLine.Length + 1;
        }
    }

    /// <summary>
    /// Processes a single sub-line (part of a semicolon-delimited line)
    /// </summary>
    /// <param name="subLine">The sub-line to process</param>
    /// <param name="pathName">Path to the source file</param>
    private void ProcessSubLine(string subLine, string pathName, bool codeText)
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

        // in the text of CODE a control follows a semicolon; in a source file only a line's first character begins one
        if (codeText && IsCommand(subLine))
        {
            CommentContinuationDirectiveCount++;
            ExecuteDirectives(subLine, pathName, true, 0);
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
    // SPITBOL's listing line: a line that begins a statement carries its number in an eight-column field,
    // any other line (comment, control, continuation) eight blanks; a line of an included file carries the
    // depth of the include as well, in the sixth column
    // SPITBOL lists a line as it compiles it, so a statement's errors follow its line: the lines are kept, with the statement they
    // stand before, until the lexer reaches it (Builder.Defer)
    private void ListSource(List<string> physicalLines, int statementNumber, int position)
    {
        if (!_parent.BuildOptions.ListSource)
            return;

        for (var i = 0; i < physicalLines.Count; i++)
        {
            var field = NumberField(i == 0 ? statementNumber : 0, _includeDepth);
            // a source line's trailing blanks are trimmed, as SPITBOL reads it
            var text = $"{field}{physicalLines[i].TrimEnd(' ', '\t')}";
            _parent.Defer(position, () => _parent.WriteListingLine(text));
        }
    }

    // The eight columns before a listed line: its statement number (none for a line that is no statement) and, inside an included
    // file, the depth of the include in the sixth column ("12   1  ", "     1  ")
    internal static string NumberField(int statementNumber, int includeDepth)
    {
        var number = statementNumber > 0 ? statementNumber.ToString(CultureInfo.InvariantCulture) : "";
        return includeDepth > 0 ? $"{number,-5}{includeDepth}".PadRight(8) : $"{number,-8}";
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

    // What SPITBOL's scane makes of the next element of a control statement
    private enum CardElement { End, Name, Integer, Real, String, Comma, Other }

    private readonly record struct CardToken(CardElement Kind, string Text = "", long Integer = 0, double Real = 0);

    // A control statement being read: SPITBOL's scnpt, the offset of what is read next, and scnse, where the element read last
    // began, under which an error in the statement is reported
    private sealed class ControlCard(string text, int offset)
    {
        internal readonly string Text = text;
        internal readonly int Offset = offset;
        internal int Position = 1;
        internal int Start = 1;
        internal int Error;

        internal CardToken Scan()
        {
            Error = 0;
            while (Position < Text.Length && Text[Position] is ' ' or '\t')
                Position++;
            if (Position >= Text.Length)
                return new CardToken(CardElement.End);

            Start = Position;
            var c = Text[Position];
            if (char.IsAsciiLetter(c))
            {
                var begin = Position;
                while (Position < Text.Length && (char.IsAsciiLetterOrDigit(Text[Position]) || Text[Position] is '_' or '.'))
                    Position++;
                // a name and a left paren is a call of a function, no control's name
                if (Position < Text.Length && Text[Position] == '(')
                {
                    Position++;
                    return new CardToken(CardElement.Other);
                }

                return new CardToken(CardElement.Name, Text[begin..Position]);
            }

            if (char.IsAsciiDigit(c))
            {
                var begin = Position;
                while (Position < Text.Length && (char.IsAsciiLetterOrDigit(Text[Position]) || Text[Position] is '_' or '.' or '+' or '-'))
                    Position++;
                var number = Text[begin..Position];
                if (long.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var integer))
                    return new CardToken(CardElement.Integer, number, integer);
                if (CompiledRegex.RealConstantPattern().IsMatch(number) &&
                    double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var real))
                    return new CardToken(CardElement.Real, number, 0, real);
                Error = 231;
                return new CardToken(CardElement.Other);
            }

            if (c is '\'' or '"')
            {
                var end = Text.IndexOf(c, Position + 1);
                if (end < 0)
                {
                    Position = Text.Length;
                    Error = 232;
                    return new CardToken(CardElement.Other);
                }

                var text = Text[(Position + 1)..end];
                Position = end + 1;
                return new CardToken(CardElement.String, text);
            }

            Position++;
            return new CardToken(c == ',' ? CardElement.Comma : CardElement.Other);
        }
    }

    /// <summary>
    /// Executes a control statement as SPITBOL's cncrd does. A control is told by the first four letters of its name (-NOEXECUTE,
    /// -NOEXEC and -NOEXECS are one control), a name SPITBOL does not know is no error, and a number, a string, a call of a function or a
    /// comma where a name should be is error 247. After a comma comes another control, but after -CASE, -DUMP, -EJECT, -INCLUDE,
    /// -LINE, -SPACE, -STITL and -TITLE nothing is read: the rest of the line is no one's business.
    /// </summary>
    /// <param name="line">The control statement, with the minus that begins it</param>
    /// <param name="pathName">Path to the source file</param>
    /// <param name="codeText">Whether it stands in the text of CODE rather than in a source file</param>
    /// <param name="offset">Where on its line the statement begins, for the column of an error</param>
    private void ExecuteDirectives(string line, string pathName, bool codeText, int offset)
    {
        var card = new ControlCard(line.TrimEnd(), offset);
        _cardLine = 1 + LineCountFile;
        void Reject() => RejectCard(card, pathName, codeText);

        while (card.Position < card.Text.Length)
        {
            if (card.Text.Length - card.Position > 2 && card.Text[card.Position] is 'I' or 'i' && card.Text[card.Position + 1] is 'N' or 'n'
                && char.IsAsciiDigit(card.Text[card.Position + 2]))
            {
                // -INnnn, the longest line the source can have: the lines after it are cut to that length
                card.Position += 2;
                var length = card.Scan();
                if (card.Error != 0 || !TrySmallInteger(length, out var maximum))
                {
                    Reject();
                    return;
                }

                _maximumLineLength = (int)maximum;
            }
            else
            {
                var name = card.Scan();
                if (card.Error != 0 || name.Kind != CardElement.Name)
                {
                    Reject();
                    return;
                }

                if (!ExecuteControl(card, name.Text, pathName, codeText))
                    return;
            }

            // another control follows a comma
            var next = card.Scan();
            if (card.Error != 0)
            {
                Reject();
                return;
            }

            if (next.Kind != CardElement.Comma)
                return;
        }
    }

    /// <summary>
    /// Does one control. True when another may follow it on the line, false when the line is done, with its error reported if it had one.
    /// </summary>
    private bool ExecuteControl(ControlCard card, string written, string pathName, bool codeText)
    {
        // names fold only while &CASE is not zero, so that -list is a control then and not otherwise; a name shorter than four letters
        // is none
        var name = written.Length < 4 ? "" : written[..4];
        if (_parent.BuildOptions.CaseFolding)
            name = name.ToUpperInvariant();

        switch (name)
        {
            case "CASE":
            {
                // the number is optional: no number is zero
                var value = 0L;
                var number = card.Scan();
                if (card.Error != 0 || number.Kind != CardElement.End && !TrySmallInteger(number, out value))
                {
                    RejectCard(card, pathName, codeText);
                    return false;
                }

                _parent.BuildOptions.CaseFolding = value != 0;
                return false;
            }

            case "COPY":
            case "INCL":
            {
                var file = card.Scan();
                if (card.Error != 0 || file.Kind != CardElement.String)
                {
                    RejectCard(card, pathName, codeText);
                    return false;
                }

                ProcessIncludeDirective(file.Text, pathName);
                return false;
            }

            case "LINE":
            {
                // -LINE n ["file"]: the next line is line n (of file, when named), the lines after it following on, as &LINE and
                // &FILE read under sbl; the number is set before the file name is read
                var number = card.Scan();
                if (card.Error != 0 || number.Kind != CardElement.Integer || number.Integer <= 0)
                {
                    RejectCard(card, pathName, codeText);
                    return false;
                }

                LineCountFile = (int)Math.Min(number.Integer, int.MaxValue) - 1 - _continuation;
                var file = card.Scan();
                if (card.Error != 0 || file.Kind is not (CardElement.End or CardElement.String))
                {
                    RejectCard(card, pathName, codeText);
                    return false;
                }

                if (file.Kind == CardElement.String)
                {
                    _lineFileName = file.Text;
                    _staleIncludeName = null;
                }

                return false;
            }

            case "SPAC":
            {
                // blank lines in the listing, which a card without a listing does not read
                if (!_parent.BuildOptions.ListSource)
                    return false;
                var number = card.Scan();
                if (card.Error != 0 || number.Kind != CardElement.End && !TrySmallInteger(number, out _))
                {
                    RejectCard(card, pathName, codeText);
                    return false;
                }

                return false;
            }

            // listing controls, which shape only SPITBOL's listing page
            case "DUMP":
            case "EJEC":
            case "STIT":
            case "TITL":
                return false;

            case "FAIL":
                _parent.BuildOptions.ErrorOnUnhandledFail = true;
                return true;

            case "NOFA":
                _parent.BuildOptions.ErrorOnUnhandledFail = false;
                return true;

            // -NOEXECUTE compiles and does not run, as the -n switch; the last of the two in the program wins, as in SPITBOL
            case "EXEC":
                _parent.BuildOptions.SuppressExecution = false;
                return true;

            case "NOEX":
                _parent.BuildOptions.SuppressExecution = true;
                return true;

            // -ERRORS runs the program despite compilation errors, as -k does; -NOERRORS, the default, does not run it
            case "ERRO":
                _parent.BuildOptions.RunDespiteCompileErrors = true;
                return true;

            case "NOER":
                _parent.BuildOptions.RunDespiteCompileErrors = false;
                return true;

            case "LIST":
                _parent.BuildOptions.ListSource = true;
                return true;

            case "NOLI":
                _parent.BuildOptions.ListSource = false;
                return true;

            // the others (-DOUBLE, -SINGLE, -PRINT, -NOPRINT, -OPTIMISE, -NOOPT, -TRACE) and a name SPITBOL does not know change nothing
            default:
                return true;
        }
    }

    // SPITBOL's gtsmi on what a control reads as a number: a non-negative integer that fits in a word; a real is cut to one and a
    // string is read as the number it spells (the null string as zero)
    private static bool TrySmallInteger(CardToken token, out long value)
    {
        value = 0;
        switch (token.Kind)
        {
            case CardElement.Integer:
                value = token.Integer;
                break;
            case CardElement.Real:
                value = (long)Math.Truncate(token.Real);
                break;
            case CardElement.String:
                var text = token.Text.Trim();
                if (text.Length == 0)
                    return true;
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var real))
                    return false;
                value = (long)Math.Truncate(real);
                break;
            default:
                return false;
        }

        return value is >= 0 and <= int.MaxValue;
    }

    // The error a control statement has at the element read last: the syntax error the scan found, else 247
    private void RejectCard(ControlCard card, string pathName, bool codeText) =>
        ControlCardError(card.Error != 0 ? card.Error : 247, card.Offset + card.Start, pathName, codeText);

    // The error of a control statement: on the listing page in a source file, under the first character of the element it was found
    // at, and the run does not start; in the text of CODE, it is the error of CODE
    private void ControlCardError(int code, int start, string pathName, bool codeText)
    {
        if (codeText)
        {
            _parent.LogCompilerException(code);
            return;
        }

        var physical = CurrentPhysicalLines.Count > 0 ? CurrentPhysicalLines[0] : "";
        _parent.LogControlCardError(code, physical, start + 1, _cardLine, pathName, SourceLines.Count, _includeDepth);
        // the page has listed the line
        _controlLineListed = true;
    }

    /// <summary>
    /// Includes a file: an -INCLUDE or -COPY control
    /// </summary>
    /// <param name="written">The file name as it was written between the quotes</param>
    private void ProcessIncludeDirective(string written, string pathName)
    {
        // sbl skips a name it has already included, comparing the name as written, and opens it with a
        // blank before the closing quote dropped (a leading one kept): "line2.sno " is a second include
        var include = written.TrimEnd(' ');
        _staleIncludeName = null;

        if (_parent.IncludeList.Contains(written))
            return;

        _parent.IncludeList.Add(written);
        var physicalLines = CurrentPhysicalLines;
        var statementsBefore = SourceLines.Count;

        // the control line is listed before the lines it brings in, not after them
        ListSource(physicalLines, 0, statementsBefore);
        var listed = _parent.BuildOptions.ListSource;
        _includeDepth++;
        
        try
        {
            using var reader = new StreamReader(ResolveIncludePath(include), Encoding.UTF8);
            ReadFile(reader, include);
            _includeDepth--;
        }
        catch (IOException)
        {
            // sbl reports it on the listing page, in its place among the statements' errors: the control line, the caret under
            // the opening quote of the name. The include is left open in sbl's source stack: the statements after it list at
            // depth 1 and its END is never seen, so the run ends "No END statement found" (see Builder.LogControlLineError)
            var physical = physicalLines.Count > 0 ? physicalLines[0] : "";
            var quote = physical.IndexOfAny(['\'', '"']);
            _parent.LogControlLineError(285, physical, quote >= 0 ? quote + 1 : 0, 1 + LineCountFile, pathName, statementsBefore,
                _includeDepth - 1, listed);
        }
        finally
        {
            // the line has been listed: ReadFile does not list it again
            _controlLineListed = true;
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

    internal bool CaseFoldingNow => _parent.BuildOptions.CaseFolding;
    internal bool ListingOn => _parent.BuildOptions.ListSource;

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