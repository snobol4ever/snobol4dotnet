namespace Snobol4.Common;

public partial class Executive
{
    public int OnErrorGoto;

    // The last runtime error's full diagnostic, as printed: file, numbers, message and source line
    internal string LastDiagnostic = "";

    // fatal: the run ends whatever &ERRLIMIT allows, as SPITBOL ends it on a stack overflow (246)
    public void LogRuntimeException(int code) => LogRuntimeException(code, fatal: false);

    // `errorType` is the value &ERRTYPE takes where it is not the code reported: assigning &ERRTYPE = 0 reports error 36 and leaves 0
    public void LogRuntimeException(int code, bool fatal, long? errorType = null)
    {
        if (PreEvaluating)
        {
            PreEvaluationCode = code;
            throw new PreEvaluationAbandoned();
        }

        AmpErrorType = errorType ?? code;
        TraceKeyword("ERRTYPE", AmpErrorType);
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber]);
        // The erring statement's own numbers, recorded when it was compiled ("name:code/list)" and its
        // physical line), not the compiler's counters, which have moved on to the end of the source
        var header = SourceCode[AmpCurrentLineNumber].Split('\n')[0];
        var counts = header[(header.LastIndexOf(':') + 1)..].TrimEnd(')');
        var lineCount = 1 + SourceLineNumbers[AmpCurrentLineNumber];
        // &ERRTEXT is the message alone, as in SPITBOL; the full diagnostic (file, numbers, source
        // line) is what is printed and recorded
        AmpErrorText = ErrorMessageText(code);
        LastDiagnostic =
            $"{fileName}({counts}/{lineCount}): error {code} -- {AmpErrorText}{Environment.NewLine}{SourceCode[AmpCurrentLineNumber].Split('\n')[1]}";
        Parent.ErrorCodeHistory.Add(code);
        Parent.ColumnHistory.Add(0);
        Failure = true;
        FailureOrigin = SystemStack.Count;
        SystemStack.Push(new StringVar(false));
        var ce = new CompilerException(code, 0, LastDiagnostic);
        Parent.MessageHistory.Add(LastDiagnostic);
        OnErrorGoto = SetExitNumber;

        if (AmpErrorLimit > 0 && !fatal)
        {
            AmpErrorLimit--;
            return;
        }

        PrintFatalReport(code);
        throw ce;
    }

    // The text of error `code`: SPITBOL's own, or for a number it does not use (an error a program signals by assigning &ERRTYPE) the
    // text the program put in &ERRTEXT
    internal string ErrorMessageText(int code) =>
        code >= 0 && code < CompilerException.ErrorMessage.Length ? CompilerException.ErrorMessage[code] : AmpErrorText;

    // Set once the fatal report has been printed: it carries the run summary, which -x does not print again
    internal bool FatalReportPrinted;

    // The position and run summary that end a run, from "in file" to "memory left (bytes)", labelled to column 21
    internal string[] RunSummary()
    {
        string Row(string label, object value) => label.PadRight(21) + value;
        var memoryUsed = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
        var memoryLeft = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        return
        [
            Row("in file", StatementFile(AmpCurrentLineNumber)),
            Row("in line", StatementLine(AmpCurrentLineNumber)),
            Row("in statement", StatementNumber(AmpCurrentLineNumber)),
            Row("stmts executed", AmpStatementCount),
            Row("execution time msec", _timerExecute.ElapsedMilliseconds),
            Row("REGENERATIONS", GC.CollectionCount(2)),
            Row("memory used (bytes)", memoryUsed),
            Row("memory left (bytes)", memoryLeft)
        ];
    }

    // The report of a fatal run-time error as sbl writes it (CEO-1858): on the listing stream, which is standard output unless
    // -o names a file. Three empty lines, the error line, five empty lines, the position and run summary labelled to column 21,
    // and an empty line; sbl hard-wraps every line at 120 columns, mid-word.
    private void PrintFatalReport(int code)
    {
        const int width = 120;
        var file = StatementFile(AmpCurrentLineNumber);
        var line = StatementLine(AmpCurrentLineNumber);
        FatalReportPrinted = true;
        string[] report =
        [
            "", "", "",
            $"{file}({line}) : ERROR {code:D3} -- {ErrorMessageText(code)}",
            "", "", "", "", "",
            .. RunSummary(),
            ""
        ];
        // the lines SPITBOL also sends to standard error: the error line and the two empty lines after it, the position and run summary
        // down to REGENERATIONS (not the memory figures), and the last empty line
        var copied = new HashSet<int> { 3, 4, 5, 9, 10, 11, 12, 13, 14, 17 };
        var listing = Parent.ListingWriter;
        for (var index = 0; index < report.Length; index++)
        {
            var rest = report[index];
            for (; rest.Length > width; rest = rest[width..])
                Parent.WriteReportLine(listing, rest[..width], copied.Contains(index));
            Parent.WriteReportLine(listing, rest, copied.Contains(index));
        }
        listing.Flush();
    }

    public StringVar NonExceptionFailure()
    {
        Failure = true;
        FailureOrigin = SystemStack.Count;
        var nullVar = new StringVar(false) { Succeeded = false };
        SystemStack.Push(nullVar);
        return nullVar;
    }

    public StringVar PredicateSuccess()
    {
        Failure = false;
        var nullVar = new StringVar(true);
        SystemStack.Push(nullVar);
        return nullVar;
    }
}