namespace Snobol4.Common;

public partial class Executive
{
    public int OnErrorGoto;

    // The last runtime error's full diagnostic, as printed: file, numbers, message and source line
    internal string LastDiagnostic = "";

    // fatal: the run ends whatever &ERRLIMIT allows, as SPITBOL ends it on a stack overflow (246)
    public void LogRuntimeException(int code) => LogRuntimeException(code, fatal: false);

    // `errorType` is the value &ERRTYPE takes where it is not the code reported: assigning &ERRTYPE = 0 reports error 36 and leaves 0
    // Counts the run-time errors raised, with the function level of the latest, so a pattern match can tell an error its own scan
    // raised from one raised inside a function the scan called (Scanner.Match)
    internal long ErrorSerial;
    internal long ErrorFunctionLevel;

    // Set while the statement's failure is an error's: with a SETEXIT handler armed, the handler takes it at once, and neither a
    // negation nor a choice's next alternative goes on from it, as in SPITBOL (Negation, ChoiceStart)
    internal bool FailureIsError;

    // An error ends the evaluation it occurs in, so that neither a negation nor a choice's next alternative goes on from it, while a
    // SETEXIT handler waits for it, and when it occurs in the code of an evaluated expression (EVAL of a string or an expression, not
    // a statement of a function the expression calls) that ends on an error (EvaluationEndsOnError), which then fails as a whole, as in
    // SPITBOL: EVAL('~(1 / N)') fails in the main program and is the null string in a function's statement
    internal bool ErrorEndsEvaluation =>
        FailureIsError && (OnErrorGoto > 0 || ExpressionDepth > 0 && ErrorFunctionLevel == ExpressionFunctionLevel && EvaluationEndsOnError);

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
        ErrorSerial++;
        ErrorFunctionLevel = AmpFunctionLevel;
        Failure = true;
        FailureOrigin = SystemStack.Count;
        FailureIsError = true;
        SystemStack.Push(new StringVar(false));
        var ce = new CompilerException(code, 0, LastDiagnostic);
        Parent.MessageHistory.Add(LastDiagnostic);
        // A SETEXIT handler intercepts an error only while &ERRLIMIT is above 0, as in SPITBOL; an error inside an expression's
        // evaluation at 0 fails its statement and enters no handler (EVAL('X / 0') with the limit at 0 just fails)
        OnErrorGoto = AmpErrorLimit > 0 ? SetExitNumber : 0;

        // An error inside the evaluation of an expression (EVAL, a deferred expression in a pattern) never ends the run, whatever
        // &ERRLIMIT is, in SPITBOL: the statement fails and &ERRLIMIT counts down no further than 0
        if ((AmpErrorLimit > 0 || ExpressionDepth > 0) && !fatal)
        {
            if (AmpErrorLimit > 0)
                AmpErrorLimit--;
            // an armed handler takes the error where it is, as in SPITBOL (RunHandlerAtError)
            if (OnErrorGoto > 0)
                RunHandlerAtError();
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

    // The statement whose line the report names when :(CONTINUE) has resumed at the goto of the statement in error and that goto errs
    // again: the line is that statement's, the statement number the handler's CONTINUE; -1 when the line is the current statement's
    internal int ReportLineIndex = -1;

    // The position and run summary that end a run, from "in file" to "memory left (bytes)", labelled to column 21
    internal string[] RunSummary()
    {
        string Row(string label, object value) => label.PadRight(21) + value;
        var memoryUsed = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
        var memoryLeft = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        return
        [
            Row("in file", StatementFile(ReportLineIndex >= 0 ? ReportLineIndex : AmpCurrentLineNumber)),
            Row("in line", StatementLine(ReportLineIndex >= 0 ? ReportLineIndex : AmpCurrentLineNumber)),
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
    // and an empty line; sbl hard-wraps every line at the page width, 120 columns by default, mid-word.
    private void PrintFatalReport(int code)
    {
        // cut at the width of the page: 120, or what -t gives
        var width = Math.Max(1, Parent.BuildOptions.PageWidth);
        var file = StatementFile(ReportLineIndex >= 0 ? ReportLineIndex : AmpCurrentLineNumber);
        var line = StatementLine(ReportLineIndex >= 0 ? ReportLineIndex : AmpCurrentLineNumber);
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
        // in the long format (-p) the listing has a form feed for the three empty lines before the error line and the last three of
        // the five after it; standard error keeps the empty lines
        var longFormat = Parent.BuildOptions.PrinterListing && Parent.ListingPath != "";
        for (var index = 0; index < report.Length; index++)
        {
            var rest = report[index];
            var copy = copied.Contains(index);
            var onListing = !(longFormat && (index < 3 || index is 6 or 7 or 8));
            var formFeed = longFormat && index == 3;
            var formFeedLine = longFormat && index == 5;
            // of a line cut into pieces only the last goes to standard error as well
            for (; rest.Length > width; rest = rest[width..])
            {
                WriteReportPiece(listing, rest[..width], false, onListing, formFeed);
                formFeed = false;
            }

            if (formFeedLine)
            {
                // an empty line of the report that the long format shows as a form feed alone on its line
                listing.WriteLine("\f");
                listing.Flush();
                Parent.WriteReportLine(TextWriter.Null, rest, copy);
                continue;
            }

            WriteReportPiece(listing, rest, copy, onListing, formFeed);
        }

        listing.Flush();
    }

    // A line of the fatal report: on the listing when it belongs there (a form feed in front of the error line in the long format), and on
    // standard error when it is one of the lines SPITBOL copies there
    private void WriteReportPiece(TextWriter listing, string text, bool copy, bool onListing, bool formFeed)
    {
        if (!onListing)
        {
            Parent.WriteReportLine(TextWriter.Null, text, copy);
            return;
        }

        if (!formFeed)
        {
            Parent.WriteReportLine(listing, text, copy);
            return;
        }

        listing.WriteLine("\f" + text);
        listing.Flush();
        if (copy)
            Parent.WriteReportLine(TextWriter.Null, text, true);
    }

    // The failure value that takes the place of operands taken off the stack after a failure (an operator, a call or a subscript that
    // does not run): the failure began at or below it, so an operand pushed after it is not the one that failed, as a negation asks
    // (REVERSE(' 12 ') * -~1 * ~6 fails: the ~ of 6 is no negation of the failure ~1 began, which the operators after it moved down)
    internal void PushFailedValue()
    {
        SystemStack.Push(new StringVar(false) { Succeeded = false });
        if (FailureOrigin > SystemStack.Count - 1)
            FailureOrigin = SystemStack.Count - 1;
    }

    public StringVar NonExceptionFailure()
    {
        Failure = true;
        FailureOrigin = SystemStack.Count;
        FailureIsError = false;
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