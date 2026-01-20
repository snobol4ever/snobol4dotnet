namespace Snobol4.Common;

//"field function argument is wrong datatype" /* 41 */
//"data argument is not a string" /* 75 */
//"data argument is null" /* 76 */ (NOT USED - COMBINED WITH 164 in a single Regex of prototype)
//"data argument is missing a left paren" /* 77 */ (NOT USED - COMBINED WITH 164 in a single Regex of prototype)
//"data argument has null datatype name" /* 78 */ (NOT USED - COMBINED WITH 164 in a single Regex of prototype)
//"data argument is missing a right paren" /* 79 */ (NOT USED - COMBINED WITH 164 in a single Regex of prototype)
//"data argument has null field name" /* 80 */ (NOT USED - COMBINED WITH 164 in Regex of field name)
//"prototype argument is not valid object" /* 164 */,

public partial class Executive
{
    internal void ProgramDefinedData(List<Var> arguments)
    {
        // data argument cannot be null
        //if (arguments.Count == 0)
        //{
        //    LogRuntimeException(76);
        //    return;
        //}

        // data argument must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(75);
            return;
        }

        // data argument must not have a null datatype name
        var prototype = (string)value;
        var match = CompiledRegex.ProgramDefinedDataPrototypePattern().Match(prototype);
        if (!match.Success)
        {
            LogRuntimeException(164);
            return;
        }

        var dataName = match.Groups[1].Value;
        var fields = new List<string>(match.Groups[2].Value.Split(','));

        foreach (var str in fields)
        {
            // field must be non-null
            //if (str.Length == 0)
            //{
            //    LogRuntimeException(80);
            //    return;
            //}

            if (CompiledRegex.IdentifierPattern().Match(str).Success)
                continue;

            // field name must be a valid identifier
            LogRuntimeException(168);
            return;
        }

        var newEntry = new FunctionTableEntry(dataName, CreateProgramDefinedDataInstance, fields.Count, fields, prototype);
        FunctionTable[dataName] = newEntry;

        // Field references cannot overwrite system variables
        foreach (var field in fields)
        {
            FunctionTable[field] = new FunctionTableEntry(field, GetProgramDefinedDataField, 1, false);
        }

        PredicateSuccess();
    }

    internal void CreateProgramDefinedDataInstance(List<Var> arguments)
    {
        if (arguments[^1] is not StringVar functionName)
        {
            LogRuntimeException(22);
            return;
        }

        var dataName = ((StringVar)arguments[^1]).Data;
        var fields = FunctionTable[functionName.Data].Locals;
        arguments = arguments[..^1];
        Dictionary<string, Var> userDefinedFields = new();

        // Field names must be a non-empty string
        for (var i = 0; i < arguments.Count; ++i)
        {
            userDefinedFields[fields[i]] = arguments[i];
        }

        var userDefinedDataVar = new ProgramDefinedDataVar(dataName, userDefinedFields);
        SystemStack.Push(userDefinedDataVar);
    }

    internal void GetProgramDefinedDataField(List<Var> arguments)
    {
        if (arguments[0] is not ProgramDefinedDataVar programDefinedDataVar)
        {
            LogRuntimeException(41);
            return;
        }

        if (!arguments[1].Convert(VarType.STRING, out _, out var field, this))
        {
            LogRuntimeException(75);
            return;
        }

        SystemStack.Push(programDefinedDataVar.ProgramDefinedData[(string)field]);
    }
}