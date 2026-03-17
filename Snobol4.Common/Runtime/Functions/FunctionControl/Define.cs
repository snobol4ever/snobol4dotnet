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
        var existingEntry = FunctionTable[functionName];
        if (existingEntry is not null && existingEntry.IsProtected)
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
        var definition = UserFunctionTable[entry?.Symbol!];
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

            // Clone the argument before renaming: the caller may still hold a
            // reference to the same Var object on the SystemStack (e.g. as the
            // LHS of an assignment like "R = INC(R)").  Mutating .Symbol in-place
            // would corrupt that stack reference, causing the outer assignment to
            // write to the wrong identifier.
            var paramVar = arguments[i].Clone();
            paramVar.Symbol = symbol;
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

            // Set local variables to the new values
            var sVar = StringVar.Null(definition.Locals[i]);
            IdentifierTable[sVar.Symbol] = sVar;
        }

        // Save value of local variables on the stack
        entry!.StateStack.Push(saveVars);

        // Update keywords
        AmpFunctionLevel++;

        if (AmpFunctionTrace > 0)
        {
            FunctionTraceEntry(arguments, functionName);
        }

        // Run function by transferring to the entry label.
        // Save caller's Failure state: ThreadedExecuteLoop restores it internally,
        // but we must not let RETURN unconditionally force Failure=false and clobber
        // the pre-call state (e.g. a prior LT() failure that a :F(label) depends on).
        var callerFailure = Failure;
        var nextIndex = ExecuteLoop(LabelTable[definition.EntryLabel]);
        var returnVar = IdentifierTable[returnVarName];

        switch (nextIndex)
        {
            case -2:
                AmpReturnType = "RETURN";
                returnVar.Succeeded = true;
                Failure = callerFailure;   // preserve caller's Failure; UDF success doesn't clear it
                break;

            case -3:
                AmpReturnType = "FRETURN";
                returnVar = StringVar.Null(returnVarName);
                Failure = true;            // FRETURN explicitly signals failure
                break;

            case -4:
                AmpReturnType = "NRETURN";
                Failure = callerFailure;   // NRETURN is neutral — restore caller's state
                break;
        }

        SystemStack.Push(returnVar);
        IdentifierTable[returnVarName] = StringVar.Null(returnVarName); // Clear function name variable
        AmpFunctionLevel--;

        // Post-processing
        if (AmpFunctionTrace > 0)
        {
            FunctionTraceExit(functionName);
        }

        // Restore local variables
        saveVars = entry.StateStack.Pop();

        foreach (var saveVar in saveVars)
        {
            IdentifierTable[saveVar.Symbol] = saveVar;
        }
    }

    private void FunctionTraceEntry(List<Var> arguments, string functionName)
    {
        var line = (int)SourceLineNumbers[AmpCurrentLineNumber - 1];
        var linePad = line.ToString().PadRight(8, '*');
        var r = (int)AmpFunctionLevel;
        var args = "";

        for (var i = 1; i < arguments.Count; ++i)
        {
            args += (i > 1 ? ", " : "") + arguments[i - 1];
        }

        Console.Error.WriteLine($@"****{linePad} {new string('i', r - 1)} {functionName}({args})");
        AmpFunctionTrace--;
    }

    private void FunctionTraceExit(string functionName)
    {
        var line = AmpFunctionTrace + 1;
        var linePad = line.ToString().PadRight(8, '*');
        var r = (int)AmpFunctionLevel;
        Console.Error.WriteLine($@"****{linePad} {new string('i', r)} return {functionName} = {SystemStack.Peek()}");
        AmpFunctionTrace--;
    }

}