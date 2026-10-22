using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snobol4.Common;

//"trace fourth arg is not function name or null" /* 197 */,
//"trace first argument is not appropriate name" /* 198 */,
//"trace second argument is not trace type" /* 199 */,

public partial class Executive
{
    internal void Trace(List<Var> arguments)
    {
        // A null first argument toggles SPITBOL's system trace switch, whatever the other arguments are
        if (arguments[0] is StringVar { Data: "" })
        {
            PredicateSuccess();
            return;
        }

        // The fourth argument names the trace function, or is null for a printed trace; whether that
        // function is defined is checked when the trace fires, as in SPITBOL (s_tra, trxeq)
        TraceHandler? handler = null;
        if (arguments[3] is not StringVar { Data: "" })
        {
            if (!NaturalVariableName(arguments[3], out var handlerName))
            {
                LogRuntimeException(197);
                return;
            }

            handler = new TraceHandler(handlerName, arguments[2]);
        }

        switch (ChangeTrace(arguments[0], arguments[1], handler, set: true))
        {
            case TraceOutcome.BadName:
                LogRuntimeException(198);
                return;
            case TraceOutcome.BadType:
                LogRuntimeException(199);
                return;
        }

        // TRACE returns the null string
        PredicateSuccess();
    }

    private enum TraceOutcome { Done, BadName, BadType }

    // SPITBOL's trace procedure, for TRACE (which sets a trace) and STOPTR (which clears it). The type is taken by its first letter,
    // folded to upper case: A access, V or a blank (and the null string) value, F, R and C a program function's call and return, L
    // a label, K a keyword. Any other is a bad type, and that is looked at before the name.
    private TraceOutcome ChangeTrace(Var nameArgument, Var typeArgument, TraceHandler? handler, bool set)
    {
        if (!typeArgument.Convert(VarType.STRING, out _, out var typeText, this))
            return TraceOutcome.BadType;

        var type = (string)typeText;
        var letter = type.Length == 0 ? ' ' : char.ToUpperInvariant(type[0]);
        if (letter is not (' ' or 'A' or 'V' or 'F' or 'R' or 'C' or 'L' or 'K'))
            return TraceOutcome.BadType;

        // The name is folded as any name is (TRACE('bar') traces BAR under case folding); an element's is no variable's
        if (!NaturalVariableName(nameArgument, out var name))
            return TraceOutcome.BadName;

        // CALL, RETURN and FUNCTION traces name a program-defined function (sbl: a builtin or an undefined name is a bad name),
        // a LABEL trace a label that is defined, a KEYWORD trace one of the keywords the system changes, named without the
        // ampersand; a VALUE trace may name a variable that is also a function's name, as its return value is
        if (letter is 'F' or 'R' or 'C' && !UserFunctionTable.ContainsKey(name)
            || letter == 'L' && !LabelTable.ContainsKey(name)
            || letter == 'K' && name is not ("ERRTYPE" or "FNCLEVEL" or "STCOUNT"))
            return TraceOutcome.BadName;

        void Apply(Dictionary<string, TraceHandler?> table)
        {
            if (set)
                table[name] = handler;
            else
                table.Remove(name);
        }

        switch (letter)
        {
            case 'A':
                Apply(TraceTableIdentifierAccess);
                break;
            case 'L':
                Apply(TraceTableLabel);
                break;
            case 'K':
                Apply(TraceTableIdentifierKeyword);
                break;
            case 'C':
                Apply(TraceTableFunctionCall);
                break;
            case 'R':
                Apply(TraceTableFunctionReturn);
                break;
            case 'F':
                Apply(TraceTableFunctionCall);
                Apply(TraceTableFunctionReturn);
                break;
            default:
                Apply(TraceTableIdentifierValue);
                break;
        }

        return TraceOutcome.Done;
    }

