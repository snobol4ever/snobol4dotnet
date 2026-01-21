using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Arithmetic strategy for statement separator
/// Statement separators don't support any arithmetic operations
/// </summary>
public sealed class StatementSeparatorArithmeticStrategy : IArithmeticStrategy
{

    public Var Add(Var self, Var other, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }


    public Var Subtract(Var self, Var other, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }


    public Var Multiply(Var self, Var other, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }


    public Var Divide(Var self, Var other, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }


    public Var Power(Var self, Var other, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }


    public Var Negate(Var self, Executive executive)
    {
        throw new InvalidOperationException("Statement separators cannot participate in arithmetic operations");
    }
}