namespace Snobol4.Common;

//"prototype argument is not valid object" /* 164 */,

public partial class Executive
{
    internal void Prototype(List<Var> arguments)
    {
        switch (arguments[0])
        {
            // A one-dimensional array given without a lower bound (ARRAY(3), ARRAY('3')) is a SPITBOL
            // vector, whose prototype is its size as an integer
            case ArrayVar { Dimensions: 1, ExplicitLower: [false] } vector:
                SystemStack.Push(new IntegerVar(vector.Sizes[0]));
                return;
            case ArrayVar array:
                SystemStack.Push(new StringVar(array.Prototype));
                return;
            case TableVar:
                SystemStack.Push(StringVar.Null());
                return;
            default:
                LogRuntimeException(164);
                break;
        }
    }
}