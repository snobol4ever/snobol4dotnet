namespace Snobol4.Common;

//"ne first argument is not numeric" /* 149 */,
//"ne second argument is not numeric" /* 150 */,

public partial class Executive
{
    internal void Ne(List<Var> arguments) => BinaryComparison(arguments, IntegerNe, RealNe, 149, 150);

    internal bool IntegerNe(long left, long right) => left != right;

    internal bool RealNe(double left, double right) => left != right;  
}