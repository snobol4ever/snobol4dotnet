using System.Globalization;
using System.Runtime.CompilerServices;
﻿namespace Snobol4.Common;

//"define first argument is not a string" /* 81 */,
//"define first argument is null" /* 82 */,
//"define first argument is missing a left paren" /* 83 */,
//"define first argument has null function name" /* 84 */,
//"null arg name or missing ) in define first arg." /* 85 */,
//"define function entry point is not defined label" /* 86 */,
//"attempted redefinition of system function" /* 248 */,

public partial class Executive
{
    //internal Dictionary<string, UserFunctionTableEntry> UserFunctionTable = new();

    internal void CreateProgramDefinedFunction(List<Var> arguments)
    {

        // SPITBOL looks at the entry label (the second argument) first: a name that is not a variable's is 86
        string? entryLabel = null;
        if (arguments.Count >= 2 && arguments[1] is not StringVar { Data: "" })
        {
            if (!NaturalVariableName(arguments[1], out var label))
            {
                LogRuntimeException(86);
                return;
            }

            entryLabel = label;
        }

        // the prototype must be a string (81) and not the null string (82)
        if (arguments.Count == 0 || !arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(arguments.Count == 0 ? 82 : 81);
            return;
        }

        if ((string)value == "")
        {
            LogRuntimeException(82);
            return;
        }

        // The prototype is scanned as SPITBOL scans it: the function's name up to the left paren (83 when there is none, 84 when
        // it is null), the arguments up to the right paren, each ended by a comma or the paren (85 at the end of the string), and
        // the locals, ended by commas. A null name is skipped, except that a prototype with no arguments ends the argument list.
        var prototype = FoldName((string)value).Trim();
        var position = 0;
        var functionName = ScanPrototype(prototype, ref position, '(', '(', out var stopped);
        if (stopped == 0)
        {
            LogRuntimeException(83);
            return;
        }

        if (functionName == "")
        {
            LogRuntimeException(84);
            return;
        }

        List<string> parameters = [];
        while (true)
        {
            var parameter = ScanPrototype(prototype, ref position, ')', ',', out stopped);
            if (stopped == 0)
            {
                LogRuntimeException(85);
                return;
            }

            if (parameter == "")
            {
                if (parameters.Count == 0)
                    break;
                continue;
            }

            parameters.Add(parameter);
            if (stopped == 1)
                break;
        }

        List<string> locals = [];
        while (true)
        {
            var local = ScanPrototype(prototype, ref position, ',', ',', out stopped);
            if (local != "")
                locals.Add(local);
            if (stopped == 0)
                break;
        }

        // function name cannot redefine a protected (system) function
        // Exception: FENCE may be redefined as a user function (SNOBOL4/SPITBOL compatibility)
        var existingEntry = FunctionTable[functionName];
        if (existingEntry is not null && existingEntry.IsProtected && functionName != "FENCE")
        {
            LogRuntimeException(248);
            return;
        }

        // No second argument, or a null one: the function's own name is the entry label. The entry label need not exist yet
        // (CODE may compile it later); a call to it while it is undefined is error 286, as in SPITBOL
        entryLabel ??= functionName;

        // Build table entry
        var argumentCount = parameters.Count;
        var newEntry = new FunctionTableEntry(this, functionName, ExecuteProgramDefinedFunction, argumentCount, false);
        FunctionTable[functionName] = newEntry;

        // Build user function definition entry; a datatype of that name is a datatype no longer
        UserFunctionTable[functionName] = new UserFunctionTableEntry(functionName, prototype, parameters, locals, entryLabel);
        UserDataDefinitions.Remove(functionName);
        DataConstructors.Remove(functionName);

        PredicateSuccess();
    }

    // CALL trace, at the calling statement and level: ****11******  F('arg',7)
    private void TraceCall(string functionName, string returnVarName, List<Var> arguments, int parametersCount)
    {
        if (!TraceThisCall(TraceTableFunctionCall, functionName, out var callHandler))
            return;
        if (callHandler != null)
            CallTraceHandler(callHandler, functionName);
        else
            TraceLine($"{returnVarName}({string.Join(",", arguments.Take(parametersCount).Select(PrintValue))})");
    }

    // The bytes of SPITBOL's stack the program-defined functions being run have taken, and the bytes it starts with
    private long StackBytesUsed;
    private const long StackBaseBytes = 3216;

