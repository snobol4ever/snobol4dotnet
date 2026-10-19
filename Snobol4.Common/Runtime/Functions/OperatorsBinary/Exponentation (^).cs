namespace Snobol4.Common;

//"exponentiation right operand is not numeric" /* 15 */,
//"exponentiation left operand is not numeric" /* 16 */,
//"exponentiation caused integer overflow" /* 17 */,
//"exponentiation result is undefined" /* 18 */,
//"exponentiation caused real overflow" /* 266 */,

public partial class Executive
{
    /// <summary>
    /// X ** Y as SPITBOL's o_exp computes it: the exponent is read first (error 15), then the base (16).
    /// An integer to a non-negative integer power multiplies out (overflow is error 17), and so does a
    /// real to one; a real exponent, or a negative integer one, goes by exp(Y * ln(X)), every step under
    /// SPITBOL's flush-to-zero arithmetic; 0 ** 0 is error 18 and a negative base to a non-integral power
    /// error 311.
    /// </summary>
    internal void Power(List<Var> arguments)
    {
        if (!Var.ToNumeric(arguments[1], out var integerExponent, out var lExponent, out var dExponent, this))
        {
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(15);
            return;
        }

        if (!Var.ToNumeric(arguments[0], out var integerBase, out var lBase, out var dBase, this))
        {
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(16);
            return;
        }

        if (integerExponent && lExponent >= 0)
        {
            if (integerBase)
            {
                if (lBase == 0 && lExponent == 0)
                {
                    LogRuntimeException(18);
                    return;
                }

                try
                {
                    SystemStack.Push(new IntegerVar(IntegerPower(lBase, lExponent)));
                }
                catch (OverflowException)
                {
                    LogRuntimeException(17);
                }
                return;
            }

            if (dBase == 0 && lExponent == 0)
            {
                LogRuntimeException(18);
                return;
            }

            var product = 1.0;
            for (long k = 0; k < lExponent; ++k)
            {
                product = RealVar.Sse(product * RealVar.Sse(dBase));
                if (double.IsInfinity(product))
                {
                    LogRuntimeException(266);
                    return;
                }
            }

            SystemStack.Push(new RealVar(product));
            return;
        }

        var x = integerBase ? lBase : dBase;
        var y = integerExponent ? lExponent : dExponent;
        if (RealPower(x, y) is { } result)
            SystemStack.Push(new RealVar(result));
    }

    // o_exp's real circuit: exp(y * ln(x)) with the sign of a negative base fixed up for an integral power;
    // null after logging the error
    private double? RealPower(double x, double y)
    {
        x = RealVar.Sse(x);
        y = RealVar.Sse(y);
        if (x == 0)
        {
            if (y == 0)
            {
                LogRuntimeException(18);
                return null;
            }

            return 0.0;
        }

        var negative = false;
        if (x < 0)
        {
            x = -x;
            var whole = Math.Truncate(y);
            if (Math.Abs(whole) > long.MaxValue)
            {
                LogRuntimeException(266);
                return null;
            }

            if (whole - y != 0)
            {
                LogRuntimeException(311);
                return null;
            }

            negative = ((long)whole & 1) != 0;
        }

        var r = RealVar.Sse(RealVar.Sse(Math.Log(x)) * y);
        if (double.IsInfinity(r))
        {
            LogRuntimeException(266);
            return null;
        }

        r = RealVar.Sse(Math.Exp(r));
        if (double.IsInfinity(r))
        {
            LogRuntimeException(266);
            return null;
        }

        return negative ? -r : r;
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

}