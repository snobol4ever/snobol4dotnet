namespace Snobol4.Common;

//"pattern assignment left operand is not pattern" /* 30 */,

public partial class Executive
{
    private void CreateConditionalVariableAssociationPattern(List<Var> arguments)
    {
        // arguments[0]: left (pattern to associate)
        // arguments[1]; right (variable to assign match(es) upon successful match

        // a call in the place of the name that handed back a value by RETURN is error 21 as the pattern is built, as in SPITBOL
        // (P = LEN(1) . H(2)); it was found only when the match assigned
        if (ValueInNamesPlace(arguments[1]) is var valueError and not 0)
        {
            LogRuntimeException(valueError);
            return;
        }

        // so is a builtin's value (LEN(1) . BREAK('a'), . SIZE('a')): no name, unless it is a deferred expression, found at match time
        if (arguments[1] is not ExpressionVar && !IsName(arguments[1]))
        {
            LogRuntimeException(21);
            return;
        }

        // *X . V defers X to match time, as $ does: evaluating it here read X before the match
        // could set it (Budne's uneval2) and, for *LEN(3) . V, failed with error 212
        if (arguments[0] is ExpressionVar expressionVar)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar.FunctionName));
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var pattern, this))
        {
            LogRuntimeException(30);
            return;
        }

        var va = new AlternatePattern(new ConditionalVariableAssociation1(arguments[1], this), new ConditionalVariableAssociationBackup1(arguments[1], this));
        var vb = new AlternatePattern(new ConditionalVariableAssociation2(arguments[1], this), new ConditionalVariableAssociationBackup2(arguments[1], this));
        var vc = new PatternVar(new ConcatenatePattern(va, new ConcatenatePattern((Pattern)pattern, vb)));

        SystemStack.Push(vc);
    }
}