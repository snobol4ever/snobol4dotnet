namespace Snobol4.Common;

//"concatenation left operand is not a string or pattern" /* 8 */,
//"concatenation right operand is not a string or pattern" /* 9 */,

public partial class Executive
{
    public void CreateConcatenatePattern(List<Var> arguments)
    {
        // If left argument is null, return the right argument unchanged.
        // Clone to prevent aliasing: the caller (e.g. a shift assignment A<K+1>=A<K>)
        // stores the result directly into an array slot, so returning the original
        // Var object would make two array slots point to the same object.
        // A pattern comes back as a new Var over the same pattern, so IDENT(P '', P) succeeds, as in SPITBOL
        // (BAL NULL is BAL).
        if (arguments[0] is StringVar { Data: "" })
        {
            SystemStack.Push(SameValue(arguments[1]));
            return;
        }

        // If right argument is null, return the left argument unchanged (cloned).
        if (arguments[1] is StringVar { Data: "" })
        {
            SystemStack.Push(SameValue(arguments[0]));
            return;
        }

        // If both arguments are strings, concatenate them
        if (arguments[0].Convert(VarType.STRING, out _, out var stringLeftValue, this) &&
            arguments[1].Convert(VarType.STRING, out _, out var stringRightValue, this))
        {
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

    private static Var SameValue(Var operand) => operand is PatternVar pattern ? new PatternVar(pattern) : operand.Clone();
}