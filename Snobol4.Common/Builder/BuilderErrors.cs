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
    private void LogCompilerExceptionCore(int code, int cursorCurrent, string message, SourceLine? source = null)
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
        if (source is null)
            Console.Error.WriteLine(message);
        else
            WriteStatementInError(code, cursorCurrent, source);
    }

    // SPITBOL's report of a statement in error, on the listing page as sbl writes it (CEO-1858): the physical line the error was
    // found at, with the number of its statement in an eight-column field and its tabs kept; a line that keeps the tabs and puts
    // ! under the error column (empty when the error has none); "file(line,col) : ERROR nnn -- text"; and an empty line. A line
    // that -LIST has already listed is not listed again. The joined text of a continued statement drops the first character of
    // each continuation line, and an error found at the end of the text is reported at its last character, as sbl does.
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
            var total = lines[0].Length;
            for (var i = 1; i < lines.Count; i++)
                total += Math.Max(0, lines[i].Length - 1);
            var at = Math.Clamp(source.SubLineOffset + cursor, 0, Math.Max(0, total - 1));
            var start = 0;
            var length = lines[0].Length;
            while (at >= start + length && index + 1 < lines.Count)
            {
                start += length;
                index++;
                length = Math.Max(0, lines[index].Length - 1);
            }
            column = index == 0 ? at - start + 1 : at - start + 2;
        }

        var physical = lines[index];
        var file = source.PathName;
        var line = 1 + source.LineCountFile + index;
        if (!BuildOptions.ListSource)
            WriteListingLine($"{source.FirstStatementNumber,-8}{physical.TrimEnd(' ', '\t')}");

        var pointer = new StringBuilder();
        if (column > 0)
        {
            pointer.Append(' ', 8);
            for (var i = 0; i < column - 1; i++)
                pointer.Append(i < physical.Length && physical[i] == '\t' ? '\t' : ' ');
            pointer.Append('!');
        }
        WriteListingLine(pointer.ToString());
        var where = column > 0 ? $"{line},{column}" : $"{line}";
        WriteListingLine($"{file}({where}) : ERROR {code:D3} -- {CompilerException.ErrorMessage[code]}");
        WriteListingLine("");
        ListingWriter.Flush();
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
