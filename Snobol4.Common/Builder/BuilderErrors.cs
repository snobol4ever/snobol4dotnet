namespace Snobol4.Common;

public partial class Builder
{
    private void ReportProgrammingError(Exception e)
    {
        Console.Error.WriteLine(@"
***UNEXPECTED EXCEPTION***");
        Console.Error.WriteLine(@$"
{e.StackTrace}");
        SaveException(e);
        WriteException(e);

        if (e.InnerException == null)
            return;

        SaveException(e.InnerException);
        WriteException(e.InnerException);
    }

    private void ClearExceptionHistory()
    {
        ErrorCodeHistory.Clear();
        ColumnHistory.Clear();
        MessageHistory.Clear();
    }

    public void LogCompilerException(int code, int cursorCurrent, string message = "")
    {
        Execute?.Failure = true;
        var ce = new CompilerException(code, cursorCurrent, message);
        ErrorCodeHistory.Add(code);
        ColumnHistory.Add(cursorCurrent);
        MessageHistory.Add(ce.Message);
        Console.Error.WriteLine(ce.Message);
    }

    public void LogCompilerException(int code, int cursorCurrent, SourceLine source)
    {
        Execute?.Failure = true;
        var message = $"{Environment.NewLine}{source.Text.Replace('\t', ' ')}{Environment.NewLine}";
        if (cursorCurrent > 0) message += $"{new string(' ', cursorCurrent)}!{Environment.NewLine}";
        var fi = new FileInfo(source.PathName);
        message += $"{fi.Name}({source.LineCountFile},{cursorCurrent + 1}) : error {code} -- {CompilerException.ErrorMessage[code]}";
        var ce = new CompilerException(code, cursorCurrent, message);
        ErrorCodeHistory.Add(code);
        ColumnHistory.Add(cursorCurrent);
        MessageHistory.Add(ce.Message);
        Console.Error.WriteLine(ce.Message);
    }

    public void SaveException(Exception e)
    {
        if (e is CompilerException)
            return;

        ErrorCodeHistory.Add(UnexpectedExceptionErrorCode);
        ColumnHistory.Add(0);
        MessageHistory.Add(e.Message);
    }

    public void WriteException(Exception e)
    {
        if (e is CompilerException ce)
        {
            Console.Error.WriteLine(@$"
{ce.Message}
");
            return;
        }

        Console.Error.WriteLine(e.Message);
    }
}
