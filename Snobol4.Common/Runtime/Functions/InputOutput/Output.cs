namespace Snobol4.Common;

//"output third argument is not a string" /* 157 */
//"inappropriate second argument for output" /* 158 */
//"inappropriate first argument for output" /* 159 */
//"inappropriate file specification for output" /*
//"output file cannot be written to" /* 161 */
//"output caused file overflow" /* 206 */
//"output caused non-recoverable error" /* 207 */
//"print limit exceeded on standard output channel" /* 253 */
//"output channel currently in use" /* 290 */

public partial class Executive
{
                    internal void OutputFileOpen(List<Var> arguments)
    {
        // arguments[0] Identifier name
        // arguments[1] Channel name (If blank, use STDIN or STDOUT
        // arguments[2] File name   (if blank, associate with channel)
        // arguments[3] File mode   (optional) Defaults to 4 - OpenOrCreate
        // arguments[4] File share  (optional) Defaults to 3 - Read and Write

        //"output third argument is not a string" /* 157 */,
        //"inappropriate second argument for output" /* 158 */,
        //"inappropriate first argument for output" /* 159 */,
        //"inappropriate file specification for output" /* 160 */,
        //"output file cannot be written to" /* 161 */,
        //"output caused file overflow" /* 206 */, NOT USED
        //"output caused non-recoverable error" /* 207 */, NOT USED
        //"output file record has incorrect format" /* 208 */, NOT USED
        //"output channel currently in use" /* 290 */

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
            LogRuntimeException(159);
            return;
        }

        // identifier name cannot be blank
        if (((StringVar)identifierNameVar).Data == "")
        {
            LogRuntimeException(159);
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
            LogRuntimeException(158);
            return;
        }

        // file name has to be a string
        if (!arguments[2].Convert(VarType.STRING, out _, out var fileNameStr, this))
        {
            LogRuntimeException(157);
            return;
        }

        // file mode has to be an integer
        if (!arguments[3].Convert(VarType.INTEGER, out _, out var fileModeObj, this))
        {
            LogRuntimeException(160);
            return;
        }

        // file share has to be an integer
        if (!arguments[4].Convert(VarType.INTEGER, out _, out var fileShareObj, this))
        {
            LogRuntimeException(160);
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
            LogRuntimeException(160);
            return;
        }

        // If both channel and file name are blank, associate the identifier with standard output  
        if (channel == "" && fileName == "")
        {
            IdentifierTable[symbol].OutputChannel = "+console-output";
            return;
        }

        if (fileName == "")
        {
            // Get the path associated with the specified channel
            var filePath = GetPathFromChannel(channel);

            // If the pathname is valid, associate the channel with the identifier
            if (filePath != "")
            {
                if (IdentifierTable[symbol].OutputChannel != "" && IdentifierTable[symbol].OutputChannel != channel)
                {
                    // An identifier can only be associated with one output channel at a time
                    LogRuntimeException(290);
                    return;
                }

                IdentifierTable[symbol].OutputChannel = channel;
                return;
            }

            // If the pathname is not valid, return an error
            // SPITBOL gives different errors depending on whether the channel is in use for input or output
            LogRuntimeException(StreamInputs.ContainsKey(channel) ? 161 : 160);
        }

        // If the filename is not blank, thw channel cannot be in use
        if (StreamInputs.ContainsKey(channel) || StreamOutputs.ContainsKey(channel))
        {
            LogRuntimeException(290);
            return;
        }

        // Create a new stream, associated it with the channel, and associate the channel with the identifier
        try
        {
            var stream = new FileStream(Path.GetFullPath(fileName), (FileMode)fileModeInt, FileAccess.Write, (FileShare)fileShareInt);
            StreamOutputs[channel] = stream;
            IdentifierTable[symbol].OutputChannel = channel;
        }
        catch (Exception e)
        {
            LogRuntimeException(161, e);
            return;
        }

        PredicateSuccess();
    }
}