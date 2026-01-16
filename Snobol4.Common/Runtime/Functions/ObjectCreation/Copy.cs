namespace Snobol4.Common;

public partial class Executive
{
    internal void Copy(List<Var> arguments) => SystemStack.Push(arguments[0].Clone());
}