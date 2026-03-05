namespace Snobol4.Common;

//"goto operand in direct goto is not code" /* 24 */,

public partial class Executive
{
    public void CreateCode(List<Var> arguments)
    {
        CodeVar code = new()
        {
            StatementNumber = Parent.StatementCount,
            Data = ""
        };

        var previousCaseFolding = Parent.BuildOptions.CaseFolding;
        Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
        Parent.CodeMode = true;

        switch (arguments[0])
        {
            case NameVar:
                if (IdentifierTable.TryGetValue(arguments[0].Symbol, out var argument) && argument is StringVar stringVar1)
                {
                    Parent.Code = new SourceCode(Parent);
                    code.Data = stringVar1.Data;
                    Parent.Code.ReadCodeInString(code.Data, Parent.FilesToCompile[^1]);
                    Parent.BuildCode();
                    Parent.BuildOptions.CaseFolding = previousCaseFolding;
                    SystemStack.Push(code);
                    Parent.CodeMode = false;
                    return;
                }

                NonExceptionFailure();
                return;

            case StringVar stringVar:
                Parent.Code = new SourceCode(Parent);
                code.Data = stringVar.Data;
                Parent.Code.ReadCodeInString(code.Data, Parent.FilesToCompile[^1]);
                Parent.BuildCode();
                Parent.BuildOptions.CaseFolding = previousCaseFolding;
                SystemStack.Push(code);
                Parent.CodeMode = false;
                return;

            default:
                NonExceptionFailure();
                return;
        }
    }
}