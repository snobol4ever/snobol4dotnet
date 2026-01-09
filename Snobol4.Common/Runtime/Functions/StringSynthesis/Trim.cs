namespace Snobol4.Common;

public partial class Executive
{
    private static readonly char[] _whiteSpace = [' ', '\t'];
    public void Trim(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var str, this))
        {
            LogRuntimeException(247);
            return;
        }

        SystemStack.Push(new StringVar(((string)str).TrimEnd(_whiteSpace)));
    }
}