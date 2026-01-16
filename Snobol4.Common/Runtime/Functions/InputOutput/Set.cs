using System.Diagnostics;
using System.Reflection;

namespace Snobol4.Common;

public partial class Executive
{
    //"set first argument is not a suitable name" /* 291 */,
    //"set first argument is null" /* 292 */,
    //"inappropriate second argument to set" /* 293 */,
    //"inappropriate third argument to set" /* 294 */,
    //"set file does not exist" /* 295 */,
    //"set file does not permit setting file pointer" /* 296 */,
    //"set caused non-recoverable I/O error" /* 297 */,

    internal void Set(List<Var> arguments)
    {
        // DIFFERENCE
        // Unicode support provided by StreamReader/StreamWriter classes does not support seeking by bytes.
        // Therefore, to seek to a positions requires getting the current byte position, rewinding to the beginning
        // of the stream, and then reading forward to the desired position. An alternative is to modify the
        // StreamReader/StreamWriter classes to support seeking by bytes, but research indicated that there may
        // not be possible. 
        //
        // In SPITBOL documentation (Emmer and Quillen), indicates that SET works only on files with a fixed
        // record length which is not possible with UNICODE files.
        //
        // whence = 2 is not supported

        // arguments[0]: string channel
        // arguments[1]: integer offset
        // arguments[2]: integer origin (0 = offset applies to beginning of file; 1 = offset is from current position

        // Channel must be a string
        if (!arguments[0].Convert(VarType.STRING, out _, out var channelObj, this))
        {
            LogRuntimeException(291);
            return;
        }

        var channel = (string)channelObj;

        // Channel cannot be a null string
        if (channel == "")
        {
            LogRuntimeException(292);
            return;
        }

        // Offset must be an integer
        if (!arguments[1].Convert(VarType.INTEGER, out _, out var offsetObj, this))
        {
            LogRuntimeException(293);
            return;
        }

        var offset = (long)offsetObj;

        // Whence must be an integer
        if (!arguments[2].Convert(VarType.INTEGER, out _, out var whenceObj, this))
        {
            LogRuntimeException(294);
        }

        var whence = (long)whenceObj;

        // Whence must be 0 or 1
        if (whence is < 0 or > 1)
        {
            LogRuntimeException(294);
            return;
        }

        // Channel must have an associated stream
        if (!StreamReadersByChannel.TryGetValue(channel, out var sr))
        {
            LogRuntimeException(295);
            return;
        }

        Seek(sr, offset, whence);
        PredicateSuccess();
    }

    private static void Seek(StreamReader sr, long offset, long whence)
    {
        // Get current position (in bytes)
        var startPosition = GetActualPosition(sr);
        var position = 0L;

        // Rewind to beginning of stream
        sr.BaseStream.Position = 0;
        sr.DiscardBufferedData();

        // Count number of records to start position
        var totalRecordCount = 0;

        while (position < startPosition && !sr.EndOfStream)
        {
            sr.ReadLine();
            position = GetActualPosition(sr);
            totalRecordCount++;
        }

        // Rewind to beginning of stream
        sr.BaseStream.Position = 0;
        sr.DiscardBufferedData();
        var goalRecord = whence == 1 ? totalRecordCount + offset : offset;

        // Read offset records
        for (var l = 0; l < goalRecord && !sr.EndOfStream; ++l)
            sr.ReadLine();
    }

    private static long GetActualPosition(StreamReader reader)
    {
        const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

        // The current buffer of decoded characters
        Debug.Assert(reader != null, nameof(reader) + " != null");
        var charBuffer = (char[])reader.GetType().InvokeMember("_charBuffer", flags, null, reader, null)!;

        // The index of the next char to be read from charBuffer
        var charPos = (int)reader.GetType().InvokeMember("_charPos", flags, null, reader, null)!;

        // The number of decoded chars presently used in charBuffer
        var charLen = (int)reader.GetType().InvokeMember("_charLen", flags, null, reader, null)!;

        // The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
        var byteBuffer = (byte[])reader.GetType().InvokeMember("_byteBuffer", flags, null, reader, null)!;

        // The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
        var byteLen = (int)reader.GetType().InvokeMember("_byteLen", flags, null, reader, null)!;

        // The number of bytes the remaining chars use in the original encoding.
        var numBytesLeft = reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

        // For variable-byte encodings, deal with partial chars at the end of the buffer
        var numFragments = 0;
        
        if (byteLen <= 0 || reader.CurrentEncoding.IsSingleByte)
            return reader.BaseStream.Position - numBytesLeft;
        
        switch (reader.CurrentEncoding.CodePage)
        {
            // UTF-8
            case 65001:
            {
                byte byteCountMask = 0;
                while (byteBuffer[byteLen - numFragments - 1] >> 6 == 95) // if the byte is "10xx xxxx", it's a continuation-byte
                    byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                if (byteBuffer[byteLen - numFragments - 1] >> 6 == 3) // if the byte is "11xx xxxx", it starts a multibyte char.
                    byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                // see if we found as many bytes as the leading-byte says to expect
                if (numFragments > 1 && byteBuffer[byteLen - numFragments] >> (7 - numFragments) == byteCountMask)
                    numFragments = 0; // no partial-char in the byte-buffer to account for
                break;
            }
            // UTF-16LE
            case 1200:
            {
                if (byteBuffer[byteLen - 1] >= 0xd8) // high-surrogate
                    numFragments = 2; // account for the partial character
                break;
            }
            // UTF-16BE
            case 1201:
            {
                if (byteBuffer[byteLen - 2] >= 0xd8) // high-surrogate
                    numFragments = 2; // account for the partial character
                break;
            }
        }
        return reader.BaseStream.Position - numBytesLeft - numFragments;
    }
}