using System.Text;

namespace Snobol4.Common;

public partial class Executive
{
                    internal void InputFileOpen(List<Var> arguments)
    {
        // arguments[0] Identifier name
        // arguments[1] Channel name (If blank, use STDIN or STDOUT
        // arguments[2] File name   (if blank, associate with channel)
        // arguments[3] File mode   (optional) Defaults to 4 - OpenOrCreate
        // arguments[4] File share  (optional) Defaults to 3 - Read and Write

        //"input third argument is not a string" /* 113 */,
        //"inappropriate second argument for input" /* 114 */,
        //"inappropriate first argument for input" /* 115 */,
        //"inappropriate file specification for input" /* 116 */,
        //"input file cannot be read" /* 117 */,
        //"input caused file overflow" /* 202 */, NOT USED
        //"input from file caused non-recoverable error" /* 202 */, NOT USED
        //"input file record has incorrect format" /* 203 */, NOT USED
        //"input channel currently in use" /* 289 */,

        // A channel is identified by a string name
        // A channel is a logical connection between a file or device and an identifier
        // A channel can be either an input channel or an output channel, but not both
        // A channel can be associated with one file or device at a time
        // A blank channel name and a blank file name indicates standard input or standard output
        // A non-blank channel name and a blank file name indicates that the identifier is to be associated with the specified channel
        // The association between a channel and a file stream is maintained in either StreamOutputs or StreamInputs

        // An identifier can be associated with one input channel and one output channel at a time
        // An identifier can be associated with a channel for input and a different channel for output
        // Multiple identifiers can be associated with the same channel
        // Identifiers associated with the same channel are aliases for the same input or output stream
        // The relationship between an identifier and its input and output channels is maintained in the identifier's variable properties InputChannel and OutputChannel
        // A channel can be valid, but creation or the channel may fail due to security restrictions
        // Access to the file associated with channel may fail due to security restrictions

        // A file can be associated with multiple channels for input or output 

        // File mode options:
        // 1 - CreateNew:    Create new file. If file exists, throw IOException
        // 2 - Create:       Create file. If file exist, overwrite
        // 3 - Open:         Open.existing file. If file does not exist, throw FileNotFoundException
        // 4 - OpenOrCreate: If file exists, open, else create.
        // 5 - Truncate:     Open existing file and truncate to zero bytes.
        // 6 - Append:       Open existing file, and seeks the end.

        // File share options:
        // 0 - No Sharing:      Any request to open the file (by this process or another process) will fail until the file is closed.
        // 1 - Share Read:      Allows subsequent opening of the file for reading.
        //                      If this flag is not specified, any request to open the file for reading (by this process or another process) will fail until the file is closed.
        //                      However, even if this flag is specified, additional permissions might still be needed to access the file.
        // 2 - Share Writing:   Allows subsequent opening of the file for writing.
        //                      If this flag is not specified, any request to open the file for writing (by this process or another process) will fail until the file is closed.
        //                      However, even if this flag is specified, additional permissions might still be needed to access the file.
        // 3 - Share Read/Write Allows subsequent opening of the file for reading or writing.
        //                      If this flag is not specified, any request to open the file for reading or writing (by this process or another process) will fail until the file is closed.
        //                      However, even if this flag is specified, additional permissions might still be needed to access the file.
        // 4 - Allows subsequent deleting of a file.

        // identifier name has to be a string 
        if (!arguments[0].Convert(VarType.STRING, out var identifierNameVar, out _, this))
        {
            LogRuntimeException(115);
            return;
        }

        // identifier name cannot be blank
        if (((StringVar)identifierNameVar).Data == "")
        {
            LogRuntimeException(115);
            return;
        }

        // identifier name cannot be "terminal" or "TERMINAL"
        var name = ((StringVar)identifierNameVar).Data;
        if (name is "terminal" or "TERMINAL")
        {
            LogRuntimeException(159);
            return;
        }

        // channel name has to be representable as a string
        if (!arguments[1].Convert(VarType.STRING, out _, out var channelNameStr, this))
        {
            LogRuntimeException(114);
            return;
        }

        // file name has to be a string
        if (!arguments[2].Convert(VarType.STRING, out _, out var fileNameStr, this))
        {
            LogRuntimeException(113);
            return;
        }

        // file mode has to be an integer
        if (!arguments[3].Convert(VarType.INTEGER, out _, out var fileModeObj, this))
        {
            LogRuntimeException(116);
            return;
        }

        // file share has to be an integer
        if (!arguments[4].Convert(VarType.INTEGER, out _, out var fileShareObj, this))
        {
            LogRuntimeException(116);
            return;
        }

        var channel = (string)channelNameStr;
        var fileName = (string)fileNameStr;
        var symbol = ((StringVar)identifierNameVar).Data;
        var fileModeInt = (long)fileModeObj;
        var fileShareInt = (long)fileShareObj;

        if (arguments[3] is StringVar { Data: "" })
            fileModeInt = 4;
        if (arguments[4] is StringVar { Data: "" })
            fileShareInt = 3;

        if (fileModeInt < 1 || fileModeInt > 6 || fileShareInt < 0 || fileShareInt > 4)
        {
            LogRuntimeException(116);
            return;
        }

        // If both channel and file name are blank, associate the identifier with standard input  
        if (channel == "" && fileName == "")
        {
            IdentifierTable[symbol].InputChannel = "+input";
            return;
        }

        if (fileName == "")
        {
            // Get the path associated with the specified channel
            var filePath = GetPathFromChannel(channel);

            // If the pathname is valid, associate the channel with the identifier
            if (filePath != "")
            {
                if (IdentifierTable[symbol].InputChannel != "" && IdentifierTable[symbol].InputChannel != channel)
                {
                    // An identifier can only be associated with one output channel at a time
                    LogRuntimeException(290);
                    return;
                }

                IdentifierTable[symbol].InputChannel = channel;
                return;
            }

            // If the pathname is not valid, return an error
            // SPITBOL gives different errors depending on whether the channel is in use for input or output
            LogRuntimeException(StreamInputs.ContainsKey(channel) ? 117 : 116);
        }

        // If the filename is not blank, thw channel cannot be in use
        if (StreamInputs.ContainsKey(channel) || StreamOutputs.ContainsKey(channel))
        {
            LogRuntimeException(289);
            return;
        }

        // Create a new stream, associated it with the channel, and associate the channel with the identifier
        try
        {
            var stream = new FileStream(Path.GetFullPath(fileName), (FileMode)fileModeInt, FileAccess.Read, (FileShare)fileShareInt);
            StreamInputs[channel] = stream;
            IdentifierTable[symbol].InputChannel = channel;
        }
        catch (Exception e)
        {
            LogRuntimeException(117, e);
            return;
        }

        PredicateSuccess();
    }

