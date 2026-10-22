namespace Snobol4.Common;

public partial class Executive
{
    internal void Item(List<Var> arguments)
    {
        // the last argument is the function name; ITEM() has no array or table to subscript
        if (arguments.Count < 2 || arguments[0] is not (ArrayVar or TableVar))
        {
            LogRuntimeException(235);
            return;
        }

        SystemStack.Push(arguments[0]);
        arguments.RemoveAt(0);
        arguments.RemoveAt(arguments.Count - 1);

        foreach (var argument in arguments)
            SystemStack.Push(argument);

        IndexCollection();
    }
}