namespace Snobol4.Common;

// "goto abort with no preceding error" /* 36 */,
// "goto continue with no preceding error" /* 37 */,
// "goto scontinue with no preceding error" /* 321 */,

public partial class Executive
{

    // ReSharper disable once UnusedMember.Global
    public string Goto => SystemStack.Pop().Symbol;
    // Do not delete. Used by DLL

    public int ProcessTrappedError()
    {

        const int ABORT = -5;
        const int CONTINUE = -6;
        const int SCONTINUE = -7;

        var result = ExecuteLoop(OnErrorGoto);
        OnErrorGoto = 0;

        switch (result)
        {
            case CONTINUE:
                if (Parent.MessageHistory.Count == 0)
                {
                    LogRuntimeException(37);
                }
                return result;

            case ABORT:
                //if (ErrorJump <= 0)
                //{
                //    LogRuntimeException(36);
                //}
                throw new CompilerException(Parent.ErrorCodeHistory[^1], 0, AmpErrorText);

            case SCONTINUE:
                //if (ErrorJump <= 0)
                //{
                //    LogRuntimeException(321);
                //}
                Failure = false;
                return result;
        }

        return 0;
    }
}
