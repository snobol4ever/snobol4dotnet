namespace Snobol4.Common;

//"pattern assignment left operand is not pattern" /* 30 */,

public partial class Executive
{
    private void CreateConditionalVariableAssociationPattern(List<Var> arguments)
    {
        // arguments[0]: left (pattern to associate)
        // arguments[1]; right (variable to assign match(es) upon successful match

        while (arguments[0] is ExpressionVar expressionVar)
        {
            expressionVar.Evaluate(this);
            arguments[0] = SystemStack.Pop();
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