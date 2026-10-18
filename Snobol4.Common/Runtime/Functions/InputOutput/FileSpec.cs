namespace Snobol4.Common;

/// <summary>
/// The third argument of INPUT and OUTPUT, read as SPITBOL reads it (osint lenfnm and sioarg): a
/// filename up to the first blank, then I/O options after that blank or in a trailing [...]
/// ("out.txt -a", "out.txt[-a]"). The options are letters with an optional leading '-', separated by
/// blanks, tabs or commas: A (append), B n, C, F n, I, L n, Q n, R n, S followed by DN, DR, DRW or DW,
/// U, W, X, Y. Anything else makes the specification inappropriate (error 116 for INPUT, 160 for
/// OUTPUT). A appends; F 1 or F 2 with no file name writes to standard output or standard error, and C,
/// Q or R (raw mode) writes no line ends; the others are accepted without effect.
/// </summary>
internal static class FileSpec
{
    internal static bool TryParse(string spec, out string fileName, out bool append) =>
        TryParse(spec, out fileName, out append, out _, out _);

    // descriptor: the F option's file descriptor (1 standard output, 2 standard error), or -1;
    // raw: the C, Q or R option, character (raw) mode, which writes no line ends
    internal static bool TryParse(string spec, out string fileName, out bool append, out long descriptor, out bool raw)
    {
        fileName = spec;
        append = false;
        descriptor = -1;
        raw = false;
        if (spec.Length == 0)
            return true;

        // a trailing [options] is not part of the name
        var length = spec.Length;
        if (spec[^1] == ']')
        {
            var open = spec.LastIndexOf('[', spec.Length - 2);
            var close = spec.LastIndexOf(']', spec.Length - 2);
            if (open >= 0 && open > close)
                length = open;
        }

        var nameLength = spec.IndexOf(' ', 0, length);
        if (nameLength < 0)
            nameLength = length;
        fileName = spec[..nameLength];

        var i = nameLength;
        var lastDash = false;
        while (i < spec.Length)
        {
            var c = char.ToUpperInvariant(spec[i++]);
            switch (c)
            {
                case ' ' or '\t' or ',' or '[' or ']':
                    lastDash = false;
                    continue;

                case '-':
                    if (lastDash)
                        return false;
                    lastDash = true;
                    continue;

                case 'A':
                    append = true;
                    break;

                case 'B' or 'L':
                    if (ScanInteger(spec, ref i) <= 0)
                        return false;
                    break;

                case 'Q' or 'R':
                    if (ScanInteger(spec, ref i) <= 0)
                        return false;
                    raw = true;
                    break;

                case 'F':
                    descriptor = ScanInteger(spec, ref i);
                    break;

                case 'S':
                    if (i >= spec.Length || char.ToUpperInvariant(spec[i++]) != 'D' || i >= spec.Length)
                        return false;
                    switch (char.ToUpperInvariant(spec[i++]))
                    {
                        case 'N' or 'W':
                            break;
                        case 'R':
                            if (i < spec.Length && char.ToUpperInvariant(spec[i]) == 'W')
                                i++;
                            break;
                        default:
                            return false;
                    }

                    if (i >= spec.Length)
                        return false;
                    break;

                case 'C':
                    raw = true;
                    break;

                case 'I' or 'U' or 'W' or 'X' or 'Y':
                    break;

                default:
                    return false;
            }

            lastDash = false;
        }

        return true;
    }

    // osint scnint: the decimal digits at i, 0 when there are none
    private static long ScanInteger(string spec, ref int i)
    {
        long value = 0;
        while (i < spec.Length && char.IsAsciiDigit(spec[i]) && value < int.MaxValue)
            value = value * 10 + (spec[i++] - '0');
        return value;
    }
}
