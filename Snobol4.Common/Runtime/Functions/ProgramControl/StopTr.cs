namespace Snobol4.Common;

//"stoptr first argument is not appropriate name" /* 190 */,
//"stoptr second argument is not trace type" /* 191 */,

public partial class Executive
{
    internal void StopTrace(List<Var> arguments)
    {
        // STOPTR(name, type) removes the trace of that type from that name (a null type is a value trace), as TRACE sets it
        switch (ChangeTrace(arguments[0], arguments[1], null, set: false))
        {
            case TraceOutcome.BadName:
                LogRuntimeException(190);
                return;
            case TraceOutcome.BadType:
                LogRuntimeException(191);
                return;
        }

        // STOPTR returns the null string, as in SPITBOL
        PredicateSuccess();
    }
}
