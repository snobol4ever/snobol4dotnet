namespace Snobol4.Common;

//"endfile argument is not a suitable name" /* 96 */,
//"endfile argument is null" /* 97 */,
//"endfile file does not exist" /* 98 */,
//"endfile file does not permit endfile" /* 99 */,
//"endfile caused non-recoverable output error" /* 100 */,

public partial class Executive
{
    internal void EndFile(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var channelStr, this))
        {
            LogRuntimeException(96);
            return;
        }

        var channel = (string)channelStr;

        if (channel == "")
        {
            LogRuntimeException(97);
            return;
        }

        if (!StreamInputs.ContainsKey(channel) && !StreamOutputs.ContainsKey(channel))
        {
            LogRuntimeException(98);
            return;
        }

        if (StreamInputs.ContainsKey(channel))
        {
            // only the variables associated with this channel lose their association; OUTPUT, INPUT and
            // other channels keep theirs
            foreach (var entry in IdentifierTable)
                if (entry.Value.InputChannel == channel)
                    entry.Value.InputChannel = "";

            var streamReader = StreamInputs[channel];
            streamReader.Close();
            StreamInputs.Remove(channel);
            DropChannelReaders(channel);
        }

        if (StreamOutputs.ContainsKey(channel))
        {
            // only the variables associated with this channel lose their association; OUTPUT, INPUT and
            // other channels keep theirs
            foreach (var entry in IdentifierTable)
                if (entry.Value.OutputChannel == channel)
                    entry.Value.OutputChannel = "";

            var streamWriter = StreamOutputs[channel];
            streamWriter.Close();
            StreamOutputs.Remove(channel);
        }

        PredicateSuccess();
    }

    // The readers made for a channel go with its stream, so a channel opened again reads its new file
    internal void DropChannelReaders(string channel)
    {
        if (!StreamReadersByChannel.Remove(channel, out var reader))
            return;
        foreach (var symbol in StreamReadersBySymbol.Where(kv => ReferenceEquals(kv.Value, reader)).Select(kv => kv.Key).ToList())
            StreamReadersBySymbol.Remove(symbol);
    }
}
