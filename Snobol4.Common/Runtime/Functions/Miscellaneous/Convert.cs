namespace Snobol4.Common;

//"convert second argument is not a string" /* 74 */,

public partial class Executive
{
    public static Dictionary<string, VarType> TranslateType = new(StringComparer.OrdinalIgnoreCase) // Always case-insensitive
    {
        { "string", VarType.STRING },
        { "integer", VarType.INTEGER },
        { "real", VarType.REAL },
        { "array", VarType.ARRAY },
        { "table", VarType.TABLE },
        { "pattern", VarType.PATTERN },
        { "name", VarType.NAME },
        { "expression", VarType.EXPRESSION },
        { "code", VarType.CODE }
    };

    public enum VarType
    {
        STRING = 1,
        INTEGER = 2,
        REAL = 3,
        ARRAY = 4,
        TABLE = 5,
        PATTERN = 6,
        NAME = 7,
        EXPRESSION = 8,
        CODE = 9
    }

    public void Convert(List<Var> arguments)
    {
        var varIn = arguments[0];

        // The type is any value with a string form -- 'INTEGER', the name .INTEGER, even 3 -- and
        // a string that names no datatype fails the call, as in SPITBOL
        if (!arguments[1].Convert(VarType.STRING, out _, out var typeName, this))
        {
            LogRuntimeException(74);
            return;
        }

        // NUMERIC converts as arithmetic does, to an INTEGER or a REAL, as in SPITBOL: '1.0' gives 1., '' gives 0
        if (string.Equals((string)typeName, "numeric", StringComparison.OrdinalIgnoreCase))
        {
            if (varIn is ArrayVar or TableVar || !Var.ToNumeric(varIn, out var isInteger, out var l, out var d, this))
            {
                NonExceptionFailure();
                return;
            }

            SystemStack.Push(isInteger ? new IntegerVar(l) : new RealVar(d));
            return;
        }

        if (!TranslateType.TryGetValue((string)typeName, out var vOutType))
        {
            NonExceptionFailure();
            return;
        }

        if (!varIn.Convert(vOutType, out var varOut, out _, this))
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(varOut);
    }
}