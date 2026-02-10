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
        return Math.Pow(left, right);
    }
}