namespace Snobol4.Common;

//"field function argument is wrong datatype" /* 41 */
//"field second argument is not integer" /* 107 */
//"field first argument is not datatype name" /* 108 */

public partial class Executive
{
    internal void Field(List<Var> arguments)
    {
        // field first argument must be a datatype name
        if (arguments[0] is not ProgramDefinedDataVar dataObj)
        {
            LogRuntimeException(41);
            return;
        }

        // field second argument must be an integer
        if (!arguments[1].Convert(VarType.INTEGER, out _, out var indexObj, this))
        {
            LogRuntimeException(107);
            return;
        }

        if (!FunctionTable.TryGetValue(dataObj.TypeName, out var function))
        {
            LogRuntimeException(108);
            return;
        }

        var functionEntry = (FunctionTableEntry)function;
        var index = (int)(long)indexObj! - 1;
        if (index < 0 || index >= dataObj.FieldCount)
        {
            Failure = true;
            SystemStack.Push(StringVar.Null());
            return;
        }

        SystemStack.Push(new StringVar(functionEntry.Locals[index]));
    }
}