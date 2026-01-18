namespace Snobol4.Common;

//"division left operand is not numeric" /* 12 */,
//"division right operand is not numeric" /* 13 */,
//"division caused integer overflow" /* 14 */,
//"division caused real overflow" /* 262 */,

public partial class Executive
{
    internal void Divide(List<Var> arguments)
    {
        BinaryNumericOperation(arguments, IntegerDivide, RealDivide,
            12, 13, 14, 262);
    }

    internal long IntegerDivide(long left, long right)
    {
        return left / right;
    }

    internal double RealDivide(double left, double right)
    {
        return left / right;
    }
}