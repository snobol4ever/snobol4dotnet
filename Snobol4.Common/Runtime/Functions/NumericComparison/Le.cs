namespace Snobol4.Common;

//"le first argument is not numeric" /* 118 */,
//"le second argument is not numeric" /* 119 */,

public partial class Executive
{
    internal void Le(List<Var> arguments) => BinaryComparison(arguments, IntegerLe, RealLe, 121, 122);

    internal bool IntegerLe(long left, long right) => left <= right;

    internal bool RealLe(double left, double right) => left <= right;
}