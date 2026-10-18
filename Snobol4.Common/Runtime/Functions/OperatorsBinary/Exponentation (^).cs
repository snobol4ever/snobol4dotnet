namespace Snobol4.Common;

//"exponentiation right operand is not numeric" /* 15 */,
//"exponentiation left operand is not numeric" /* 16 */,
//"exponentiation caused integer overflow" /* 17 */,
//"exponentiation result is undefined" /* 18 */,
//"exponentiation caused real overflow" /* 266 */,

public partial class Executive
{
    internal void Power(List<Var> arguments)
    {
        // An integer to a negative integer power is a real (9 ^ -1 is 0.111111111111111), and a zero
        // base gives 0., as sbl answers both
        if (Var.ToNumeric(arguments[0], out var integerBase, out var baseValue, out _, this) && integerBase
            && Var.ToNumeric(arguments[1], out var integerExponent, out var exponent, out _, this) && integerExponent
            && exponent < 0)
        {
            SystemStack.Push(new RealVar(RealPower(baseValue, exponent)));
            return;
        }

        BinaryNumericOperation(arguments, IntegerPower, RealPower,
            15, 16, 17, 266);
    }

                                internal long IntegerPower(long left, long right)
    {
        long result = 1;
        if (left == 0 && right <= 0)
        {
            LogRuntimeException(18);
            return 0;
        }

        checked
        {
            for (var i = 0; i < right; ++i)
                result *= left;
        }

        return result;
    }

    internal double RealPower(double left, double right)
    {
        // A zero base to a negative power is 0., not infinity, as in SPITBOL
        if (left == 0 && right < 0)
            return 0.0;
        return Math.Pow(left, right);
    }
}