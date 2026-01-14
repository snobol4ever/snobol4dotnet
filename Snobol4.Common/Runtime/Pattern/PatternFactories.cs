using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snobol4.Common;

public partial class Executive
{
    #region Integer argument pattern helper

    /// <summary>
    /// Converts and validates a variable argument as a non-negative integer for pattern functions.
    /// </summary>
    /// <param name="argument">The variable to convert to an integer</param>
    /// <param name="notNumeric">Error code to log if argument is not numeric</param>
    /// <param name="outOfRange">Error code to log if integer is negative or too large</param>
    /// <returns>
    /// The integer value if valid (0 to int.MaxValue),
    /// or 0 if conversion fails or value is out of range
    /// </returns>
    /// <remarks>
    /// <para>
    /// This helper method is used by pattern functions that require integer arguments
    /// (LEN, POS, RPOS, TAB, RTAB). It ensures the argument is:
    /// - Convertible to an integer type
    /// - Non-negative (>= 0)
    /// - Within int.MaxValue range
    /// </para>
    /// <para>
    /// If validation fails, a runtime exception is logged and 0 is returned.
    /// The calling pattern factory will typically continue with the value 0,
    /// which may result in a valid but potentially unexpected pattern behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid usage
    /// var length = GetInteger(arguments[0], 120, 121);
    /// // If arguments[0] = 10, returns 10
    /// // If arguments[0] = -5, logs error 121, returns 0
    /// // If arguments[0] = "test", logs error 120, returns 0
    /// </code>
    /// </example>
    internal int GetInteger(Var argument, int notNumeric, int outOfRange)
    {
        if (!argument.Convert(VarType.INTEGER, out _, out var n, this))
        {
            LogRuntimeException(notNumeric);
            return 0;
        }

        var l = (long)n;

        if (l is >= 0 and <= int.MaxValue)
            return (int)l;

        LogRuntimeException(outOfRange);
        return 0;
    }

    #endregion

    #region Create ANY Pattern (STRING)

    public void CreateAnyPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new AnyPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(59);
            return;
        }

        SystemStack.Push(new PatternVar(new AnyPattern((string)s)));
    }

    #endregion

    #region Create ARB Pattern ()

    public void CreateArbPattern(List<Var> _)
    {
        SystemStack.Push(new PatternVar(ArbPattern.Structure()));
    }

    #endregion

    #region Create ARBNO Pattern (PATTERN)

    public void CreateArbNoPattern(List<Var> arguments)
    {
        if (!arguments[0].Convert(VarType.PATTERN, out _, out var pattern, this))
        {
            LogRuntimeException(70);
            return;
        }

        SystemStack.Push(new PatternVar(ArbNoPattern.Structure((Pattern)pattern)));
    }

    #endregion

    #region Create BREAK Pattern (STRING)

    public void CreateBreakPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new BreakPattern(expressionVar.FunctionName, 69)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(69);
            return;
        }

        SystemStack.Push(new PatternVar(new BreakPattern((string)s, 69)));
    }

    #endregion

    #region Create BREAKX Pattern (STRING)

    public void CreateBreakXPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            var arbNoArg0 = new ConcatenatePattern(new LenPattern(1), new BreakPattern(expressionVar.FunctionName, 45));
            var arbNo0 = ArbNoPattern.Structure(arbNoArg0);
            var breakX0 = new ConcatenatePattern(new BreakPattern(expressionVar.FunctionName, 45), arbNo0);
            SystemStack.Push(new PatternVar(breakX0));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(45);
            return;
        }

        var arbNoArg = new ConcatenatePattern(new LenPattern(1), new BreakPattern((string)s, 45));
        var arbNo = ArbNoPattern.Structure(arbNoArg);
        var breakX = new ConcatenatePattern(new BreakPattern((string)s, 45), arbNo);
        SystemStack.Push(new PatternVar(breakX));
    }

    #endregion

    #region Create FENCE function (PATTERN)

    public void CreateFenceFunction(List<Var> arguments)
    {
        var v0 = arguments[0];

        if (!v0.Convert(VarType.PATTERN, out _, out var p, this))
        {
            LogRuntimeException(259);
            return;
        }

        SystemStack.Push(new PatternVar(new AlternatePattern((Pattern)p, new AbortPattern())));
    }

    #endregion

    #region Create LEN Pattern (INTEGER)

    public void CreateLenPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 120, 121);
        SystemStack.Push(new PatternVar(new LenPattern(i)));
    }

    #endregion

    #region Create NOTANY Pattern (STRING)

    public void CreateNotAnyPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new NotAnyPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(151);
            return;
        }

        SystemStack.Push(new PatternVar(new NotAnyPattern((string)s)));
    }

    #endregion

    #region Create POS Pattern (INTEGER)

    public void CreatePosPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 162, 163);
        SystemStack.Push(new PatternVar(new PosPattern(i)));
    }

    #endregion

    #region Create RPOS Pattern (STRING)

    public void CreateRPosPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 185, 186);
        SystemStack.Push(new PatternVar(new RPosPattern(i)));
    }

    #endregion

    #region Create RTAB Pattern (INTEGER)

    public void CreateRTabPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 181, 182);
        SystemStack.Push(new PatternVar(new RTabPattern(i)));
    }

    #endregion

    #region Create SPAN Pattern (STRING)

    public void CreateSpanPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new SpanPattern(expressionVar.FunctionName)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(188);
            return;
        }

        SystemStack.Push(new PatternVar(new SpanPattern((string)s)));

    }

    #endregion

    #region Create TAB Pattern (INTEGER)

    public void CreateTabPattern(List<Var> arguments)
    {
        var i = GetInteger(arguments[0], 183, 184);
        SystemStack.Push(new PatternVar(new TabPattern(i)));
    }

    #endregion
}