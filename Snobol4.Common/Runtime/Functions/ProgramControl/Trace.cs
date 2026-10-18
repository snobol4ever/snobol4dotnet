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

        if (!arguments[0].Convert(VarType.STRING, out _, out var traceName, this) || FunctionTable.ContainsKey((string)traceName))
        {
            LogRuntimeException(198);
            return;
        }

        if (!arguments[1].Convert(VarType.STRING, out _, out var traceType, this))
        {
            LogRuntimeException(199);
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

        switch (((string)traceType).ToUpper(CultureInfo.CurrentCulture))
        {
            case "A":
            case "ACCESS":
                if (TraceTableIdentifierAccess.ContainsKey((string)traceName))
                    TraceTableIdentifierAccess.Remove((string)traceName);
                TraceTableIdentifierAccess.Add((string)traceName, "");
                break;

            case "":
            case "V":
            case "VALUE":
                if (TraceTableIdentifierValue.ContainsKey((string)traceName))
                    TraceTableIdentifierValue.Remove((string)traceName);
                TraceTableIdentifierValue.Add((string)traceName, "");
                break;

            case "K":
            case "KEYWORD":
                if (TraceTableIdentifierKeyword.ContainsKey("&" + (string)traceName))
                    TraceTableIdentifierKeyword.Remove((string)traceName);
                TraceTableIdentifierKeyword.Add((string)traceName, "");
                break;

            case "L":
            case "LABEL":
                if (TraceTableLabel.ContainsKey((string)traceName))
                    TraceTableLabel.Remove((string)traceName);
                TraceTableLabel.Add((string)traceName, "");
                break;

            case "C":
            case "CALL":
                if (TraceTableFunctionCall.ContainsKey((string)traceName))
                    TraceTableFunctionCall.Remove((string)traceName);
                TraceTableFunctionCall.Add((string)traceName, "");
                break;

            case "R":
            case "RETURN":
                if (TraceTableFunctionReturn.ContainsKey((string)traceName))
                    TraceTableFunctionReturn.Remove((string)traceName);
                TraceTableFunctionReturn.Add((string)traceName, "");
                break;

            case "F":
            case "FUNCTION":
                if (TraceTableFunctionCall.ContainsKey((string)traceName))
                    TraceTableFunctionCall.Remove((string)traceName);
                TraceTableFunctionCall.Add((string)traceName, "");

                if (TraceTableFunctionReturn.ContainsKey((string)traceName))
                    TraceTableFunctionReturn.Remove((string)traceName);
                TraceTableFunctionReturn.Add((string)traceName, "");
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


    // ReSharper disable once UnusedMember.Global
    internal void TraceGoto(string name)
    {
        if (AmpTrace <= 0)
            return;

        if (!TraceTableLabel.ContainsKey(name))
            return;

        var line = AmpCurrentLineNumber + 1;
        var linePad = line.ToString().PadRight(8, '*');
        var r = (int)AmpFunctionLevel;

        Console.Error.WriteLine($@"****{linePad} {new string('i', r)} goto {name}");

        AmpTrace--;
    }

    // ReSharper disable once UnusedMember.Global
    internal void TraceFunctionCall(string name)
    {
        if (AmpTrace <= 0)
            return;

        if (!TraceTableFunctionCall.ContainsKey(name))
            return;

        var line = AmpCurrentLineNumber + 1;
        var linePad = line.ToString().PadRight(8, '*');
        var r = (int)AmpFunctionLevel;

        Console.Error.WriteLine($@"****{linePad} {new string('i', r)} call {name}");

        AmpTrace--;
    }

    // ReSharper disable once UnusedMember.Global
    internal void TraceFunctionReturn(string name)
    {
        if (AmpTrace <= 0)
            return;

        if (!TraceTableFunctionReturn.ContainsKey(name))
            return;

        var line = AmpCurrentLineNumber + 1;
        var linePad = line.ToString().PadRight(8, '*');
        var r = (int)AmpFunctionLevel;

        Console.Error.WriteLine($@"****{linePad} {new string('i', r)} return {name}");

        AmpTrace--;
    }
}