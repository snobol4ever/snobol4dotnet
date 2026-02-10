namespace Snobol4.Common;

public sealed class TableArithmeticStrategy : IArithmeticStrategy
{

    public Var Add(Var self, Var other, Executive executive)
    {
        // Tables don't support addition
        executive.LogRuntimeException(1); // LeftPattern operand of + is not numeric
        return StringVar.Null();
    }


    public Var Subtract(Var self, Var other, Executive executive)
    {
        // Tables don't support subtraction
        executive.LogRuntimeException(32); // LeftPattern operand of - is not numeric
        return StringVar.Null();
    }


    public Var Multiply(Var self, Var other, Executive executive)
    {
        // Tables don't support multiplication
        executive.LogRuntimeException(26); // LeftPattern operand of * is not numeric
        return StringVar.Null();
    }


    public Var Divide(Var self, Var other, Executive executive)
    {
        // Tables don't support division
        executive.LogRuntimeException(12); // LeftPattern operand of / is not numeric
        return StringVar.Null();
    }


    public Var Power(Var self, Var other, Executive executive)
    {
        // Tables don't support exponentiation
        executive.LogRuntimeException(15); // LeftPattern operand of ^ is not numeric
        return StringVar.Null();
    }


    public Var Negate(Var self, Executive executive)
    {
        // Tables don't support negation
        executive.LogRuntimeException(10); // Unary minus operand is not numeric
        return StringVar.Null();
    }
}