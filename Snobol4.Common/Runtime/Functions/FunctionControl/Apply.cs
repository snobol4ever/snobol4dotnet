namespace Snobol4.Common;

public partial class Executive
{
    public void Apply(List<Var> arguments)
    {
        if (arguments.Count < 2 || !NaturalVariableName(arguments[0], out var name))
        {
            LogRuntimeException(60);
            return;
        }

        arguments[0] = new StringVar(name);
        arguments.RemoveAt(arguments.Count - 1);

        foreach (var argument in arguments)
            SystemStack.Push(argument);

        Function(arguments.Count - 1);
    }
}