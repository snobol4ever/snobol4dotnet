namespace Snobol4.Common;

public partial class Executive
{
                        internal void Interrogation(List<Var> arguments)
    {
        var v = SystemStack.Pop();
        var stringVar = StringVar.Null();
        stringVar.Succeeded = v.Succeeded;
        SystemStack.Push(stringVar);
    }
}