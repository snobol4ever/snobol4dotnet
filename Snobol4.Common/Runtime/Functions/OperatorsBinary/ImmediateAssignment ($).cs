namespace Snobol4.Common;

//"immediate assignment left operand is not pattern" /* 25 */,

public partial class Executive
{
    public void CreateImmediateVariableAssociationPattern(List<Var> arguments)
    {
        // arguments[0]: pattern to associate
        // arguments[1]; variable to immediately assign all match(es)

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
        var va = new ImmediateVariableAssociation1(vb);
        var structure = new PatternVar(new ConcatenatePattern(va, new ConcatenatePattern((Pattern)pattern, vb)));
        SystemStack.Push(structure);
    }
}