using Microsoft.CodeAnalysis.CSharp.Syntax;
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


public class UserDataDefinition
{
    internal string Prototype;
    internal List<string> FieldNames;
    
    internal UserDataDefinition(string prototype, List<string> fieldNames)
    {
        Prototype = prototype;
        FieldNames = fieldNames;
    }
}

public partial class Executive
{
    internal Dictionary<string,UserDataDefinition> UserDataDefinitions = new();

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
            Assert.IsTrue(true, "prototype argument is not valid object in Data()");
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

        // must have a right paren
        var rParen = match.Groups[4].Value;
        if (rParen == "")
        {
            LogRuntimeException(79);
            return;
        }

        // TODO Does this ignore consecutive commas like DEFINE
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


        FunctionTable[dataName] = new FunctionTableEntry(dataName, CreateProgramDefinedDataInstance, fields.Count, false);
        UserDataDefinition userDataDefinition = new UserDataDefinition(prototype, fields);
        UserDataDefinitions[dataName] = userDataDefinition;

        foreach (var fieldName in fields)
        {
            FunctionTable[fieldName] = new FunctionTableEntry(fieldName, GetProgramDefinedDataField, 1, false);
        }

        PredicateSuccess();
    }

    internal void CreateProgramDefinedDataInstance(List<Var> arguments)
    {
        var dataName = ((StringVar)arguments[^1]).Data;
        var dataDefinition = UserDataDefinitions[dataName];
        arguments = arguments[..^1];
        var userDefinedDataVar = new ProgramDefinedDataVar(dataName, dataDefinition.Prototype, dataDefinition.FieldNames);
        var fieldsCount = dataDefinition.FieldNames.Count;
        var fieldValues = userDefinedDataVar.FieldValues.Data;

        for (var i = 0; i < fieldsCount; i++)
        {
            fieldValues[i] = arguments[i];
        }

        SystemStack.Push(userDefinedDataVar);
    }

    internal void GetProgramDefinedDataField(List<Var> arguments)
    {
        var fieldName = ((StringVar)arguments[1]).Data;
        var programDefinedDataVar = (ProgramDefinedDataVar)arguments[0];
        object index = (long)programDefinedDataVar.Definition.FieldNames.IndexOf(fieldName);
        var v = programDefinedDataVar.FieldValues.Data[(int)(long)index];
        v.Key = index;
        v.Collection = programDefinedDataVar.FieldValues;
        SystemStack.Push(v);
    }
}