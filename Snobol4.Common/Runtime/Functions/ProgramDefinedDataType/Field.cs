namespace Snobol4.Common;

//"field second argument is not integer" /* 107 */
//"field first argument is not datatype name" /* 108 */

public partial class Executive
{
    internal void Field(List<Var> arguments)
    {
        // SPITBOL takes the second argument first: a number out of range fails before the first one is looked at
        if (!SmallInteger(arguments[1], out var index, out var inRange))
        {
            LogRuntimeException(107);
            return;
        }

        if (!inRange)
        {
            NonExceptionFailure();
            return;
        }

        // the first argument must be the name of a datatype
        if (!NaturalVariableName(arguments[0], out var datatype) || !UserDataDefinitions.TryGetValue(datatype, out var definition))
        {
            LogRuntimeException(108);
            return;
        }

        if (index <= 0 || index > definition.FieldNames.Count)
        {
            NonExceptionFailure();
            return;
        }

        // the field's NAME, as in SPITBOL (DATATYPE(FIELD('D',1)) is NAME)
        SystemStack.Push(new NameVar(definition.FieldNames[(int)index - 1], null, null));
    }
}
