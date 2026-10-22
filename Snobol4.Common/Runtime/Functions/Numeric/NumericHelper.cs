namespace Snobol4.Common;

public partial class Executive
{
    #region Delegate declarations

    internal delegate long IntegerBinaryOperator(long left, long right);
    internal delegate double RealBinaryOperator(double left, double right);
    internal delegate double UnaryOperator(double operand);
    internal delegate bool IntegerCompare(long left, long right);
    internal delegate bool RealCompare(double left, double right);

    #endregion

    #region Binary Numeric Operation Helper Function
    private void BinaryNumericOperation(
        List<Var> arguments,
        IntegerBinaryOperator integerBinaryOperator,
        RealBinaryOperator realBinaryOperator,
        int errorLeft,
        int errorRight,
        int errorIntegerOverflow,
        int errorRealOverflow)
    {
        // The right operand is taken off the stack first, so it is the one SPITBOL finds wrong when both are
        if (!Var.ToNumeric(arguments[1], out var isIntegerRight, out var lRight, out var dRight, this))
        {
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(errorRight);
            return;
        }

        // Propagate deferred-expression failure as a SNOBOL match failure
        // rather than a fatal "left operand is not numeric" error.
        if (!Var.ToNumeric(arguments[0], out var isIntegerLeft, out var lLeft, out var dLeft, this))
        {
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(errorLeft);
            return;
        }

        if (isIntegerLeft && isIntegerRight)
        {
            try
            {
                var lResult = integerBinaryOperator(lLeft, lRight);
                SystemStack.Push(new IntegerVar(lResult));
                return;
            }
            catch
            {
                LogRuntimeException(errorIntegerOverflow);
                return;
            }
        }

        if (isIntegerLeft)
            dLeft = lLeft;

        if (isIntegerRight)
            dRight = lRight;

        // SPITBOL's SSE flags: a subnormal operand counts as zero and a subnormal result becomes zero
        var dResult = RealVar.Sse(realBinaryOperator(RealVar.Sse(dLeft), RealVar.Sse(dRight)));

        if (realBinaryOperator.Method.Name == "RealPower")
        {

            if (dLeft == 0 && dRight == 0)
            {
                LogRuntimeException(18);
                return;
            }

            if (double.IsNaN(dResult))
            {
                LogRuntimeException(311);
                return;
            }

            if (double.IsInfinity(dResult))
            {
                LogRuntimeException(System.Convert.ToDouble(lLeft) == 0 ? 18 : errorRealOverflow);
                return;
            }
        }
        else
        {
            // 0.0 / 0.0 and 0.0 REMDR 0.0 are 0. in SPITBOL (the invalid operation is not an overflow), not an error
            if (double.IsNaN(dResult) && dLeft == 0 && dRight == 0)
                dResult = 0.0;
            else if (double.IsNaN(dResult) || double.IsInfinity(dResult))
            {
                LogRuntimeException(errorRealOverflow);
                return;
            }
        }

        SystemStack.Push(new RealVar(dResult));
    }

    #endregion

    #region Unary Numeric Operation Helper Function

    internal void UnaryNumericOperation(List<Var> arguments, UnaryOperator unaryOperator, int errorNumeric, int errorOverflow, int errorNaN)
    {
        var v = arguments[0];
        if (!v.Convert(VarType.REAL, out _, out var operand, this))
        {
            LogRuntimeException(errorNumeric);
            return;
        }

        var dResult = unaryOperator(System.Convert.ToDouble(operand));

        if (double.IsInfinity(dResult))
        {
            LogRuntimeException(errorOverflow);
            return;
        }

        if (double.IsNaN(dResult))
        {
            LogRuntimeException(errorNaN);
            return;
        }

        SystemStack.Push(new RealVar(dResult));
    }

    #endregion
}