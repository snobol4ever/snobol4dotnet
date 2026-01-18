namespace Snobol4.Common;

//"char argument not integer" /* 281 */,
//"char argument not in range" /* 282 */,

public partial class Executive
{
    internal void Char(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var c, this))
        {
            LogRuntimeException(281);
            return;
        }

        var i = (int)(long)c;

        if (i is < 0 or >= 32768)
        {
            LogRuntimeException(282);
            return;
        }

        SystemStack.Push(new StringVar(char.ToString((char)i)));
    }
}