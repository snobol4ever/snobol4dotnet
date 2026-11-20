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

        // Channel must be a string, and not the null one
        if (!arguments[0].Convert(VarType.STRING, out _, out var channel, this) || (string)channel == "")
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

        // a file opened for input and not read yet stands at its start, where a backspace moves nothing, as in SPITBOL
        if (StreamInputs.ContainsKey((string)channel))
        {
            PredicateSuccess();
            return;
        }

        // an output file goes back to the start of the record written last, which the next record then writes over, as in SPITBOL
        // (x, long, BACKSPACE, z leaves x z ng)
        if (StreamOutputs.TryGetValue((string)channel, out var output))
        {
            if (OutputRecordStarts.TryGetValue((string)channel, out var starts) && starts.Count > 0)
                output.Position = starts.Pop();
            PredicateSuccess();
            return;
        }

        // standard input, opened on the unit by the F 0 option, cannot be moved: 318, as in SPITBOL
        LogRuntimeException(ConsoleInputUnits.Contains((string)channel) ? 318 : 317);
    }

    // Where each record written to an output file began, the last on top (BACKSPACE)
    private readonly Dictionary<string, Stack<long>> OutputRecordStarts = [];

    internal void RecordStart(string channel, Stream stream)
    {
        if (!stream.CanSeek)
            return;
        if (!OutputRecordStarts.TryGetValue(channel, out var starts))
            OutputRecordStarts[channel] = starts = new Stack<long>();
        starts.Push(stream.Position);
    }
}