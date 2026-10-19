namespace Snobol4.Common;

//"collect argument is not integer" /* 73 */,

public partial class Executive
{
    // COLLECT(N) collects and answers the free storage in words, failing when fewer than N words are free, as
    // in SPITBOL; the free storage is what the -d heap leaves after what the program holds
    internal void GarbageCollect(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var wanted, this))
        {
            LogRuntimeException(73);
            return;
        }

        var free = Math.Max(0, Parent.BuildOptions.HeapMaxBytes - GC.GetTotalMemory(true)) / IntPtr.Size;
        if ((long)wanted > free)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new IntegerVar(free));
    }
}