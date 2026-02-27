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
                Parent.BuildEval();
                Parent.BuildOptions.CaseFolding = previousCaseFolding;
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
