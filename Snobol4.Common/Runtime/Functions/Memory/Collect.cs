namespace Snobol4.Common;

//"collect argument is not integer" /* 73 */,

public partial class Executive
{
    internal void GarbageCollect(List<Var> arguments)
    {
        GC.Collect();
        SystemStack.Push(StringVar.Null());
    }
}