using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        int lineCount = 1 + Parent.Code.LineCountFile;
        AmpErrorText =
            $"{fileName}({codeCount}/{listCount}/{lineCount}): error {code} -- {CompilerException.ErrorMessage[code]}{Environment.NewLine}{SourceCode[AmpCurrentLineNumber].Split('\n')[1]}";
        Parent.ErrorCodeHistory.Add(code);
        Parent.ColumnHistory.Add(0);
        Failure = true;
        SystemStack.Push(new StringVar(false));
        var ce = new CompilerException(code, 0, AmpErrorText);
        Parent.MessageHistory.Add(AmpErrorText);
        ErrorJump = SetExitNumber;
        AmpErrorLimit--;
        if (Parent.CodeMode || AmpErrorLimit != 0) return;
        Console.Error.WriteLine(AmpErrorText);
        throw ce;
    }

    public StringVar NonExceptionFailure()
    {
        Failure = true;
        var nullVar = new StringVar(false);
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