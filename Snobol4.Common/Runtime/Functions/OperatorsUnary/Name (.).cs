namespace Snobol4.Common;


// TODO: Determine error codes

public partial class Executive
{
    internal void Name(List<Var> arguments)
    {
        if (arguments[0].Symbol == "" && (arguments[0].Key == null || arguments[0].Collection == null))
        {
            LogRuntimeException(212);
            return;
        }

        var nameVar = new NameVar(arguments[0].Symbol, arguments[0].Key, arguments[0].Collection);
        SystemStack.Push(nameVar);
    }
}