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

    // SPITBOL's alocs: a string longer than &MAXLNGTH is error 205
    private bool StringTooLong(long length)
    {
        if (length <= AmpMaxLength)
            return false;
        LogRuntimeException(205);
        return true;
    }
}
