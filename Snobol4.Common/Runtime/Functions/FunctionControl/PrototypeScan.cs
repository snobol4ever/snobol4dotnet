namespace Snobol4.Common;

public partial class Executive
{
    // SPITBOL's xscan on a prototype (DEFINE's, DATA's): the text from `position` up to the first of two delimiters or the end of the
    // string, trimmed of blanks. `stopped` is 1 or 2 for the delimiter that ended it and 0 for the end; the position moves past it.
    private static string ScanPrototype(string text, ref int position, char first, char second, out int stopped)
    {
        var delimiter = text.AsSpan(position).IndexOfAny(first, second);
        string token;
        if (delimiter < 0)
        {
            stopped = 0;
            token = text[position..];
            position = text.Length;
        }
        else
        {
            delimiter += position;
            stopped = text[delimiter] == first ? 1 : 2;
            token = text[position..delimiter];
            position = delimiter + 1;
        }

        return token.Trim(' ', '\t');
    }
}
