namespace Snobol4.Common;

public partial class Executive
{
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
        if (arguments[0] is ExpressionVar expressionVar0)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar0.FunctionName));
        }

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
            // an argument evaluated when the pattern is matched is error 44 if it is not a string
            SystemStack.Push(new PatternVar(new BreakPattern(expressionVar.FunctionName, 44)));
            return;
        }

        if (!arguments[0].Convert(VarType.STRING, out _, out var s, this) || string.IsNullOrEmpty((string)s))
        {
            LogRuntimeException(69);
            return;
        }

        SystemStack.Push(new PatternVar(new BreakPattern((string)s, 44)));
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
            LogRuntimeException(70);
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
        if (arguments[0] is ExpressionVar expressionVar0)
        {
            arguments[0] = new PatternVar(UnevaluatedPattern.Structure(expressionVar0.FunctionName));
        }

        if (!arguments[0].Convert(VarType.PATTERN, out _, out var p, this))
        {
            LogRuntimeException(259);
            return;
        }

        // FENCE(p): try p; if p fails → backtrack normally (no abort here);
        // if p succeeds → seal pops alternates down to the entry mark.
        // FENCE(P) structure: ConcatenatePattern(Mark, ConcatenatePattern(p, Seal))
        // MarkPattern.Scan pushes a -3 sentinel at FENCE entry to capture the
        // outer alt-stack depth.  SealPattern.Scan pops back down through the
        // mark, then pushes -2.  On backtrack through -2, Match returns ABORT
        // outward — the entire match terminates without cursor-position retry,
        // matching Gimpel 1973 §"Pattern Theory": FENCE = NULL | ABORT.
        // P failure: p simply fails; alternates p saved are popped; mark is
        // skipped transparently by RestoreAlternate; outer alternates fire.
        SystemStack.Push(new PatternVar(
            new ConcatenatePattern(
                new MarkPattern(),
                new ConcatenatePattern((Pattern)p, new SealPattern()))));
    }

    #endregion

    #region Create LEN Pattern (INTEGER)

    public void CreateLenPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new LenPattern(expressionVar.FunctionName)));
            return;
        }

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
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new PosPattern(expressionVar.FunctionName)));
            return;
        }

        var i = GetInteger(arguments[0], 162, 163);
        SystemStack.Push(new PatternVar(new PosPattern(i)));
    }

    #endregion

    #region Create RPOS Pattern (STRING)

    public void CreateRPosPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new RPosPattern(expressionVar.FunctionName)));
            return;
        }

        var i = GetInteger(arguments[0], 185, 186);
        SystemStack.Push(new PatternVar(new RPosPattern(i)));
    }

    #endregion

    #region Create RTAB Pattern (INTEGER)

    public void CreateRTabPattern(List<Var> arguments)
    {
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new RTabPattern(expressionVar.FunctionName)));
            return;
        }

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
        if (arguments[0] is ExpressionVar expressionVar)
        {
            SystemStack.Push(new PatternVar(new TabPattern(expressionVar.FunctionName)));
            return;
        }

        var i = GetInteger(arguments[0], 183, 184);
        SystemStack.Push(new PatternVar(new TabPattern(i)));
    }

    #endregion
}