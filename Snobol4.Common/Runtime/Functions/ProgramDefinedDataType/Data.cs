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
        // data argument cannot be null (76) and must be a string (75)
        if (arguments.Count == 0)
        {
            LogRuntimeException(76);
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var value, this))
        {
            LogRuntimeException(75);
            return;
        }

        if ((string)value == "")
        {
            LogRuntimeException(76);
            return;
        }

        // The prototype is scanned as SPITBOL scans it: the datatype's name up to the left paren (77 when there is none, 78 when it
        // is null), then the field names up to the right paren, each ended by a comma or the paren (79 at the end of the string,
        // 80 for a null name)
        var prototype = Parent.FoldCase((string)value).Trim();
        var position = 0;
        var dataName = ScanPrototype(prototype, ref position, '(', '(', out var stopped);
        if (stopped == 0)
        {
            LogRuntimeException(77);
            return;
        }

        if (dataName == "")
        {
            LogRuntimeException(78);
            return;
        }

        var fields = new List<string>();
        while (true)
        {
            var field = ScanPrototype(prototype, ref position, ')', ',', out stopped);
            if (stopped == 0)
            {
                LogRuntimeException(79);
                return;
            }

            if (field == "")
            {
                LogRuntimeException(80);
                return;
            }

            fields.Add(field);
            if (stopped == 1)
                break;
        }

        // a datatype or field name that is a system function's cannot be defined (248)
        foreach (var fieldName in fields)
        {
            var existingField = FunctionTable[fieldName];
            if (existingField is not null && existingField.IsProtected)
            {
                LogRuntimeException(248);
                return;
            }
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

            if (DataConstructors.TryGetValue(dataName, out var constructor) && existingDataType.Handler == constructor
                && UserDataDefinitions.TryGetValue(dataName, out var existingDefinition)
                && existingDefinition.Prototype == prototype)
            {
                PredicateSuccess();
                return;
            }
        }


        // The constructor and field functions carry their own names, so a synonym OPSYN makes (QLINK for
        // COUNT) still builds that type and reads that field, as in SPITBOL
        // a datatype's constructor and its field functions replace any program-defined function of the same name
        UserFunctionTable.Remove(dataName);
        foreach (var fieldName in fields)
            UserFunctionTable.Remove(fieldName);

        FunctionTableEntry.FunctionHandler create = a => CreateProgramDefinedDataInstance(a, dataName);
        DataConstructors[dataName] = create;
        FunctionTable[dataName] = new FunctionTableEntry(this, dataName, create, fields.Count, false);
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
                FunctionTable[fieldName] = new FunctionTableEntry(this, fieldName, a => GetProgramDefinedDataField(a, fieldName), 1, false);
            }
        }

        PredicateSuccess();
    }

    // Each DATA type's constructor, to tell a repeated identical DATA call from a redefinition
    private readonly Dictionary<string, FunctionTableEntry.FunctionHandler> DataConstructors = [];

    internal void CreateProgramDefinedDataInstance(List<Var> arguments, string dataName)
    {
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

    internal void GetProgramDefinedDataField(List<Var> arguments) =>
        GetProgramDefinedDataField(arguments, ((StringVar)arguments[1]).Data);

    internal void GetProgramDefinedDataField(List<Var> arguments, string fieldName)
    {
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
            // A field function applied to anything but a DATA object is error 41, as in SPITBOL
            if (arg0 is not ProgramDefinedDataVar pdFallback)
            {
                LogRuntimeException(41);
                return;
            }
            programDefinedDataVar = pdFallback;
        }

        // so is one applied to an object of a type without that field (F(Q(1)) with F a field of P)
        var fieldIndex = programDefinedDataVar.Definition.FieldNames.IndexOf(fieldName);
        if (fieldIndex < 0)
        {
            LogRuntimeException(41);
            return;
        }

        object index = (long)fieldIndex;
        var raw = programDefinedDataVar.FieldValues.Data[(int)(long)index];
        // Clone the field-slot value before stamping lvalue bookkeeping (Key/Collection) onto
        // it. Without this, the live field-slot object accumulates bookkeeping from each caller,
        // which confuses subsequent reads and creates NameVar → ArrayVar → NameVar cycles when
        // the bookmarked object is later stored back into another DATA field (ShiftReduce S-10).
        // ArrayVar, TableVar, ProgramDefinedDataVar have reference semantics — do not clone them.
        var v = raw is ArrayVar or TableVar ? Handle(raw) : raw is ProgramDefinedDataVar ? raw : raw.Clone();
        v.Key = index;
        v.Collection = programDefinedDataVar.FieldValues;
        v.LvalueMark = StatementSerial;
        SystemStack.Push(v);
    }
}