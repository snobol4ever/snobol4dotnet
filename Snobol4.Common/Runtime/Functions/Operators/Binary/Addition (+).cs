namespace Snobol4.Common;

public partial class Executive
{
    // Lock object for thread synchronization
    private readonly Lock _additionLock = new();

    internal void Add(List<Var> arguments)
    {
        lock (_additionLock)
        {
            BinaryNumericOperation(arguments, IntegerAdd, RealAdd, 1, 2, 3, 261);
        }
    }

    internal long IntegerAdd(long left, long right)
    {
        checked
        {
            return left + right;
        }
    }

    internal double RealAdd(double left, double right)
    {
        return left + right;
    }
}