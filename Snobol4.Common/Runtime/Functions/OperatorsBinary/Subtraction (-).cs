namespace Snobol4.Common;

//"subtraction left operand is not numeric" /* 32 */,
//"subtraction right operand is not numeric" /* 33 */,
//"subtraction caused integer overflow" /* 34 */,
//"subtraction caused real overflow" /* 264 */,

public partial class Executive
{
    // Lock object for thread synchronization
    private readonly Lock _subtractionLock = new();

    internal void Subtract(List<Var> arguments)
    {
        lock (_subtractionLock)
        {
            BinaryNumericOperation(arguments, IntegerSubtract, RealSubtract, 32, 33, 34, 264);
        }
    }

    internal long IntegerSubtract(long left, long right)
    {
        checked
        {
            return left - right;
        }
    }

    internal double RealSubtract(double left, double right)
    {
        return left - right;
    }
}