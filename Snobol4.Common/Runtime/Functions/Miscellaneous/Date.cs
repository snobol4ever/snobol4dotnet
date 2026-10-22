using System.Globalization;

namespace Snobol4.Common;

//"date argument is not integer" /* 330 */,

public partial class Executive
{
    // SPITBOL's DATE: the argument is an integer (the null string is 0); 1 gives MM/DD/YYYY HH:MM:SS, 2 gives YYYY-MM-DD HH:MM:SS, and
    // any other gives MM/DD/YY HH:MM:SS
    internal void Date(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var kind, this))
        {
            LogRuntimeException(330);
            return;
        }

        var format = (long)kind switch
        {
            1 => "MM/dd/yyyy HH:mm:ss",
            2 => "yyyy-MM-dd HH:mm:ss",
            _ => "MM/dd/yy HH:mm:ss"
        };
        SystemStack.Push(new StringVar(DateTime.Now.ToString(format, CultureInfo.InvariantCulture)));
    }
}