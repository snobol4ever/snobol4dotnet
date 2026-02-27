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
        Console.Error.WriteLine("***UNEXPECTED EXCEPTION***");
        Console.Error.WriteLine();
        Console.Error.WriteLine(e.StackTrace);

        SaveException(e);
        WriteException(e);

        if (e.InnerException is not null)
        {
            SaveException(e.InnerException);
            WriteException(e.InnerException);
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
    public void LogCompilerException(int code, int cursorCurrent = 0)
    {
        if (Execute is not null)
        {
            Execute.Failure = true;
        }

        var ce = new CompilerException(code, cursorCurrent, CompilerException.ErrorMessage[code]);
        SaveExceptionHistory(ce);
        Console.Error.WriteLine(ce.Message);
    }

    /// <summary>
    /// Logs a compiler exception with source line context, displaying the error with a caret pointer.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="cursorCurrent">The current cursor position in the source line.</param>
    /// <param name="source">The source line where the error occurred.</param>
    public void LogCompilerException(int code, int cursorCurrent, SourceLine source)
    {
        if (Execute is not null)
        {
            Execute.Failure = true;
        }

        var message = BuildErrorMessage(code, cursorCurrent, source);
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
        var str = source.Text.Replace('\t', ' ');
        if (source.DeferredExpression)
        {
            str = " " + str[3..^1];
            cursorCurrent--;
        }

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(str);
 
        if (cursorCurrent > 0)
        {
            sb.Append(' ', cursorCurrent);
            sb.Append('!');
            sb.AppendLine();
        }

        var fileName = Path.GetFileName(source.PathName);
        var errorMessage = CompilerException.ErrorMessage[code];

        var codeCount = 1 + source.LineCountFile - source.BlankLineCount - source.CommentContinuationDirectiveCount;
        var listCount = 1 + source.LineCountFile - source.CommentContinuationDirectiveCount;
        int lineCount = 1 + source.LineCountFile;
        var pathLine = $"{fileName}({codeCount}/{listCount}/{lineCount}): error {code} -- {errorMessage}";
        sb.AppendLine(pathLine);
        return sb.ToString();
    }

    /// <summary>
    /// Saves an exception to the error history (excludes CompilerExceptions).
    /// </summary>
    /// <param name="e">The exception to save.</param>
    public void SaveException(Exception e)
    {
        if (e is CompilerException)
            return;

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
