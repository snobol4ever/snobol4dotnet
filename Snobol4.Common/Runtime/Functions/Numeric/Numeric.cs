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
        if (!Var.ToNumeric(arguments[0], out var isIntegerLeft, out var lLeft, out var dLeft, this))
        {
            LogRuntimeException(errorLeft);
            return;
        }

        if (!Var.ToNumeric(arguments[1], out var isIntegerRight, out var lRight, out var dRight, this))
        {
            LogRuntimeException(errorRight);
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
            }
        }

        if (isIntegerLeft)
            dLeft = lLeft;

        if (isIntegerRight)
            dRight = lRight;

        var dResult = realBinaryOperator(dLeft, dRight);

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
            if (double.IsNaN(dResult) || double.IsInfinity(dResult))
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

    #region Atan()

    internal void Atan(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Atan0, 301, 0, 0);
    }

    internal double Atan0(double dOperand)
    {
        return Math.Atan(dOperand);
    }

    #endregion

    #region Chop()

    internal void Chop(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Chop0, 302, 0, 0);
    }

    internal double Chop0(double dOperand)
    {
        return Math.Round(dOperand, MidpointRounding.ToZero);
    }

    #endregion

    #region Cos()

    internal void Cos(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Cos0, 303, 0, 0);
    }

    internal double Cos0(double dOperand)
    {
        return Math.Cos(dOperand);
    }

    #endregion

    #region Exp()

    internal void Exp(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Exp0, 304, 305, 0);
    }

    internal double Exp0(double dOperand)
    {
        return Math.Exp(dOperand);
    }

    #endregion

    #region Ln()

    internal void Ln(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Ln0, 306, 307, 315);
    }

    internal double Ln0(double dOperand)
    {
        return Math.Log(dOperand);
    }

    #endregion

    #region  Remdr()
    internal void Remainder(List<Var> arguments)
    {
        BinaryNumericOperation(arguments, IntegerRemainder, RealRemainder, 166, 165, 167, 312);
    }

    internal long IntegerRemainder(long left, long right)
    {
        return left % right;
    }

    internal double RealRemainder(double left, double right)
    {
        return left % right;
    }

    #endregion

    #region Sin()

    internal void Sin(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Sin0, 308, 0, 0);
    }

    internal double Sin0(double dOperand)
    {
        return Math.Sin(dOperand);
    }

    #endregion

    #region Sqrt()

    internal void Sqrt(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Sqrt0, 313, 314, 314);
    }

    internal double Sqrt0(double dOperand)
    {
        return Math.Sqrt(dOperand);
    }

    #endregion

    #region Tan()

    // ReSharper disable once UnusedMember.Global
    internal void Tan(List<Var> arguments)
    {
        UnaryNumericOperation(arguments, Tan0, 313, 312, 0);
    }

    internal double Tan0(double dOperand)
    {
        return Math.Tan(dOperand);
    }

    #endregion
}