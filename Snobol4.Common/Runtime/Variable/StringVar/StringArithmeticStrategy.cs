using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Arithmetic strategy for string variables
/// Strings support concatenation but not traditional arithmetic
/// </summary>
public class StringArithmeticStrategy : IArithmeticStrategy
{

    public Var Add(Var self, Var other, Executive executive)
    {
        // String "addition" is not supported in SNOBOL4
        // Use concatenation (space operator) instead
        executive.LogRuntimeException(1); // LeftPattern operand of + is not numeric
        return StringVar.Null();
    }


    public Var Subtract(Var self, Var other, Executive executive)
    {
        executive.LogRuntimeException(32); // LeftPattern operand of - is not numeric
        return StringVar.Null();
    }


    public Var Multiply(Var self, Var other, Executive executive)
    {
        executive.LogRuntimeException(26); // LeftPattern operand of * is not numeric
        return StringVar.Null();
    }


    public Var Divide(Var self, Var other, Executive executive)
    {
        executive.LogRuntimeException(12); // LeftPattern operand of / is not numeric
        return StringVar.Null();
    }


    public Var Power(Var self, Var other, Executive executive)
    {
        executive.LogRuntimeException(15); // LeftPattern operand of ^ is not numeric
        return StringVar.Null();
    }


    public Var Negate(Var self, Executive executive)
    {
        executive.LogRuntimeException(10); // Unary minus operand is not numeric
        return StringVar.Null();
    }
}