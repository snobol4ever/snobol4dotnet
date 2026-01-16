namespace Snobol4.Common
{
    public partial class Executive
    {
        #region  Binary Comparison Helper Function

        private void BinaryComparison(List<Var> arguments, IntegerCompare integerCompare, RealCompare realCompare, int errorLeft, int errorRight)
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
                    dLeft = System.Convert.ToDouble(lRight);
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

        #region EQ

        internal void Eq(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerEq, RealEq, 101, 102);
        }

        internal bool IntegerEq(long left, long right)
        {
            return left == right;
        }

        internal bool RealEq(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left == right;
        }

        #endregion

        #region GE

        internal void Ge(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerGe, RealGe, 109, 110);
        }

        internal bool IntegerGe(long left, long right)
        {
            return left >= right;
        }

        internal bool RealGe(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left >= right;
        }

        #endregion

        #region GT

        internal void Gt(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerGt, RealGt, 111, 112);
        }

        internal bool IntegerGt(long left, long right)
        {
            return left > right;
        }

        internal bool RealGt(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left > right;
        }

        #endregion

        #region LE

        internal void Le(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerLe, RealLe, 121, 122);
        }

        internal bool IntegerLe(long left, long right)
        {
            return left <= right;
        }

        internal bool RealLe(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left <= right;
        }

        #endregion

        #region LT

        internal void Lt(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerLt, RealLt, 147, 148);
        }

        internal bool IntegerLt(long left, long right)
        {
            return left < right;
        }

        internal bool RealLt(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left < right;
        }

        #endregion

        #region NE

        internal void Ne(List<Var> arguments)
        {
            BinaryComparison(arguments, IntegerNe, RealNe, 149, 150);
        }

        internal bool IntegerNe(long left, long right)
        {
            return left != right;
        }

        internal bool RealNe(double left, double right)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return left != right;
        }

        #endregion

        #region Integer

        public void Integer(List<Var> arguments)
        {
            if (arguments[0].Convert(VarType.INTEGER, out _, out _, this))
            {
                PredicateSuccess();
                return;
            }

            NonExceptionFailure();
        }
        #endregion
    }
}