    private string GetPathFromChannel(string channel)
    {
        var filePath = "";

        if (StreamOutputs.TryGetValue(channel, out var streamWrite))
            if (streamWrite is FileStream fs)
                filePath = fs.Name;

        if (filePath != "" || !StreamInputs.TryGetValue(channel, out var streamRead))
            return filePath;

        if (streamRead is FileStream fs1)
            filePath = fs1.Name;

        return filePath;
    }

    private void InputArguments(List<Var> arguments)
    {
        for (var i = 0; i < arguments.Count; ++i)
        {
            if (arguments[i].InputChannel == "")
                continue;

            arguments[i] = InputArgument(arguments[i]);
        }
    }

    private Var InputArgument(Var arg)
    {
        string? inputLine;

        switch (arg.InputChannel)
        {
            case "":
                return arg;

            case "+console-input":
            case "+terminal-input":
                inputLine = ReadLineDelegate != null ? ReadLineDelegate() : Console.ReadLine();
                break;

            default:
                if (!StreamReadersBySymbol.ContainsKey(arg.Symbol))
                {
                    StreamReadersBySymbol[arg.Symbol] = new StreamReader(StreamInputs[arg.InputChannel], Encoding.UTF8, true);
                    StreamReadersByChannel[arg.InputChannel] = StreamReadersBySymbol[arg.Symbol];
                }

                var reader = StreamReadersBySymbol[arg.Symbol];
                inputLine = reader.ReadLine();

                break;
        }

        if (inputLine != null)
            return new StringVar(inputLine);

        NonExceptionFailure();
        inputLine = "";
        return new StringVar(inputLine);
    }
}