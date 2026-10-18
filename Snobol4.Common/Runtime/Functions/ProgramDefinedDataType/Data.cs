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
    internal Dictionary<string, UserDataDefinition> UserDataDefinitions = new();

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
        var prototype = Parent.FoldCase((string)value).Trim();
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

        // datatype name cannot be an existing user-defined function
        // (protected builtins may be shadowed by DATA type names, per SNOBOL4 spec)
        // A system function cannot be redefined (error 248); a program-defined function or DATA type
        // can, as in SPITBOL. The same prototype again (EVAL re-running an include's initialisation)
        // changes nothing.
        var existingDataType = FunctionTable[dataName];
        if (existingDataType is not null)
        {
            if (existingDataType.IsProtected)
            {
                LogRuntimeException(248);
                return;
            }

            if (existingDataType.Handler == CreateProgramDefinedDataInstance
                && UserDataDefinitions.TryGetValue(dataName, out var existingDefinition)
                && existingDefinition.Prototype == prototype)
            {
                PredicateSuccess();
                return;
            }
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
            fields[i] = Parent.FoldCase(fields[i].Trim());

            // field name cannot be null
            if (fields[i] == "")
            {
                LogRuntimeException(80);
                return;
            }

            // nor can a field name be a system function's
            var existingField = FunctionTable[fields[i]];
            if (existingField is not null && existingField.IsProtected)
            {
                LogRuntimeException(248);
                return;
            }
        }


        FunctionTable[dataName] = new FunctionTableEntry(this, dataName, CreateProgramDefinedDataInstance, fields.Count, false);
        var userDataDefinition = new UserDataDefinition(prototype, fields);
        UserDataDefinitions[dataName] = userDataDefinition;

        foreach (var fieldName in fields)
        {
            var existingEntry = FunctionTable[fieldName];
            if (existingEntry is null || !existingEntry.IsProtected)
            {
                // Only register if no protected builtin owns this name.
                // Protected builtins (e.g. VALUE) keep their slot; GetProgramDefinedDataField
                // dispatches by argument type at runtime so field access still works.
                FunctionTable[fieldName] = new FunctionTableEntry(this, fieldName, GetProgramDefinedDataField, 1, false);
            }
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
            // Strip array/field lvalue bookkeeping (Key/Collection) before storing.
            // AssignReplace stamps destination Key/Collection onto cloned values so
            // they serve as lvalues; storing that into a DATA field causes an infinite
            // dereference cycle in GetProgramDefinedDataField. NameVars are preserved.
            var fv = arguments[i];
            if (fv is not NameVar && fv.Collection != null)
            {
                fv = fv.Clone();
                fv.Key        = null;
                fv.Collection = null;
            }
            fieldValues[i] = fv;
        }

        SystemStack.Push(userDefinedDataVar);
    }

    internal void GetProgramDefinedDataField(List<Var> arguments)
    {
        var fieldName = ((StringVar)arguments[1]).Data;
        // Resolve argument to ProgramDefinedDataVar, handling the Top()/NRETURN NameVar chain.
        // Top() returns .value($'@S') — a pointer NameVar. Deref gives value-field contents,
        // which may itself be a collection-backed NameVar (Collection=tree ProgramDefinedDataVar,
        // Key=field-index) when Push stored a name via NRETURN. In that case the Collection IS
        // the node we want — reading through the field slot would give 'v's current value, not
        // the node, and cycling back to another NameVar. Use Collection directly instead.
        Var arg0 = arguments[0];
        if (arg0 is NameVar nv0 && nv0.Collection is null)
            arg0 = nv0.Dereference(this);
        ProgramDefinedDataVar programDefinedDataVar;
        if (arg0 is NameVar nv1 && nv1.Collection is ProgramDefinedDataVar pdFromCollection)
            programDefinedDataVar = pdFromCollection;
        else if (arg0 is ProgramDefinedDataVar pdDirect)
            programDefinedDataVar = pdDirect;
        else
        {
            for (int g = 0; g < 8 && arg0 is NameVar nvX; g++)
                arg0 = nvX.Dereference(this);
            if (arg0 is not ProgramDefinedDataVar pdFallback) {
                // arg0 is not a DATA instance (e.g. PatternVar during build-time pattern
                // construction). Signal FRETURN — caller must handle failure branch.
                Failure = true;
                SystemStack.Push(StringVar.Null());
                return;
            }
            programDefinedDataVar = pdFallback;
        }
        object index = (long)programDefinedDataVar.Definition.FieldNames.IndexOf(fieldName);
        var raw = programDefinedDataVar.FieldValues.Data[(int)(long)index];
        // Clone the field-slot value before stamping lvalue bookkeeping (Key/Collection) onto
        // it. Without this, the live field-slot object accumulates bookkeeping from each caller,
        // which confuses subsequent reads and creates NameVar → ArrayVar → NameVar cycles when
        // the bookmarked object is later stored back into another DATA field (ShiftReduce S-10).
        // ArrayVar, TableVar, ProgramDefinedDataVar have reference semantics — do not clone them.
        var v = raw is ArrayVar or TableVar or ProgramDefinedDataVar ? raw : raw.Clone();
        v.Key = index;
        v.Collection = programDefinedDataVar.FieldValues;
        v.LvalueMark = StatementSerial;
        SystemStack.Push(v);
    }
}