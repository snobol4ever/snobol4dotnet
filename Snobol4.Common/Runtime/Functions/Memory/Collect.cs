namespace Snobol4.Common;

//"collect argument is not integer" /* 73 */,

public partial class Executive
{
    // The bytes the program holds, read from the last full blocking collection's own account of what it kept (each generation's size
    // after it, less the free space in it) and not from GC.GetTotalMemory, which counts the part of each thread's allocation context
    // not yet used: two COLLECT()s with nothing between them disagreed by 4 KB or 8 KB, now and then, so the storage TIMEGC reports as
    // freed came out negative about a run in three
    internal static long HeldBytes()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        var held = 0L;
        foreach (var generation in GC.GetGCMemoryInfo(GCKind.FullBlocking).GenerationInfo)
        {
            held += generation.SizeAfterBytes - generation.FragmentationAfterBytes;
        }

        return held;
    }

    // COLLECT(N) collects and answers the free storage in words, failing when fewer than N words are free, as
    // in SPITBOL; the free storage is what the -d heap leaves after what the program holds
    internal void GarbageCollect(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.INTEGER, out _, out var wanted, this))
        {
            LogRuntimeException(73);
            return;
        }

        var free = Math.Max(0, Parent.BuildOptions.HeapMaxBytes - HeldBytes()) / IntPtr.Size;
        if ((long)wanted > free)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(new IntegerVar(free));
    }
}