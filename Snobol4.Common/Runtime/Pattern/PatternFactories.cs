namespace Snobol4.Common;

public partial class Executive
{
    // Terminal pattern functions with string input
    // Name         Input can be null   Matches null
    // ANY(string)      N                   N
    // BREAK(string)    N                   Y
    // BREAKX(string)   N                   Y
    // NOTANY(string)   N                   N
    // SPAN(string)     N                   N

    #region Integer argument pattern helper

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
        if (arguments[0] is ExpressionVar expression)
        {
            SystemStack.Push(expression);
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var stringValue, this) || string.IsNullOrEmpty((string)stringValue))
        {
            LogRuntimeException(59);
            return;
        }

        SystemStack.Push(new PatternVar(new AnyPattern((string)stringValue)));
    }

    #endregion

    #region Create ARB Pattern ()

    // ReSharper disable once UnusedMember.Global
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
            SystemStack.Push(new PatternVar(new BreakPattern(expressionVar)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(69);
            return;
        }

        SystemStack.Push(new PatternVar(new BreakPattern((string)s)));
    }

    #endregion

    #region Create BREAKX Pattern (STRING)

    public void CreateBreakXPattern(List<Var> arguments)
    {
        var v0 = arguments[0];

        if (!v0.Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string?)s))
        {
            LogRuntimeException(45);
            return;
        }

        var arbNoArg = new ConcatenatePattern(new LenPattern(1), new BreakPattern((string)s));
        var arbNo = ArbNoPattern.Structure(arbNoArg);
        var breakX = new ConcatenatePattern(new BreakPattern((string)s), arbNo);
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
        var v0 = arguments[0];

        if (!v0.Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))  
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
        var v0 = arguments[0];

        if (!v0.Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
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