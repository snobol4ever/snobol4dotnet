namespace Snobol4.Common;

public partial class Executive
{
    // Lock object for thread synchronization
    private readonly Lock _multiplicationLock = new();

    internal void Multiply(List<Var> arguments)
    {
        lock (_multiplicationLock)
        {
            BinaryNumericOperation(arguments, IntegerMultiply, RealMultiply, 26, 27, 28, 263);
        }
    }

    internal long IntegerMultiply(long left, long right)
    {
        checked
        {
            return left * right;
        }
    }

    internal double RealMultiply(double left, double right)
    {
        return left * right;
    }
}