    // An ACCESS trace line, when the program fetches a traced variable's value: ****4*******  N = 5, as
    // sbl prints it
    internal void TraceIdentifierAccess(string name, Var value)
    {
        if (name.Length == 0 || name[0] == '&')
            return;

        if (AmpTrace <= 0)
            return;

        if (!TraceTableIdentifierAccess.TryGetValue(name, out var handler))
            return;

        AmpTrace--;
        if (handler != null)
            CallTraceHandler(handler, name);
        else
            TraceLine($"{name} = {PrintValue(value)}");
    }

    // A VALUE trace line, after the program assigns a traced variable (never for the binding of a
    // parameter or local, or its restore on return): ****3*******  N = 'first', as sbl prints it
    internal void TraceIdentifierValue(string name, Var value)
    {
        if (AmpTrace <= 0 || name.Length == 0 || name[0] == '&')
            return;

        if (!TraceTableIdentifierValue.TryGetValue(name, out var handler))
            return;

        AmpTrace--;
        if (handler != null)
            CallTraceHandler(handler, name);
        else
            TraceLine($"{name} = {PrintValue(value)}");
    }

    // SPITBOL's trace line: **** and the statement number padded with * to 8, a blank, an i for each
    // function level, a blank, the event; written to standard output, as sbl writes it, cut at the page width
    internal void TraceLine(string text)
    {
        var stno = StatementNumber(AmpCurrentLineNumber).ToString(CultureInfo.InvariantCulture).PadRight(8, '*');
        foreach (var piece in Builder.WrapLine($"****{stno} {new string('i', (int)AmpFunctionLevel)} {text}", Parent.BuildOptions.PageWidth))
            Console.Out.WriteLine(piece);
    }


    // A LABEL trace, when a goto (taken, conditional or not) transfers to a traced label, at the goto's
    // statement: ****5*******  :(L)
    internal void TraceGoto(string name)
    {
        if (AmpTrace <= 0 || !TraceTableLabel.TryGetValue(name, out var handler))
            return;

        AmpTrace--;
        if (handler != null)
            CallTraceHandler(handler, name);
        else
            TraceLine($":({name})");
    }

    // A KEYWORD trace of &ERRTYPE, &FNCLEVEL or &STCOUNT when the system changes it: ****5*******  &STCOUNT = 5
    internal void TraceKeyword(string name, long value)
    {
        if (AmpTrace <= 0 || !TraceTableIdentifierKeyword.TryGetValue(name, out var handler))
            return;

        AmpTrace--;
        if (handler != null)
            CallTraceHandler(handler, name);
        else
            TraceLine($"&{name} = {value}");
    }

    // A trace with a fourth argument calls that function in place of printing, as SPITBOL's trxeq
    // does: its arguments are the NAME of the traced item and TRACE's third argument, &TRACE reads 0
    // during the call and is restored after it, and the call's value, success or failure is dropped;
    // an undefined function is error 197 when the trace fires
    internal void CallTraceHandler(TraceHandler handler, string name)
    {
        var entry = FunctionTable[handler.Function];
        if (entry == null)
        {
            LogRuntimeException(197);
            return;
        }

        var savedTrace = AmpTrace;
        var savedFailure = Failure;
        var depth = SystemStack.Count;
        AmpTrace = 0;
        Failure = false;

        // a fresh argument list: the caller may still hold the shared one
        var arguments = new List<Var> { new NameVar(name, null, null), handler.Tag };
        for (var i = arguments.Count; i < entry.ArgumentCount; ++i)
            arguments.Add(StringVar.Null());
        arguments.Add(new StringVar(handler.Function));
        entry.Handler(arguments);

        while (SystemStack.Count > depth)
            SystemStack.Pop();
        AmpTrace = savedTrace;
        Failure = savedFailure;
    }
}

/// <summary>The function a trace calls (TRACE's fourth argument) and the tag it passes (the third).</summary>
internal sealed record TraceHandler(string Function, Var Tag);
