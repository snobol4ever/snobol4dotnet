namespace Snobol4.Common;

public partial class Executive
{
    public int OnErrorGoto;

    // The last runtime error's full diagnostic, as printed: file, numbers, message and source line
    internal string LastDiagnostic = "";

    public void LogRuntimeException(int code)
    {
        AmpErrorType = code;
        TraceKeyword("ERRTYPE", code);
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber]);
        var codeCount = 1 + Parent.Code.LineCountFile - Parent.Code.BlankLineCount -
                        Parent.Code.CommentContinuationDirectiveCount;
        var listCount = 1 + Parent.Code.LineCountFile - Parent.Code.CommentContinuationDirectiveCount;
        var lineCount = 1 + Parent.Code.LineCountFile;
        // &ERRTEXT is the message alone, as in SPITBOL; the full diagnostic (file, numbers, source
        // line) is what is printed and recorded
        AmpErrorText = CompilerException.ErrorMessage[code];
        LastDiagnostic =
            $"{fileName}({codeCount}/{listCount}/{lineCount}): error {code} -- {CompilerException.ErrorMessage[code]}{Environment.NewLine}{SourceCode[AmpCurrentLineNumber].Split('\n')[1]}";
        Parent.ErrorCodeHistory.Add(code);
        Parent.ColumnHistory.Add(0);
        Failure = true;
        SystemStack.Push(new StringVar(false));
        var ce = new CompilerException(code, 0, LastDiagnostic);
        Parent.MessageHistory.Add(LastDiagnostic);
        OnErrorGoto = SetExitNumber;

        if (AmpErrorLimit > 0)
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