using System.Text;

namespace Snobol4.Common;

//"replace third argument is not a string" /* 168 */,
//"replace second argument is not a string" /* 169 */,
//"replace first argument is not a string" /* 170 */,
//"null or unequally long 2nd, 3rd args to replace" /* 171 */,

public partial class Executive
{
    public void Replace(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var target, this))
        {
            LogRuntimeException(170);
            return;
        }

        if (!arguments[1].Convert(VarType.STRING, out _, out var from, this))
        {
            LogRuntimeException(169);
            return;
        }

        if (!arguments[2].Convert(VarType.STRING, out _, out var to, this))
        {
            LogRuntimeException(168);
            return;
        }

        var fromStr = (string)from;
        var toStr = (string)to;

        if (fromStr == "" || toStr == "" || fromStr.Length != toStr.Length)
        {
            LogRuntimeException(171);
            return;
        }

        var result = new StringBuilder((string)target);
        for (var i = 0; i < result.Length; ++i)
        {
            for (var j = 0; j < fromStr.Length; ++j)
            {
                if (result[i] != fromStr[j]) continue;
                result[i] = toStr[j];
                j = fromStr.Length;
            }
        }

        SystemStack.Push(new StringVar(result.ToString()));
    }
}