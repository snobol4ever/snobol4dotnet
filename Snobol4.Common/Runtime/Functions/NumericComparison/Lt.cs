namespace Snobol4.Common;

//"lt first argument is not numeric" /* 147 */,
//"lt second argument is not numeric" /* 148 */,

public partial class Executive
{
    internal void Lt(List<Var> arguments) => BinaryComparison(arguments, IntegerLt, RealLt, 147, 148);

    internal bool IntegerLt(long left, long right) => left < right;

    internal bool RealLt(double left, double right) => left < right;
}