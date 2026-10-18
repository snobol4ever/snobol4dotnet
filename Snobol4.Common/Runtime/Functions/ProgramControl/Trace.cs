using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snobol4.Common;

//"trace fourth arg is not function name or null" /* 197 */,
//"trace first argument is not appropriate name" /* 198 */,
//"trace second argument is not trace type" /* 199 */,

public partial class Executive
{
    // TODO implement TRACE function
    internal void Trace(List<Var> arguments)
    {

        // The name is folded as any name is (TRACE('bar') traces BAR under case folding)
        if (!arguments[0].Convert(VarType.STRING, out _, out var traceNameRaw, this) || (string)traceNameRaw == "")
        {
            LogRuntimeException(198);
            return;
        }

        object traceName = Parent.FoldCase((string)traceNameRaw);

        if (!arguments[1].Convert(VarType.STRING, out _, out var traceType, this))
        {
            LogRuntimeException(199);
            return;
        }

        // CALL, RETURN and FUNCTION traces name a program-defined function (sbl: error 198 for a
        // builtin or an undefined name); a VALUE trace may name a variable that is also a function's
        // name, as its return value is
        var typeLetter = ((string)traceType).ToUpperInvariant();
        if (typeLetter is "C" or "CALL" or "R" or "RETURN" or "F" or "FUNCTION" && !UserFunctionTable.ContainsKey((string)traceName))
        {
            LogRuntimeException(198);
            return;
        }

        if (!arguments[3].Convert(VarType.STRING, out _, out var handlerName, this))
        {
            LogRuntimeException(197);
            return;
        }

        Assert.IsNotNull(handlerName);

        if ((string)handlerName != "" && FunctionTable[(string)handlerName] is not null)
        {
            LogRuntimeException(197);
            return;
        }

        Assert.IsNotNull(traceName);
        Assert.IsNotNull(traceType);

        var name = (string)traceName;
        switch (typeLetter)
        {
            case "A":
            case "ACCESS":
                TraceTableIdentifierAccess[name] = "";
                break;

            case "":
            case "V":
            case "VALUE":
                TraceTableIdentifierValue[name] = "";
                break;

            // Only the keywords the system changes may be traced, named without the ampersand
            case "K":
            case "KEYWORD":
                if (name is not ("ERRTYPE" or "FNCLEVEL" or "STCOUNT"))
                {
                    LogRuntimeException(198);
                    return;
                }
                TraceTableIdentifierKeyword[name] = "";
                break;

            case "L":
            case "LABEL":
                TraceTableLabel[name] = "";
                break;

            case "C":
            case "CALL":
                TraceTableFunctionCall[name] = "";
                break;

            case "R":
            case "RETURN":
                TraceTableFunctionReturn[name] = "";
                break;

            case "F":
            case "FUNCTION":
                TraceTableFunctionCall[name] = "";
                TraceTableFunctionReturn[name] = "";
                break;

            default:
                LogRuntimeException(199);
                return;
        }
    }

    internal void TraceIdentifierAccess(string name)
    {

        if (name[0] == '&')
            return;

        if (AmpTrace <= 0)
            return;

        if (!TraceTableIdentifierAccess.ContainsKey(name))
            return;

        TraceLine(name);
        AmpTrace--;
    }

    // A VALUE trace line, after the program assigns a traced variable (never for the binding of a
    // parameter or local, or its restore on return): ****3*******  N = 'first', as sbl prints it
    internal void TraceIdentifierValue(string name, Var value)
    {
        if (AmpTrace <= 0 || name.Length == 0 || name[0] == '&')
            return;

        if (!TraceTableIdentifierValue.ContainsKey(name))
            return;

        TraceLine($"{name} = {value.DumpString()}");
        AmpTrace--;
    }

    // SPITBOL's trace line: **** and the statement number padded with * to 8, a blank, an i for each
    // function level, a blank, the event; written to standard output, as sbl writes it
    internal void TraceLine(string text)
    {
        var stno = StatementNumber(AmpCurrentLineNumber).ToString(CultureInfo.InvariantCulture).PadRight(8, '*');
        Console.Out.WriteLine($"****{stno} {new string('i', (int)AmpFunctionLevel)} {text}");
    }


    // A LABEL trace, when a goto (taken, conditional or not) transfers to a traced label, at the goto's
    // statement: ****5*******  :(L)
    internal void TraceGoto(string name)
    {
        if (AmpTrace <= 0 || !TraceTableLabel.ContainsKey(name))
            return;

        TraceLine($":({name})");
        AmpTrace--;
    }

    // A KEYWORD trace of &ERRTYPE, &FNCLEVEL or &STCOUNT when the system changes it: ****5*******  &STCOUNT = 5
    internal void TraceKeyword(string name, long value)
    {
        if (AmpTrace <= 0 || !TraceTableIdentifierKeyword.ContainsKey(name))
            return;

        TraceLine($"&{name} = {value}");
        AmpTrace--;
    }
}
