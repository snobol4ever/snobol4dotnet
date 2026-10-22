namespace Snobol4.Common;

//"remdr second argument is not numeric" /* 165 */,
//"remdr first argument is not numeric" /* 166 */,
//"remdr caused integer overflow" /* 167 */,
//"remdr caused real overflow" /* 312 */,

public partial class Executive
{
    internal void Remainder(List<Var> arguments) => BinaryNumericOperation(arguments, IntegerRemainder, RealRemainder, 166, 165, 167, 312);

    // the remainder has the sign of the first argument; the least integer over -1 is 0 and not an overflow
    internal long IntegerRemainder(long left, long right) => right == -1 ? 0 : left % right;

    // SPITBOL's chop(n1 / n2) * n2 - n1, and not the exact fmod (REMDR(12, 3.7) is 0.899999999999999 there), made to take the sign of the
    // first argument by negating it when it has the other (srm06): when the quotient is too large for the product to be exact the
    // difference comes out with the sign the rounding gave it, and it is the positive that is answered for a positive first argument
    internal double RealRemainder(double left, double right)
    {
        var difference = Math.Truncate(left / right) * right - left;
        return left < 0 ? (difference <= 0 ? difference : -difference) : (difference < 0 ? -difference : difference);
    }
}