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
        var vOutType = TranslateType[((StringVar)arguments[1]).Data];

        if (!varIn.Convert(vOutType, out var varOut, out _, this))
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(varOut);
    }
}