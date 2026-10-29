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

        // the range is that of the whole integer: cut to 32 bits first, a real beyond them (CHAR(1E150)) fell in range
        var i = (long)c;

        if (i is < 0 or >= 32768)
        {
            LogRuntimeException(282);
            return;
        }

        SystemStack.Push(new StringVar(char.ToString((char)i)));
    }
}