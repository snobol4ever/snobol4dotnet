namespace Snobol4.Common;

//"backspace argument is not a suitable name" /* 316 */,
//"backspace file does not exist" /* 317 */,
//"backspace file does not permit backspace" /* 318 */,
//"backspace caused non-recoverable error" /* 319 */,

public partial class Executive
{
    internal void BackspaceFile(List<Var> arguments)
    {
        // DIFFERENCE
        // Unicode support provided by StreamReader/StreamWriter classes does not support seeking by bytes.
        // Therefore, to seek to a positions requires getting the current byte position, rewinding to the beginning
        // of the stream, and then reading forward to the desired position. An alternative is to modify the
        // StreamReader/StreamWriter classes to support seeking by bytes, but research indicated that there may
        // not be possible. 
        //
        // The current implementation of BackspaceFile uses the simpler approach of only allowing backspace
        // by one record at a time.

        // Channel must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var channel, this))
        {
            LogRuntimeException(316);
            return;
        }

        // Channel must exist as a reader
        if (StreamReadersByChannel.ContainsKey((string)channel))
        {
            Seek(StreamReadersByChannel[(string)channel], -1, 1);
            PredicateSuccess();
            return;
        }

        LogRuntimeException(StreamOutputs.ContainsKey((string)channel) ? 318 : 317);
    }
}