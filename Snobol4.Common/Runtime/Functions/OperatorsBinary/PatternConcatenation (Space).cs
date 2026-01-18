using System.Diagnostics;

namespace Snobol4.Common;

//"concatenation left operand is not a string or pattern" /* 8 */,
//"concatenation right operand is not a string or pattern" /* 9 */,

public partial class Executive
{
    public void CreateConcatenatePattern(List<Var> arguments)
    {

        // IF both arguments are strings, concatenate them
        if (arguments[0].Convert(VarType.STRING, out _, out var stringLeftValue, this) &&
            arguments[1].Convert(VarType.STRING, out _, out var stringRightValue, this))
        {
            Debug.Assert(stringRightValue != null, nameof(stringRightValue) + " != null");
            SystemStack.Push(new StringVar((string)stringLeftValue + (string)stringRightValue));
            return;
        }

        if (arguments[0] is ExpressionVar expressionVar0)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar0.FunctionName));
        }

        if (arguments[1] is ExpressionVar expressionVar1)
        {
            arguments[1] = new PatternVar(UnevaluatedPattern.Structure(expressionVar1.FunctionName));
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var leftPattern, this))
        {
            LogRuntimeException(8);
            return;
        }

        if (!arguments[1].Convert(VarType.PATTERN, out _, out var rightPattern, this))
        {
            LogRuntimeException(9);
            return;
        }

        SystemStack.Push(new PatternVar(new ConcatenatePattern((Pattern)leftPattern, (Pattern)rightPattern)));
    }
}