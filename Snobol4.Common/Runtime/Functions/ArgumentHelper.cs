namespace Snobol4.Common;

public partial class Executive
{
    // SPITBOL's gtsmi: the argument as an integer. False when it is not one; `inRange` is false when it is negative or
    // larger than the biggest object (the -m value), which each function treats as its own case
    private bool SmallInteger(Var argument, out long value, out bool inRange)
    {
        value = 0;
        inRange = true;
        if (!argument.Convert(VarType.INTEGER, out _, out var number, this))
            return false;
        value = (long)number;
        inRange = value >= 0 && value <= MaxObjectLength;
        return true;
    }

    // SPITBOL's gtnvr: the name of a natural variable (a function, a datatype) as given in the argument, folded as the program is.
    // A string of any kind will do, even 3, but not the null string, a pattern or the name of an element of an array or table.
    private bool NaturalVariableName(Var argument, out string name)
    {
        name = "";

        // the NAME of an element has no pointer, and has no string; a NAME kept in an array or a DATA field is stamped with its place
        // (Key, Collection) but is still the name of its variable
        if (!argument.Convert(VarType.STRING, out _, out var text, this) || (string)text == "")
            return false;
        name = Parent.FoldCase((string)text);
        return true;
    }

    // SPITBOL's alocs: a string longer than &MAXLNGTH is error 205
    private bool StringTooLong(long length)
    {
        if (length <= AmpMaxLength)
            return false;
        LogRuntimeException(205);
        return true;
    }
}
