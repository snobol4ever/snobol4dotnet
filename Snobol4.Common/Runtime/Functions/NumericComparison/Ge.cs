namespace Snobol4.Common;

//"ge first argument is not numeric" /* 109 */,
//"ge second argument is not numeric" /* 110 */,

public partial class Executive
{
    internal void Ge(List<Var> arguments) => BinaryComparison(arguments, IntegerGe, RealGe, 109, 110);

    internal bool IntegerGe(long left, long right) => left >= right;

    internal bool RealGe(double left, double right) => left >= right;
}