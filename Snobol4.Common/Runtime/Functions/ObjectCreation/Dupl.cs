namespace Snobol4.Common;

//"dupl second argument is not integer" /* 90 */,
//"dupl first argument is not a string or pattern" /* 91 */,

public partial class Executive
{
    internal void Duplicate(List<Var> arguments)
    {
        if (!SmallInteger(arguments[1], out var count, out var inRange))
        {
            LogRuntimeException(90);
            return;
        }

        if (!inRange)
        {
            NonExceptionFailure();
            return;
        }

        if (arguments[0].Convert(VarType.STRING, out _, out var stringDupl, this))
        {
            var text = (string)stringDupl;
            if (text.Length == 0 || count == 0)
            {
                SystemStack.Push(new StringVar(""));
                return;
            }

            if (StringTooLong(text.Length * count))
                return;

            var stringOut = new System.Text.StringBuilder((int)Math.Min(text.Length * count, int.MaxValue));
            for (long i = 0; i < count; ++i)
                stringOut.Append(text);
            SystemStack.Push(new StringVar(stringOut.ToString()));
            return;
        }

        // an unevaluated expression is a pattern to duplicate
        if (arguments[0] is ExpressionVar expressionVar)
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar.FunctionName));

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var patternDupl, this))
        {
            LogRuntimeException(91);
            return;
        }

        if (count == 0)
        {
            SystemStack.Push(new PatternVar(new NullPattern()));
            return;
        }

        var patternOut = (Pattern)patternDupl;

        for (long i = 1; i < count; ++i)
            patternOut = new ConcatenatePattern(patternOut, (Pattern)patternDupl);
        SystemStack.Push(new PatternVar(patternOut));
    }
}
