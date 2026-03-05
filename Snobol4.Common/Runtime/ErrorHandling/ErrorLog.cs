namespace Snobol4.Common;

public partial class Executive
{
    public int ErrorJump;

    public void LogRuntimeException(int code)
    {
        AmpErrorType = code;
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber]);
        var codeCount = 1 + Parent.Code.LineCountFile - Parent.Code.BlankLineCount -
                        Parent.Code.CommentContinuationDirectiveCount;
        var listCount = 1 + Parent.Code.LineCountFile - Parent.Code.CommentContinuationDirectiveCount;
        var lineCount = 1 + Parent.Code.LineCountFile;
        AmpErrorText =
            $"{fileName}({codeCount}/{listCount}/{lineCount}): error {code} -- {CompilerException.ErrorMessage[code]}{Environment.NewLine}{SourceCode[AmpCurrentLineNumber].Split('\n')[1]}";
        Parent.ErrorCodeHistory.Add(code);
        Parent.ColumnHistory.Add(0);
        Failure = true;
        SystemStack.Push(_sentinelFailure);
        var ce = new CompilerException(code, 0, AmpErrorText);
        Parent.MessageHistory.Add(AmpErrorText);
        ErrorJump = SetExitNumber;
        AmpErrorLimit--;
        if (Parent.CodeMode || AmpErrorLimit != 0) return;
        Console.Error.WriteLine(AmpErrorText);
        throw ce;
    }

    // -------------------------------------------------------------------------
    // Static sentinels — reused for every predicate success/failure push.
    // These objects carry only the Succeeded flag; their string data is never
    // read before they are discarded at the next Finalize boundary.
    // Marking them IsReadOnly prevents accidental mutation.
    // -------------------------------------------------------------------------
    private static readonly StringVar _sentinelSuccess = new(true)  { IsReadOnly = true };
    private static readonly StringVar _sentinelFailure = new(false) { IsReadOnly = true };

    /// <summary>Shared failure sentinel — use instead of new StringVar(false).</summary>
    public StringVar FailureSentinel => _sentinelFailure;

    public StringVar NonExceptionFailure()
    {
        Failure = true;
        SystemStack.Push(_sentinelFailure);
        return _sentinelFailure;
    }

    public StringVar PredicateSuccess()
    {
        Failure = false;
        SystemStack.Push(_sentinelSuccess);
        return _sentinelSuccess;
    }
}