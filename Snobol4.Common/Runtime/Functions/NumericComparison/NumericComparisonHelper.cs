namespace Snobol4.Common;

public partial class Executive
{
    #region  Binary Comparison Helper Function

    private void BinaryComparison(List<Var> arguments, IntegerCompare integerCompare, RealCompare realCompare, int errorLeft, int errorRight)
    {
        if (!Var.ToNumeric(arguments[0], out var isIntegerLeft, out var lLeft, out var dLeft, this))
        {
            // If a deferred expression argument set Failure during evaluation,
            // propagate that failure normally rather than raising errorLeft.
            // SPITBOL: a failed *(...) yields a match failure, not a fatal
            // "first argument is not numeric" error.
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(errorLeft);
            return;
        }

        if (!Var.ToNumeric(arguments[1], out var isIntegerRight, out var lRight, out var dRight, this))
        {
            if (Failure) { NonExceptionFailure(); return; }
            LogRuntimeException(errorRight);
            return;
        }

        switch (isIntegerLeft)
        {
            case true when isIntegerRight:
            {
                if (integerCompare(lLeft, lRight))
                {
                    PredicateSuccess();
                    return;
                }

                NonExceptionFailure();
                return;
            }
            case true:
                dLeft = System.Convert.ToDouble(lLeft);
                break;
        }

        if (isIntegerRight)
            dRight = System.Convert.ToDouble(lRight);

        if (realCompare(dLeft, dRight))
        {
            PredicateSuccess();
            return;
        }

        NonExceptionFailure();
    }

    #endregion
}