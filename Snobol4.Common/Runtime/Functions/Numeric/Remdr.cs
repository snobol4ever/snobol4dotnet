namespace Snobol4.Common;

//"remdr second argument is not numeric" /* 165 */,
//"remdr first argument is not numeric" /* 166 */,
//"remdr caused integer overflow" /* 167 */,
//"remdr caused real overflow" /* 312 */,

public partial class Executive
{
    internal void Remainder(List<Var> arguments) => BinaryNumericOperation(arguments, IntegerRemainder, RealRemainder, 166, 165, 167, 312);

    internal long IntegerRemainder(long left, long right) => left % right;

    // SPITBOL's n1 - chop(n1 / n2) * n2, and not the exact fmod: REMDR(12, 3.7) is 0.899999999999999 there
    internal double RealRemainder(double left, double right) => left - Math.Truncate(left / right) * right;
}