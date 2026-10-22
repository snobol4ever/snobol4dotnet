namespace Snobol4.Common;

//"trim argument is not a string" /* 200 */,

public partial class Executive
{
    private static readonly char[] _whiteSpace = [' ', '\t'];
    public void Trim(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var str, this))
        {
            LogRuntimeException(200);
            return;
        }

        SystemStack.Push(new StringVar(((string)str).TrimEnd(_whiteSpace)));
    }
}