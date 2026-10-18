namespace Snobol4.Common;

public partial class Executive
{
    public int OnErrorGoto;

    // The last runtime error's full diagnostic, as printed: file, numbers, message and source line
    internal string LastDiagnostic = "";

    // fatal: the run ends whatever &ERRLIMIT allows, as SPITBOL ends it on a stack overflow (246)
    public void LogRuntimeException(int code) => LogRuntimeException(code, fatal: false);

    public void LogRuntimeException(int code, bool fatal)
    {
        AmpErrorType = code;
        TraceKeyword("ERRTYPE", code);
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber]);
        // The erring statement's own numbers, recorded when it was compiled ("name:code/list)" and its
        // physical line), not the compiler's counters, which have moved on to the end of the source
        var header = SourceCode[AmpCurrentLineNumber].Split('\n')[0];
        var counts = header[(header.LastIndexOf(':') + 1)..].TrimEnd(')');
        var lineCount = 1 + SourceLineNumbers[AmpCurrentLineNumber];
        // &ERRTEXT is the message alone, as in SPITBOL; the full diagnostic (file, numbers, source
        // line) is what is printed and recorded
        AmpErrorText = CompilerException.ErrorMessage[code];
        LastDiagnostic =
            $"{fileName}({counts}/{lineCount}): error {code} -- {CompilerException.ErrorMessage[code]}{Environment.NewLine}{SourceCode[AmpCurrentLineNumber].Split('\n')[1]}";
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

        Console.Error.WriteLine(LastDiagnostic);
        throw ce;
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