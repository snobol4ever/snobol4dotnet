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
        Console.Error.WriteLine(e.StackTrace);

        SaveAndWriteException(e);

        if (e.InnerException is not null)
        {
            SaveAndWriteException(e.InnerException);
        }
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
    public void LogCompilerException(int code, int cursorCurrent, SourceLine source) =>
        LogCompilerExceptionCore(code, cursorCurrent, BuildErrorMessage(code, cursorCurrent, source));

    /// <summary>
    /// Core logic for logging a compiler exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position.</param>
    /// <param name="message">The formatted error message.</param>
    private void LogCompilerExceptionCore(int code, int cursorCurrent, string message)
    {
        if (Execute is not null)
        {
            Execute.Failure = true;
        }

        var ce = new CompilerException(code, cursorCurrent, message);
        SaveExceptionHistory(ce);
        Console.Error.WriteLine(ce.Message);
    }

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

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(formattedSource);

        if (adjustedCursor > 0)
        {
            sb.Append(' ', adjustedCursor);
            sb.AppendLine("!");
        }

        var fileName = Path.GetFileName(source.PathName);
        var errorMessage = CompilerException.ErrorMessage[code];
        var (codeCount, listCount, lineCount) = CalculateLineCounts(source);

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

        Console.Error.WriteLine(e.Message);
    }
}
