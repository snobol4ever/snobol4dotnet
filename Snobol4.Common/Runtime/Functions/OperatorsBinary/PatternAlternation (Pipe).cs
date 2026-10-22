namespace Snobol4.Common;

//"alternation right operand is not pattern" /* 5 */,
//"alternation left operand is not pattern" /* 6 */,

public partial class Executive
{
    private void CreateAlternatePattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar0)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar0.FunctionName));
        }

        if (arguments[1] is ExpressionVar expressionVar1)
        {
            arguments[1] = new PatternVar(UnevaluatedPattern.Structure(expressionVar1.FunctionName));
        }

        // the right operand is taken off the stack first, so it is the one reported when neither is a pattern
        if (!arguments[1].Convert(VarType.PATTERN, out _, out var patternRight, this))
        {
            LogRuntimeException(5);
            return;
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var patternLeft, this))
        {
            LogRuntimeException(6);
            return;
        }

        SystemStack.Push(new PatternVar(new AlternatePattern((Pattern)patternLeft, (Pattern)patternRight)));
    }
}