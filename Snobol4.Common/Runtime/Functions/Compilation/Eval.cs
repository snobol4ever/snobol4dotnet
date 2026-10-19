namespace Snobol4.Common;

//"eval argument is not expression" /* 103 */,

public partial class Executive
{
    public void Eval(List<Var> arguments)
    {
        var arg = arguments[0];

        if (arg is NameVar nameVar)
            arg = IdentifierTable[nameVar.Pointer];

        switch (arg)
        {
            case IntegerVar integerVar:
                SystemStack.Push(new IntegerVar(integerVar.Data));
                return;

            case RealVar realVar:
                SystemStack.Push(new RealVar(realVar.Data));
                return;

            case StringVar stringVar:
                var previousCaseFolding = Parent.BuildOptions.CaseFolding;
                Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
                Parent.CodeMode = true;
                Parent.Code = new SourceCode(Parent);
                Parent.Code.ReadCodeInString($" *({stringVar.Data.Trim()})", Parent.FilesToCompile[^1]);
                Parent.Code.SourceLines[0].DeferredExpression = true;
                var errorsBefore = Parent.ErrorCodeHistory.Count;
                Parent.QuietCompile = true;
                try { Parent.BuildEval(); }
                finally { Parent.QuietCompile = false; }
                Parent.BuildOptions.CaseFolding = previousCaseFolding;

                // A string that does not compile makes EVAL fail with &ERRTYPE set to the compile error
                // (EVAL('1 +') is 221, an out-of-range real 231), printing nothing and counting as no
                // run-time error, as in SPITBOL
                if (Parent.ErrorCodeHistory.Count > errorsBefore)
                {
                    var code = Parent.ErrorCodeHistory[^1];
                    var added = Parent.ErrorCodeHistory.Count - errorsBefore;
                    Parent.ErrorCodeHistory.RemoveRange(errorsBefore, added);
                    Parent.ColumnHistory.RemoveRange(Parent.ColumnHistory.Count - added, added);
                    Parent.MessageHistory.RemoveRange(Parent.MessageHistory.Count - added, added);
                    Parent.CodeMode = false;
                    AmpErrorType = code;
                    NonExceptionFailure();
                    return;
                }

                StarFunctionList[^1](this);
                Parent.CodeMode = false;
                return;

            case ExpressionVar expression:
                expression.FunctionName(this);
                return;

            default:
                LogRuntimeException(103);
                return;
        }
    }
}
