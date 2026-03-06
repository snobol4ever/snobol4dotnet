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

        // function name cannot be an existing function
        //if (FunctionTable.ContainsKey(functionName))
        if (FunctionTable[functionName] is not null)
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
        if (arguments[1] is StringVar entryVar && entryVar.Data == "")
        {
            entryLabel = functionName;
        }
        else
        {
            if (!arguments[1].Convert(VarType.NAME, out var entry, out _, this))
            {
                LogRuntimeException(86);
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
        var argumentCount = locals.Count;
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
        var parametersCount = definition!.Parameters.Count;
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

            // Set local variables to the new values
            arguments[i].Symbol = symbol;
            IdentifierTable[arguments[i].Symbol] = arguments[i];
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

        // Run function by transferring to the entry label
        var nextIndex = ExecuteLoop(LabelTable[definition.EntryLabel]);
        var returnVar = IdentifierTable[functionName];

        switch (nextIndex)
        {
            case -2:
                AmpReturnType = "RETURN";
                returnVar.Succeeded = true;
                Failure = false;
                break;

            case -3:
                AmpReturnType = "FRETURN";
                returnVar = StringVar.Null(functionName);
                Failure = true;
                break;

            case -4:
                AmpReturnType = "NRETURN";
                break;
        }

        SystemStack.Push(returnVar);
        IdentifierTable[functionName] = StringVar.Null(functionName); // Clear function name variable
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