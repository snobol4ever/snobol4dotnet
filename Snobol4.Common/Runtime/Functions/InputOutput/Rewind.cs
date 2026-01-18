namespace Snobol4.Common;

//"rewind argument is not a suitable name" /* 172 */,
//"rewind argument is null" /* 173 */,
//"rewind file does not exist" /* 174 */,
//"rewind file does not permit rewind" /* 175 */,
//"rewind caused non-recoverable error" /* 176 */,

public partial class Executive
{
    internal void Rewind(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var channelStr, this))
            LogRuntimeException(172);

        var channel = (string)channelStr;

        if (channel == "")
        {
            LogRuntimeException(173);
            return;
        }

        if (!StreamReadersByChannel.TryGetValue(channel, out var stream))
        {
            if (!StreamOutputs.TryGetValue(channel, out var streamOut))
            {
                LogRuntimeException(174);
                return;
            }

            streamOut.Position = 0;
            PredicateSuccess();
        }

        if (stream != null)
        {
            stream.BaseStream.Position = 0;
            stream.DiscardBufferedData();
            PredicateSuccess();
            return;
        }

        NonExceptionFailure();
    }
}