namespace Snobol4.Common;

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

        // define argument cannot be null
        if (arguments.Count == 0)
        {
            LogRuntimeException(82);
            return;
        }

        // define argument must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(81);
            return;
        }

        var prototype = Parent.FoldCase((string)value).Trim();

        // define argument cannot be null string
        if (prototype == "")
        {
            LogRuntimeException(81);
            return;
        }

        // define argument must not have a null datatype name
        var match = CompiledRegex.FunctionPrototypePattern().Match(prototype);
        var functionName = match.Groups[1].Value.Trim();

        // function name cannot be null
        if (functionName == "")
        {
            LogRuntimeException(84);
            return;
        }

        // function name cannot redefine a protected (system) function
        // Exception: FENCE may be redefined as a user function (SNOBOL4/SPITBOL compatibility)
        var existingEntry = FunctionTable[functionName];
        if (existingEntry is not null && existingEntry.IsProtected && functionName != "FENCE")
        {
            LogRuntimeException(248);
            return;
        }

        // must have a left paren
        var lParen = match.Groups[2].Value;
        if (lParen == "")
        {
            LogRuntimeException(83);
            return;
        }

        // must have a right paren
        var rParen = match.Groups[4].Value;
        if (rParen == "")
        {
            LogRuntimeException(85);
            return;
        }

        // Get entry label
        string entryLabel;
        if (arguments.Count < 2 || arguments[1] is StringVar { Data: "" })
        {
            // No second arg or empty string → use function name as entry label
            entryLabel = functionName;
        }
        else if (arguments[1] is StringVar strArg)
        {
            // String second arg → use it directly as label name
            entryLabel = Parent.FoldCase(strArg.Data.Trim());
        }
        else
        {
            // Name/indirect reference (.label)
            if (!arguments[1].Convert(VarType.NAME, out var entry, out _, this))
            {
                LogRuntimeException(86);
                return;
            }
            entryLabel = ((NameVar)entry).Pointer;
        }

        //if (!LabelTable.ContainsKey(entryLabel))
        if (LabelTable[entryLabel] == GotoNotFound)
        {
            LogRuntimeException(86);
        }

        // Build table entry
        var tempParameters = new List<string>(match.Groups[3].Value.Split(','));
        List<string> parameters = new();
        parameters.AddRange(tempParameters.Select(t => t.Trim()).Where(parameter => parameter != ""));
        var tempLocals = new List<string>(match.Groups[5].Value.Split(','));
        List<string> locals = new();
        locals.AddRange(tempLocals.Select(t => t.Trim()).Where(local => local != ""));
        var argumentCount = parameters.Count;
        var newEntry = new FunctionTableEntry(this, functionName, ExecuteProgramDefinedFunction, argumentCount, false);
        FunctionTable[functionName] = newEntry;

        // Build user function definition entry
        UserFunctionTable[functionName] = new UserFunctionTableEntry(functionName, prototype, parameters, locals, entryLabel);

        PredicateSuccess();
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
                paramVar = arguments[i];
                paramVar.Symbol = symbol;
            }
            else
            {
                paramVar = arguments[i].Clone();
                paramVar.Symbol = symbol;
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

            // Set local variables to the new values
            var sVar = StringVar.Null(definition.Locals[i]);
            IdentifierTable[sVar.Symbol] = sVar;
        }

        // Save value of local variables on the stack
        entry!.StateStack.Push(saveVars);

        // CALL trace, at the calling statement and level: ****11******  F('arg',7)
        var traceCall = TraceThisCall(TraceTableFunctionCall, functionName);
        if (traceCall)
            TraceLine($"{functionName}({string.Join(",", arguments.Take(parametersCount).Select(a => a.DumpString()))})");

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
        var nextIndex = ExecuteLoop(LabelTable[definition.EntryLabel]);
        var returningStatement = AmpCurrentLineNumber;
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
                Failure = callerFailure;   // preserve caller's Failure; UDF success doesn't clear it
                break;

            case -3:
                AmpReturnType = "FRETURN";
                rtnTypeStr = "FRETURN";
                returnVar = StringVar.Null(returnVarName);
                Failure = true;            // FRETURN explicitly signals failure
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

        // A NAME an NRETURN hands back is an assignment target in this statement (F() = V)
        if (returnVar is NameVar)
            returnVar.LvalueMark = StatementSerial;
        SystemStack.Push(returnVar);
        IdentifierTable[returnVarName] = StringVar.Null(returnVarName); // Clear function name variable
        AmpFunctionLevel--;

        // RETURN and &FNCLEVEL traces, at the returning statement and the caller's level:
        // ****15******  RETURN F = 1, FRETURN F, NRETURN G = .Z
        var current = AmpCurrentLineNumber;
        AmpCurrentLineNumber = returningStatement;
        if (TraceThisCall(TraceTableFunctionReturn, functionName))
            TraceLine(rtnTypeStr == "FRETURN" ? $"FRETURN {functionName}" : $"{rtnTypeStr} {functionName} = {returnVar.DumpString()}");
        TraceKeyword("FNCLEVEL", AmpFunctionLevel);
        AmpCurrentLineNumber = current;

        // Restore the caller's variables under their own names, last saved first, so a name that
        // is both a parameter and a local unwinds to the caller's value
        saveVars = entry.StateStack.Pop();

        for (var i = saveVars.Count - 1; i >= 0; --i)
        {
            saveVars[i].Symbol = saveSymbols[i];
            IdentifierTable[saveSymbols[i]] = saveVars[i];
        }

        // Pop ProgramDefinedFunctionStack — balances the Push at the top of this method.
        // Without this Pop, Peek() returns stale data after the function returns,
        // breaking the body-assign isReturnSlot suppression in nested-call scenarios.
        // See GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan.
        ProgramDefinedFunctionStack.Pop();
    }

    // A call or return is traced when &FTRACE is on (each line counts it down) or when TRACE named the
    // function for this event and &TRACE is on (each line counts &TRACE down), as in SPITBOL
    private bool TraceThisCall(Dictionary<string, string> table, string functionName)
    {
        if (AmpFunctionTrace > 0)
        {
            AmpFunctionTrace--;
            return true;
        }

        if (AmpTrace <= 0 || !table.ContainsKey(functionName))
            return false;

        AmpTrace--;
        return true;
    }
}
