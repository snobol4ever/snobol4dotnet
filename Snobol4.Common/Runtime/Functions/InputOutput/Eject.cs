namespace Snobol4.Common;

//"eject argument is not a suitable name" /* 92 */,
//"eject file does not exist" /* 93 */,
//"eject file does not permit page eject" /* 94 */,
//"eject caused non-recoverable output error" /* 95 */,

public partial class Executive
{
    internal void EjectFile(List<Var> arguments)
    {
        // DIFFERENCE
        // EJECT is not supported as files do not support a page eject. Essentially a no-op.

        SystemStack.Push(StringVar.Null());
    }

}