    // The operands of the calling expression that wait on the stack, which SPITBOL's call frame also carries: those above the
    // statement's separator, less the one that is the target of an assignment
    private int OperandsWaiting()
    {
        var waiting = 0;
        foreach (var v in SystemStack)
        {
            if (v is StatementSeparator)
                break;
            waiting++;
        }

        return Math.Max(0, waiting - 1);
    }

    internal void ExecuteProgramDefinedFunction(List<Var> arguments)
    {
        var functionName = ((StringVar)arguments[^1]).Data;
        ProgramDefinedFunctionStack.Push(functionName);
        var entry = FunctionTable[functionName];
        List<Var> saveVars = [];
        // The name each saved value is restored under. An array or table argument is bound by
        // reference and renamed to its parameter, so a saved Var's own Symbol can no longer say
        // where it came from.
        List<string> saveSymbols = [];

        // Builtin called through user-function dispatch path (e.g. *LEQ in EVAL'd pattern).
        // UserFunctionTable only holds user-DEFINE'd functions; builtins have no Symbol entry there.
        // Guard entry?.Symbol before the lookup to avoid ArgumentNullException.
        if (entry == null || entry.Symbol == null || !UserFunctionTable.ContainsKey(entry.Symbol))
        {
            ProgramDefinedFunctionStack.Pop();
            if (entry != null)
            {
                var builtinArgs = arguments.Take(arguments.Count - 1).ToList();
                entry.Handler(builtinArgs);
            }
            else
            {
                LogRuntimeException(22); // undefined function
            }
            return;
        }

        var definition = UserFunctionTable[entry.Symbol];
        if (LabelTable[definition!.EntryLabel] == GotoNotFound)
        {
            // the call is traced before the branch to the entry label that is not there, as in SPITBOL
            ProgramDefinedFunctionStack.Pop();
            TraceCall(functionName, definition.FunctionName, arguments, definition.Parameters.Count);
            LogRuntimeException(286);
            return;
        }

        // For OPSYN aliases, definition.FunctionName is the original (e.g. "fact"),
        // while functionName may be the alias (e.g. "facto").
        // The return variable is always named after the original function.
        var returnVarName = definition!.FunctionName;
        var parametersCount = definition.Parameters.Count;
        var localsCount = definition.Locals.Count;

        for (var i = 0; i < parametersCount; ++i)
        {
            // Save the current value of local variables
            var symbol = definition.Parameters[i];

            // If the identifier does not exist, make it now
            if (!IdentifierTable.ContainsKey(symbol))
            {
                IdentifierTable[symbol] = StringVar.Null();
            }

            var v = IdentifierTable[symbol];
            saveVars.Add(v);
            saveSymbols.Add(symbol);

            // Clone the argument before renaming: the caller may still hold a
            // reference to the same Var object on the SystemStack (e.g. as the
            // LHS of an assignment like "R = INC(R)").  Mutating .Symbol in-place
            // would corrupt that stack reference, causing the outer assignment to
            // write to the wrong identifier.
            // Exception: ArrayVar and TableVar are reference types in SNOBOL4 —
            // they must be passed by reference so mutations inside the function
            // are visible to the caller.  We rebind the same object under the
            // parameter name without cloning.
            Var paramVar;
            if (arguments[i] is ArrayVar or TableVar)
            {
                paramVar = Handle(arguments[i]);
                paramVar.Symbol = symbol;
            }
            else
            {
                paramVar = arguments[i].Clone();
                paramVar.Symbol = symbol;
                // an input or output association belongs to the variable, not its value, as in SPITBOL
                paramVar.InputChannel = v.InputChannel;
                paramVar.OutputChannel = v.OutputChannel;
            }
            IdentifierTable[symbol] = paramVar;
        }

        for (var i = 0; i < localsCount; ++i)
        {
            // Save the current value of local variables
            var symbol = definition.Locals[i];

            // If the identifier does not exist, make it now
            if (!IdentifierTable.ContainsKey(symbol))
            {
                IdentifierTable[symbol] = StringVar.Null();
            }

            var v = IdentifierTable[symbol];
            saveVars.Add(v);
            saveSymbols.Add(symbol);

            // Set local variables to the new values, keeping the variable's input or output association
            // (a local LIB.FILE still reads its file, as SPITBOL's LOADEX expects)
            var sVar = StringVar.Null(definition.Locals[i]);
            sVar.InputChannel = v.InputChannel;
            sVar.OutputChannel = v.OutputChannel;
            IdentifierTable[sVar.Symbol] = sVar;
        }

        // The function's own variable is saved and set null on entry like a local, and the caller's value
        // comes back on return, as in SPITBOL: F = F(N + 1) inside F keeps F's value across the inner call
        // A protected variable (BAL, ARB) is nulled too but stays protected, so assigning it in the body
        // is still error 42
        if (!saveSymbols.Contains(returnVarName))
        {
            var callerValue = IdentifierTable[returnVarName];
            saveVars.Add(callerValue);
            saveSymbols.Add(returnVarName);
            var entryValue = StringVar.Null(returnVarName);
            entryValue.IsReadOnly = callerValue.IsReadOnly;
            entryValue.InputChannel = callerValue.InputChannel;
            entryValue.OutputChannel = callerValue.OutputChannel;
            IdentifierTable[returnVarName] = entryValue;
        }

        // Save value of local variables on the stack
        entry!.StateStack.Push(saveVars);

        TraceCall(functionName, returnVarName, arguments, parametersCount);

        // SPITBOL's stack holds each call's frame, 64 bytes and 8 for each parameter, local and operand waiting in the calling
        // expression, down from the 3,216 bytes its start takes, in the -s bytes (4m by default); a call past it is error 246
        var frameBytes = 64 + 8 * (parametersCount + localsCount + OperandsWaiting());
        if (StackBytesUsed + frameBytes + StackBaseBytes > Parent.BuildOptions.StackSizeBytes
            || !RuntimeHelpers.TryEnsureSufficientExecutionStack())
        {
            RunLeftAtOnce = true;
            LogRuntimeException(246, fatal: true);
        }
        StackBytesUsed += frameBytes;

        // Update keywords
        AmpFunctionLevel++;
        TraceKeyword("FNCLEVEL", AmpFunctionLevel);

        // Monitor bridge — CALL event (mirrors csn DEFF18 / spl bpf09 predecessor).
        MonitorIpc.EmitCall(functionName);

        // Run function by transferring to the entry label.
        // Save caller's Failure state: ThreadedExecuteLoop restores it internally,
        // but we must not let RETURN unconditionally force Failure=false and clobber
        // the pre-call state (e.g. a prior LT() failure that a :F(label) depends on).
        var callerFailure = Failure;
        // Every statement of the body clears AlphaStack and BetaStack, so a call made during a
        // match (*F(X) . V) would discard the caller's pending conditional assignments
        var callerAlphaStack = AlphaStack;
        var callerBetaStack = BetaStack;
        AlphaStack = [];
        BetaStack = [];
        // On return the calling statement is the current one again, so the statement after it
        // reads it as &LASTNO, as in SPITBOL
        var callerStatement = AmpCurrentLineNumber;
        var callerSerial = StatementSerial;
        int nextIndex;
        try
        {
            nextIndex = ExecuteLoop(LabelTable[definition.EntryLabel]);
        }
        finally
        {
            StackBytesUsed -= frameBytes;
        }

        // A goto to END inside the body ends the whole program, as in SPITBOL, not just the call; the END
        // statement stays the current one (&STNO, &LINE) for the end-of-run dump
        if (nextIndex == -1)
            throw new ProgramEndException();

        var returningStatement = AmpCurrentLineNumber;
        // The call's last statement is the last executed, so the rest of the calling statement (and a statement a trace call
        // interrupts) reads it as &LASTNO, as in SPITBOL; the calling statement is the current one again
        AmpLastLineNumber = returningStatement;
        AmpCurrentLineNumber = callerStatement;
        StatementSerial = callerSerial;
        AlphaStack = callerAlphaStack;
        BetaStack = callerBetaStack;
        var returnVar = IdentifierTable[returnVarName];

        string rtnTypeStr;
        switch (nextIndex)
        {
            case -2:
                AmpReturnType = "RETURN";
                rtnTypeStr = "RETURN";
                returnVar.Succeeded = true;
                ValueReturned = returnVar;
                Failure = callerFailure;   // preserve caller's Failure; UDF success doesn't clear it
                break;

            case -3:
                AmpReturnType = "FRETURN";
                rtnTypeStr = "FRETURN";
                returnVar = StringVar.Null(returnVarName);
                Failure = true;            // FRETURN explicitly signals failure
                FailureOrigin = SystemStack.Count;   // where the call's value is pushed below
                break;

            case -4:
            default:
                AmpReturnType = "NRETURN";
                rtnTypeStr = "NRETURN";
                Failure = callerFailure;   // NRETURN is neutral — restore caller's state
                break;
        }

        // Monitor bridge — RETURN event (mirrors csn DEFF20 / spl retrn body).
        // Per spl/csn convention, RETURN payload carries only the return type
        // (RETURN/FRETURN/NRETURN); the function's actual return value was
        // already emitted as a VALUE record when the body assigned to the
        // function-name variable (e.g. "foo = expr").  Bare returns (no
        // body-assign to fn-name) emit no VALUE here, matching spl.
        // See spl runtime comment at monitor_ipc_runtime.c:449-452 and
        // GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan.
        MonitorIpc.EmitReturn(functionName, rtnTypeStr);

        AmpFunctionLevel--;

        // &FNCLEVEL and then RETURN traces, in SPITBOL's order (retrn: ktrex first), at the returning
        // statement and the caller's level: ****15******  RETURN F = 1, FRETURN F, NRETURN G = .Z
        var current = AmpCurrentLineNumber;
        AmpCurrentLineNumber = returningStatement;
        TraceKeyword("FNCLEVEL", AmpFunctionLevel);
        if (TraceThisCall(TraceTableFunctionReturn, functionName, out var returnHandler))
        {
            if (returnHandler != null)
                CallTraceHandler(returnHandler, functionName);
            else
                TraceLine(rtnTypeStr == "FRETURN" ? $"FRETURN {returnVarName}" : $"{rtnTypeStr} {returnVarName} = {PrintValue(returnVar)}");
        }
        AmpCurrentLineNumber = current;

        // Clear the function name variable once the RETURN trace (whose function may read it) is done
        IdentifierTable[returnVarName] = StringVar.Null(returnVarName);

        // Restore the caller's variables under their own names, last saved first, so a name that
        // is both a parameter and a local unwinds to the caller's value
        saveVars = entry.StateStack.Pop();

        for (var i = saveVars.Count - 1; i >= 0; --i)
        {
            saveVars[i].Symbol = saveSymbols[i];
            IdentifierTable[saveSymbols[i]] = saveVars[i];
        }

        // A NAME an NRETURN hands back gives the named variable's value, which the statement may also
        // assign (F() = V) or name (.F()), as in SPITBOL; a RETURN of a NAME gives the NAME itself.  The
        // value is read once the caller's variables are back, so a name of a local or a parameter gives
        // the caller's value of it, as SPITBOL dereferences the name after the return.  A string or number
        // an NRETURN hands back names the variable it spells, as $ would (F = 'ZZ' :(NRETURN) names ZZ)
        SystemStack.Push(rtnTypeStr == "NRETURN" && (returnVar as NameVar ?? SpelledName(returnVar)) is { } returnedName
            ? NameReturnValue(returnedName)
            : returnVar);

        // Pop ProgramDefinedFunctionStack — balances the Push at the top of this method.
        // Without this Pop, Peek() returns stale data after the function returns,
        // breaking the body-assign isReturnSlot suppression in nested-call scenarios.
        // See GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan.
        ProgramDefinedFunctionStack.Pop();
    }

    // The variable a string, integer or real spells, as the indirection operator reads it, or null for any other value or the null string
    private NameVar? SpelledName(Var value)
    {
        var spelling = value switch
        {
            StringVar s => s.Data,
            IntegerVar i => i.Data.ToString(CultureInfo.InvariantCulture),
            RealVar r => r.Data.ToString(CultureInfo.InvariantCulture),
            _ => ""
        };
        return spelling == "" ? null : new NameVar(FoldName(spelling), null, null);
    }

    // A call or return is traced when &FTRACE is on (each line counts it down) or when TRACE named the
    // function for this event and &TRACE is on (each line counts &TRACE down), as in SPITBOL
    // An &FTRACE line is always printed; a TRACE'd call or return may name a trace function instead
    private bool TraceThisCall(Dictionary<string, TraceHandler?> table, string functionName, out TraceHandler? handler)
    {
        handler = null;
        if (AmpFunctionTrace > 0)
        {
            AmpFunctionTrace--;
            return true;
        }

        if (AmpTrace <= 0 || !table.TryGetValue(functionName, out handler))
            return false;

        AmpTrace--;
        return true;
    }
}
