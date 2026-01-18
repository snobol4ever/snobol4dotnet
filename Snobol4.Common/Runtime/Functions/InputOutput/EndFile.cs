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
            foreach (var entry in IdentifierTable)
                entry.Value.InputChannel = "";

            var streamReader = StreamInputs[channel];
            streamReader.Close();
            StreamInputs.Remove(channel);
        }

        if (StreamOutputs.ContainsKey(channel))
        {
            foreach (var entry in IdentifierTable)
                entry.Value.OutputChannel = "";

            var streamWriter = StreamOutputs[channel];
            streamWriter.Close();
            StreamOutputs.Remove(channel);
        }

        PredicateSuccess();
    }
}