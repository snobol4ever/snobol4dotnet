namespace Snobol4.Common;

public partial class Executive
{
    internal void Time(List<Var> arguments)
    {
        SystemStack.Push(new StringVar(_timerExecute.Elapsed.ToString()));
    }
}