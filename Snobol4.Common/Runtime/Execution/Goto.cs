namespace Snobol4.Common;

public partial class Executive
{
    // ReSharper disable once UnusedMember.Global
    public string Goto => SystemStack.Pop().Symbol;
    // Do not delete. Used by DLL

    public int ProcessTrappedError()
    {
        return ExecuteLoop(ErrorJump);
    }
}

