using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snobol4.Common;

//"field function argument is wrong datatype" /* 41 */
//"data argument is not a string" /* 75 */
//"data argument is null" /* 76 */
//"data argument is missing a left paren" /* 77 */
//"data argument has null datatype name" /* 78 */
//"data argument is missing a right paren" /* 79 */
//"data argument has null field name" /* 80 */
//"prototype argument is not valid object" /* 164 */,
//"attempted redefinition of system function" /* 248 */

public partial class Executive
{
    internal void ProgramDefinedData(List<Var> arguments)
    {
        // data argument cannot be null
        if (arguments.Count == 0)
        {
            LogRuntimeException(76);
            return;
        }

        // data argument must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(75);
            return;
        }

        // data argument cannot be null
        var prototype = ((string)value).Trim();
        if (prototype == "")
        {
            LogRuntimeException(76);
            return;
        }

        // data argument must not have a null datatype name
        var match = CompiledRegex.ProgramDefinedDataPrototypePattern().Match(prototype);
        if (!match.Success)
        {
            Assert.IsTrue(true, "prototype argument is not valid object in ProgramDefinedData()");
            LogRuntimeException(164);
            return;
        }

        // datatype name cannot be null
        var dataName = match.Groups[1].Value.Trim();
        if (dataName == "")
        {
            LogRuntimeException(78);
            return;
        }

        // datatype name cannot be an existing function
        if (FunctionTable.ContainsKey(dataName))
        {
            LogRuntimeException(248);
            return;
        }

        // must have a left paren
        var lParen = match.Groups[2].Value;
        if (lParen == "")
        {
            LogRuntimeException(77);
            return;
        }

        // must have a left paren
        var rParen = match.Groups[4].Value;
        if (rParen == "")
        {
            LogRuntimeException(79);
            return;
        }

        var fields = new List<string>(match.Groups[3].Value.Split(','));
        for (var i = 0; i < fields.Count; i++)
        {
            fields[i] = fields[i].Trim();

            // field name cannot be null
            if (fields[i] == "")
            {
                LogRuntimeException(80);
                return;
            }

            // field name cannot be an existing function
            if (FunctionTable.ContainsKey(fields[i]))
            {
                LogRuntimeException(248);
                return;
            }
        }

        FunctionTable[dataName] = new FunctionTableEntry(dataName, CreateProgramDefinedDataInstance, fields.Count, fields, prototype);

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
        Dictionary<string, Var> userDefinedFields = [];

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