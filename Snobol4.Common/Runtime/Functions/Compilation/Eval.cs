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
                if (long.TryParse(stringVar.Data, out var l))
                {
                    SystemStack.Push(new IntegerVar(l));
                    return;
                }

                if (double.TryParse(stringVar.Data, out var d))
                {
                    SystemStack.Push(new RealVar(d));
                    return;
                }

                var previousCaseFolding = Parent.BuildOptions.CaseFolding;
                Parent.BuildOptions.CaseFolding = AmpCaseFolding!= 0;
                Parent.CodeMode = true;
                Parent.Code = new SourceCode(Parent);
                Parent.Code.ReadCodeInString($" A_ = *({stringVar.Data.Trim()})", Parent.FilesToCompile[^1]);
                Parent.BuildEval();
                Parent.BuildOptions.CaseFolding = previousCaseFolding;
                
                if (StarFunctionList.Count > 0)
                {
                    StarFunctionList[^1](this);
                } 
                else
                {
                    SystemStack.Push(StringVar.Null());
                }
                
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
