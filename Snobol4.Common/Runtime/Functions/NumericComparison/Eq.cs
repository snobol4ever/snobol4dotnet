namespace Snobol4.Common;

//"eq first argument is not numeric" /* 101 */,
//"eq second argument is not numeric" /* 102 */,

public partial class Executive
{
    internal void Eq(List<Var> arguments) => BinaryComparison(arguments, IntegerEq, RealEq, 101, 102);

    internal bool IntegerEq(long left, long right) => left == right;

    internal bool RealEq(double left, double right) => left == right;
}