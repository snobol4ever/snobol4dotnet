using System.Text;

namespace Snobol4.Common;

public partial class Builder
{
    /// <summary>
    /// Reports an unexpected programming error to the error output stream and logs it to the error history.
    /// </summary>
    /// <param name="e">The exception that occurred.</param>
    private void ReportProgrammingError(Exception e)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("*** UNEXPECTED EXCEPTION ***");
        Console.Error.WriteLine();
        
        SaveAndWriteExceptionRecursive(e);
        
        Console.Error.WriteLine();
        Console.Error.WriteLine("Stack Trace:");
        Console.Error.WriteLine(e.StackTrace);
    }

    /// <summary>
    /// Saves and writes an exception to the error history and output stream.
    /// </summary>
    /// <param name="e">The exception to save and write.</param>
    private void SaveAndWriteException(Exception e)
    {
        SaveException(e);
        WriteException(e);
    }

    /// <summary>
    /// Recursively saves and writes an exception and all its inner exceptions.
    /// </summary>
    /// <param name="e">The exception to save and write.</param>
    private void SaveAndWriteExceptionRecursive(Exception e)
    {
        SaveAndWriteException(e);
        
        if (e.InnerException is not null)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("--- Inner Exception ---");
            SaveAndWriteExceptionRecursive(e.InnerException);
        }
    }

    /// <summary>
    /// Clears all exception history data.
    /// </summary>
    private void ClearExceptionHistory()
    {
        ErrorCodeHistory.Clear();
        ColumnHistory.Clear();
        MessageHistory.Clear();
    }

    /// <summary>
    /// Saves a compiler exception to the error history.
    /// </summary>
    /// <param name="ce">The compiler exception to save.</param>
    internal void SaveExceptionHistory(CompilerException ce)
    {
        ErrorCodeHistory.Add(ce.Code);
        ColumnHistory.Add(ce.Column);
        MessageHistory.Add(ce.Message);
    }

    /// <summary>
    /// Logs a compiler exception with a specified error code and cursor position.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position in the source line.</param>
    public void LogCompilerException(int code, int cursorCurrent = 0) =>
        LogCompilerExceptionCore(code, cursorCurrent, CompilerException.ErrorMessage[code]);

    /// <summary>
    /// Logs a compiler exception with source line context, displaying the error with a caret pointer.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position in the source line.</param>
    /// <param name="source">The source line where the error occurred.</param>
    public void LogCompilerException(int code, int cursorCurrent, SourceLine source)
    {
        FirstErrorSource ??= source;
        LogCompilerExceptionCore(code, cursorCurrent, BuildErrorMessage(code, cursorCurrent, source), source);
    }

    // The first statement a compile found in error, reset by whoever needs it (CODE numbers only the
    // statements before it)
    internal SourceLine? FirstErrorSource;

    /// <summary>
    /// Core logic for logging a compiler exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position.</param>
    /// <param name="message">The formatted error message.</param>
    private void LogCompilerExceptionCore(int code, int cursorCurrent, string message, SourceLine? source = null, Action? report = null)
    {
        if (Execute is not null)
        {
            Execute.Failure = true;
        }

        var ce = new CompilerException(code, cursorCurrent, message);
        SaveExceptionHistory(ce);
        if (QuietCompile)
            return;
        // an error in a statement is reported on the listing page, as sbl does; one with no statement stays on standard error
        // as sbl's does ("No END statement found")
        if (source is not null)
            WriteStatementInError(code, cursorCurrent, source);
        else if (report is not null)
            report();
        else
            Console.Error.WriteLine(message);
    }

    // SPITBOL's report of a statement in error, on the listing page as sbl writes it (CEO-1858): the physical line the error was
    // found at, with the number of its statement in an eight-column field and its tabs kept; a line that keeps the tabs and puts
    // ! under the error column (empty when the error has none); "file(line,col) : ERROR nnn -- text"; and an empty line. A line
    // that -LIST has already listed is not listed again. The joined text of a continued statement has a blank for the first character
    // of each continuation line, and an error found at the end of the text is reported at its last character, as sbl does.
    private void WriteStatementInError(int code, int cursor, SourceLine source)
    {
        var lines = source.PhysicalLines;
        if (lines.Count == 0)
            lines = [source.Text];
        var index = 0;
        var column = 0;
        if (cursor > 0)
        {
            if (source.DeferredExpression)
                cursor -= 2;
            var total = lines.Sum(line => line.Length);
            var at = Math.Clamp(source.SubLineOffset + cursor, 0, Math.Max(0, total - 1));
            var start = 0;
            var length = lines[0].Length;
            while (at >= start + length && index + 1 < lines.Count)
            {
                start += length;
                index++;
                length = lines[index].Length;
            }
            column = at - start + 1;
        }

        WriteErrorPage(SourceCode.NumberField(source.FirstStatementNumber, source.IncludeDepth), lines[index], 1 + source.LineCountFile + index, column,
            code, source.PathName, source.Listed);
    }

    // An error in a control line (an -INCLUDE of a file that cannot be opened) is reported on the same page: no statement number,
    // and the caret under `column`, the opening quote of the name
    // The reports of control lines that went wrong, kept until the lexer reaches the statement they stand before: SPITBOL compiles
    // as it reads, so its errors list in the order of the source, and the source is read before it is lexed here
    private readonly List<(int Position, Action Report)> _deferred = [];

    // Keeps a report of the source, a listed line or the page of a control line that went wrong, until the lexer reaches statement
    // `position`
    internal void Defer(int position, Action report) => _deferred.Add((position, report));

    // Takes back a kept report that is not wanted after all
    internal void DropDeferred(Action? report)
    {
        if (report is not null)
            _deferred.RemoveAll(kept => ReferenceEquals(kept.Report, report));
    }

    internal void LogControlLineError(int code, string physical, int column, int line, string file, int position, int depth, bool listed)
    {
        // an include that cannot be opened ends the run before it starts, whatever -k says; SPITBOL's source stack keeps the
        // include open, so its END is never seen and the run ends "No END statement found" once the rest is compiled
        SourceFailed = true;
        IncludeFailed = true;
        ExitStatus = 1;
        Defer(position, () =>
            LogCompilerExceptionCore(code, column, CompilerException.ErrorMessage[code], null,
                () => WriteErrorPage(SourceCode.NumberField(0, depth), physical, line, column, code, file, listed)));
    }

    // An error in a control statement is reported on the page of the line, the caret under the first character of the element it was
    // found at; the compile goes on, but the program does not run
    internal void LogControlCardError(int code, string physical, int column, int line, string file, int position, int depth) =>
        Defer(position, () =>
            LogCompilerExceptionCore(code, column, CompilerException.ErrorMessage[code], null,
                () => WriteErrorPage(SourceCode.NumberField(0, depth), physical, line, column, code, file, false)));

    // Runs the kept reports that stand before statement `before` (all of them for int.MaxValue), in the order they were kept
    internal void FlushDeferred(int before)
    {
        while (_deferred.Count > 0 && _deferred[0].Position <= before)
        {
            var report = _deferred[0].Report;
            _deferred.RemoveAt(0);
            report();
        }
    }

    // sbl's last word after a failed include, on standard error once everything has been compiled and listed
    internal void ReportNoEndAfterFailedInclude()
    {
        if (!IncludeFailed)
            return;
        IncludeFailed = false;
        MissingEndPending = false;   // the one line says both
        // sbl says it only when END ends the file: a line after it, a blank one too, is something its stack, still inside the include,
        // has to read, and it says nothing
        if (LinesAfterEnd.Count > 0)
            return;
        Console.Error.WriteLine("No END statement found in source file(s).");
    }

    // sbl's line on standard error for source that holds no END; the run does not start. It is sbl's last word, after the listing and
    // the errors of the statements it did compile, so it is kept until the lexer has finished (ReportMissingEnd).
    internal void LogMissingEnd()
    {
        SourceFailed = true;
        ExitStatus = 1;
        MissingEndPending = true;
        LogCompilerExceptionCore(216, 0, "No END statement found in source file(s).", null, () => { });
    }

    internal bool MissingEndPending;

    internal void ReportMissingEnd()
    {
        if (!MissingEndPending)
            return;
        MissingEndPending = false;
        Console.Error.WriteLine("No END statement found in source file(s).");
    }

    // sbl's line on standard error for a source file that cannot be opened, named as it was given; the run does not start
    internal void LogCannotOpen(string file)
    {
        SourceFailed = true;
        ExitStatus = 1;
        Console.Error.WriteLine($"Can't open {file}");
    }

    // SPITBOL copies the lines of an error report to its interactive channel, standard error, as it prints them (prtis in its sources:
    // ERRORS is in its default flags), unless its standard printer is the terminal (PRTICH, from the test of standard output). Run
    // with standard output in a file or a pipe it therefore writes each such line twice, and a harness that merges the two streams
    // reads it twice.
    internal bool CopyErrorsToStandardError => !BuildOptions.ErrorsToStdout && (ListingPath != "" || Console.IsOutputRedirected);

    // One line of an error report on the listing and, when `copy` and the copy is wanted, straight after it on standard error
    internal void WriteReportLine(TextWriter listing, string text, bool copy)
    {
        listing.WriteLine(text);
        listing.Flush();
        if (copy && CopyErrorsToStandardError)
        {
            Console.Error.WriteLine(text);
            Console.Error.Flush();
        }
    }

    // The page of one error: the physical line under its number field, the line that keeps its tabs and puts ! under `column`
    // (empty when there is none), the error line, and an empty line
    private void WriteErrorPage(string numberField, string physical, int line, int column, int code, string file, bool listed)
    {
        // a statement that -LIST has already listed is not listed again, but its line is still copied
        WriteReportPageLine($"{numberField}{physical.TrimEnd(' ', '\t')}", listed, newPageAllowed: true);
        // under -e, errors to the list file only, sbl leaves an empty line before the pointer, which the page does not count
        if (BuildOptions.ErrorsToStdout)
            WriteReportPageLine("", counted: false);

        var pointer = new StringBuilder();
        if (column > 0)
        {
            pointer.Append(' ', 8);
            for (var i = 0; i < column - 1; i++)
                pointer.Append(i < physical.Length && physical[i] == '\t' ? '\t' : ' ');
            pointer.Append('!');
        }
        WriteReportPageLine(pointer.ToString());
        var where = column > 0 ? $"{line},{column}" : $"{line}";
        WriteReportPageLine($"{file}({where}) : ERROR {code:D3} -- {CompilerException.ErrorMessage[code]}");
        WriteReportPageLine("");
    }

    // A line of an error page: on the listing, which opens its head if this is the first line, and copied to standard error. Only
    // the line that opens a report may begin a new page: sbl breaks between the reports, never inside one
    private void WriteReportPageLine(string text, bool listed = false, bool newPageAllowed = false, bool counted = true)
    {
        if (!listed)
            WriteListingLine(text, newPageAllowed, counted);
        ListingWriter.Flush();
        WriteReportCopy(text);
    }

    private void WriteReportCopy(string text)
    {
        if (!CopyErrorsToStandardError)
            return;
        // the copy is cut at the width of the listing too, and of a line cut into pieces only the last is copied
        Console.Error.WriteLine(WrapLine(text, BuildOptions.PageWidth)[^1]);
        Console.Error.Flush();
    }

    // Set while EVAL compiles its argument: a compile error there makes EVAL fail, as in SPITBOL, and
    // prints nothing
    internal bool QuietCompile;

    /// <summary>
    /// Builds a formatted error message with source context and error location pointer.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position in the source line.</param>
    /// <param name="source">The source line where the error occurred.</param>
    /// <returns>A formatted error message string.</returns>
    private static string BuildErrorMessage(int code, int cursorCurrent, SourceLine source)
    {
        var (formattedSource, adjustedCursor) = FormatSourceLine(source, cursorCurrent);
        var fileName = Path.GetFileName(source.PathName);
        var errorMessage = CompilerException.ErrorMessage[code];
        var (codeCount, listCount, lineCount) = CalculateLineCounts(source);

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(formattedSource);

        if (adjustedCursor > 0)
        {
            sb.Append(' ', adjustedCursor);
            sb.AppendLine("!");
        }

        sb.AppendLine($"{fileName}({codeCount}/{listCount}/{lineCount}): error {code} -- {errorMessage}");
        return sb.ToString();
    }

    /// <summary>
    /// Formats the source line for display, handling deferred expressions.
    /// </summary>
    /// <param name="source">The source line to format.</param>
    /// <param name="cursorPosition">The original cursor position.</param>
    /// <returns>A tuple containing the formatted source text and adjusted cursor position.</returns>
    private static (string FormattedText, int AdjustedCursor) FormatSourceLine(SourceLine source, int cursorPosition)
    {
        var str = source.Text.Replace('\t', ' ');
        return source.DeferredExpression
            ? ($" {str[3..^1]}", cursorPosition - 1)
            : (str, cursorPosition);
    }

    /// <summary>
    /// Calculates the various line counts from the source line.
    /// </summary>
    /// <param name="source">The source line.</param>
    /// <returns>A tuple containing code count, list count, and line count.</returns>
    private static (int CodeCount, int ListCount, int LineCount) CalculateLineCounts(SourceLine source)
    {
        var codeCount = 1 + source.LineCountFile - source.BlankLineCount - source.CommentContinuationDirectiveCount;
        var listCount = 1 + source.LineCountFile - source.CommentContinuationDirectiveCount;
        var lineCount = 1 + source.LineCountFile;

        return (codeCount, listCount, lineCount);
    }

    /// <summary>
    /// Saves an exception to the error history (excludes CompilerExceptions).
    /// </summary>
    /// <param name="e">The exception to save.</param>
    public void SaveException(Exception e)
    {
        if (e is CompilerException)
        {
            return;
        }

        ErrorCodeHistory.Add(_unexpectedExceptionErrorCode);
        ColumnHistory.Add(0);
        MessageHistory.Add(e.Message);
    }

    /// <summary>
    /// Writes an exception message to the error output stream.
    /// </summary>
    /// <param name="e">The exception to write.</param>
    public void WriteException(Exception e)
    {
        if (e is CompilerException ce)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(ce.Message);
            Console.Error.WriteLine();
            return;
        }

        var exceptionType = e.GetType().Name;
        Console.Error.WriteLine($"{exceptionType}: {e.Message}");
    }
}
