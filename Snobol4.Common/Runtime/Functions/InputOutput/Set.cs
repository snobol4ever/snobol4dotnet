using System.Text;
using System.Reflection;

namespace Snobol4.Common;

//"set first argument is not a suitable name" /* 291 */,
//"set first argument is null" /* 292 */,
//"inappropriate second argument to set" /* 293 */,
//"inappropriate third argument to set" /* 294 */,
//"set file does not exist" /* 295 */,
//"set file does not permit setting file pointer" /* 296 */,
//"set caused non-recoverable I/O error" /* 297 */,

public partial class Executive
{
    /// <summary>
    /// SET(channel, offset, origin), as SPITBOL's: moves an input channel's file pointer to offset bytes
    /// from the start (origin 0), the current position (1) or the end (2), kept within the file, and
    /// answers the new byte position; reading resumes there.
    /// </summary>
    internal void Set(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.STRING, out _, out var channelObj, this))
        {
            LogRuntimeException(291);
            return;
        }

        var channel = (string)channelObj;
        if (channel == "")
        {
            LogRuntimeException(292);
            return;
        }

        if (!arguments[1].Convert(VarType.INTEGER, out _, out var offsetObj, this))
        {
            LogRuntimeException(293);
            return;
        }

        var offset = (long)offsetObj;
        if (!arguments[2].Convert(VarType.INTEGER, out _, out var whenceObj, this) || (long)whenceObj is < 0 or > 2)
        {
            LogRuntimeException(294);
            return;
        }

        var whence = (long)whenceObj;
        var sr = ChannelReader(channel);
        if (sr is null)
        {
            LogRuntimeException(295);
            return;
        }

        var target = whence switch
        {
            0 => offset,
            1 => GetActualPosition(sr) + offset,
            _ => sr.BaseStream.Length + offset
        };
        // the position stays within the file, as sbl keeps it (SET(1,100,0) on a 13-byte file is 13)
        target = Math.Clamp(target, 0, sr.BaseStream.Length);
        sr.BaseStream.Position = target;
        sr.DiscardBufferedData();
        SystemStack.Push(new IntegerVar(target));
    }

    // The reader an input channel is read through, made on first use and shared by every variable
    // associated with the channel
    internal StreamReader? ChannelReader(string channel)
    {
        if (StreamReadersByChannel.TryGetValue(channel, out var reader))
            return reader;
        if (!StreamInputs.TryGetValue(channel, out var stream))
            return null;
        reader = new StreamReader(stream, Encoding.UTF8, true);
        StreamReadersByChannel[channel] = reader;
        return reader;
    }

    // Record-based repositioning, which BACKSPACE uses
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