namespace Snobol4.Common;

//"dupl second argument is not integer" /* 90 */,
//"dupl first argument is not a string or pattern" /* 91 */,

public partial class Executive
{
    internal void Duplicate(List<Var> arguments)
    {
        if (!arguments[1].Convert(VarType.INTEGER, out _, out var numberDupl, this))
        {
            LogRuntimeException(90);
            return;
        }

        if (arguments[0].Convert(VarType.STRING, out _, out var stringDupl, this))
        {
            var stringOut = "";

            for (long i = 0; i < (long)numberDupl; ++i)
                stringOut += (string)stringDupl;
            SystemStack.Push(new StringVar(stringOut));
            return;
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var patternDupl, this))
        {
            LogRuntimeException(91);
            return;
        }

        Pattern patternOut = (Pattern)patternDupl;

        for (long i = 1; i < (long)numberDupl; ++i)
            patternOut = new ConcatenatePattern(patternOut, (Pattern)patternDupl);
        SystemStack.Push(new PatternVar(patternOut));
    }
}