namespace Snobol4.Common;

//"immediate assignment left operand is not pattern" /* 25 */,

public partial class Executive
{
    public void CreateImmediateVariableAssociationPattern(List<Var> arguments)
    {
        // arguments[0]: pattern to associate
        // arguments[1]; variable to immediately assign all match(es)

        // a call in the place of the name that handed back a value by RETURN is error 21 as the pattern is built, as in SPITBOL
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

        if (arguments[0] is ExpressionVar expressionVar)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar.FunctionName));
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var pattern, this))
        {
            LogRuntimeException(25);
            return;
        }

        var vb = new ImmediateVariableAssociation2(arguments[1]);
        var va = new ImmediateVariableAssociation1();
        var structure = new PatternVar(new ConcatenatePattern(va, new ConcatenatePattern((Pattern)pattern, vb)));
        SystemStack.Push(structure);
    }
}