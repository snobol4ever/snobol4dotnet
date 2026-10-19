namespace Snobol4.Common;

//"stoptr first argument is not appropriate name" /* 190 */,
//"stoptr second argument is not trace type" /* 191 */,

public partial class Executive
{
    internal void StopTrace(List<Var> arguments)
    {
        // STOPTR(name, type) — remove trace on variable/function/label for the given type.
        // Error 190: first arg not a name. Error 191: second arg not a valid trace type.
        // If type is null/empty, remove from all tables (matches SPITBOL behaviour).

        if (!arguments[0].Convert(VarType.STRING, out _, out var traceName, this))
        {
            LogRuntimeException(190);
            return;
        }

        var typeArg = arguments.Count > 1 ? arguments[1] : null;
        var traceTypeStr = "";
        if (typeArg != null && typeArg.Convert(VarType.STRING, out _, out var tt, this))
            traceTypeStr = ((string)tt).ToUpperInvariant();

        var name = Parent.FoldCase((string)traceName);

        switch (traceTypeStr)
        {
            case "A":
            case "ACCESS":
                TraceTableIdentifierAccess.Remove(name);
                break;
            case "":
            case "V":
            case "VALUE":
                TraceTableIdentifierValue.Remove(name);
                break;
            case "K":
            case "KEYWORD":
                TraceTableIdentifierKeyword.Remove("&" + name);
                TraceTableIdentifierKeyword.Remove(name);
                break;
            case "L":
            case "LABEL":
                TraceTableLabel.Remove(name);
                break;
            case "C":
            case "CALL":
                TraceTableFunctionCall.Remove(name);
                break;
            case "R":
            case "RETURN":
                TraceTableFunctionReturn.Remove(name);
                break;
            case "F":
            case "FUNCTION":
                TraceTableFunctionCall.Remove(name);
                TraceTableFunctionReturn.Remove(name);
                break;
            default:
                LogRuntimeException(191);
                return;
        }

        // STOPTR returns the null string, as in SPITBOL
        PredicateSuccess();
    }
}