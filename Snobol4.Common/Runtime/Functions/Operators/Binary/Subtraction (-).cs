namespace Snobol4.Common;

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