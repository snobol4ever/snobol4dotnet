namespace Snobol4.Common;

//"eval argument is not expression" /* 103 */,

public partial class Executive
{
    public void Eval(List<Var> arguments)
    {
        var arg = arguments[0];

        // A NAME evaluates to the value of what it names, as SPITBOL's expression for the name does; the name of an element of an
        // array or table, or of a keyword, has no text and is no expression (103)
        if (arg is NameVar nameVar)
        {
            if (!nameVar.HasText)
            {
                LogRuntimeException(103);
                return;
            }

            SystemStack.Push(NameReturnValue(nameVar));
            return;
        }

        switch (arg)
        {
            case IntegerVar integerVar:
                SystemStack.Push(new IntegerVar(integerVar.Data));
                MarkValueEvaluated();
                return;

            case RealVar realVar:
                SystemStack.Push(new RealVar(realVar.Data));
                MarkValueEvaluated();
                return;

            case StringVar stringVar:
                // a text that ends in a colon or a semicolon is no expression at all, whatever comes before: run-time error 103, as in
                // SPITBOL, which looks at the last character before it compiles anything (EVAL('4 ;'), EVAL('(4 :'))
                if (stringVar.Data.Length > 0 && stringVar.Data[^1] is ':' or ';')
                {
                    LogRuntimeException(103);
                    return;
                }

                // the text is one expression: a comma outside every bracket and quote is not part of one (223), a compile error
                // like any other (counted while &ERRLIMIT is above 0, taken by an armed SETEXIT handler)
                if (HasTopLevelComma(stringVar.Data))
                {
                    AmpErrorType = 223;
                    AmpErrorText = CompilerException.ErrorMessage[223];
                    CompileErrorFailure();
                    return;
                }

                // a colon or a semicolon outside every bracket and quote ends the expression, which is then compiled alone: its own
                // compile error if it has one ('4 + ; ' is 221), else 103, "eval argument is not expression" ('4 :(L)'), a compile
                // error of the EVAL like any other
                var statementMark = TopLevelStatementMark(stringVar.Data);
                var expressionText = statementMark >= 0 ? stringVar.Data[..statementMark] : stringVar.Data;

                var previousCaseFolding = Parent.BuildOptions.CaseFolding;
                Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
                Parent.CodeMode = true;
                Parent.Code = new SourceCode(Parent);
                Parent.Code.ReadCodeInString($" *({expressionText.Trim()})", Parent.FilesToCompile[^1], oneLine: true);
                Parent.Code.SourceLines[0].DeferredExpression = true;
                // A string that does not compile makes EVAL fail with &ERRTYPE set to the compile error
                // (EVAL('1 +') is 221, an out-of-range real 231), printing nothing; with &ERRLIMIT above 0 the
                // error is counted and handled like a run-time one, as in SPITBOL (CompileErrorFailure)
                // before a colon or a semicolon only a syntax error is the text's own: EVAL('1 / 0 ; 5') is 103, not 14, as in SPITBOL
                Parent.SyntaxOnly = statementMark >= 0;
                bool compiled;
                try { compiled = CompileQuietly(() => { Parent.BuildEval(); return true; }); }
                finally { Parent.SyntaxOnly = false; }
                Parent.BuildOptions.CaseFolding = previousCaseFolding;
                if (!compiled)
                {
                    Parent.CodeMode = false;
                    CompileErrorFailure();
                    return;
                }

                if (statementMark >= 0)
                {
                    Parent.CodeMode = false;
                    AmpErrorType = 103;
                    AmpErrorText = CompilerException.ErrorMessage[103];
                    CompileErrorFailure();
                    return;
                }

                StarFunctionList[^1](this);
                Parent.CodeMode = false;
                MarkValueEvaluated();
                return;

            case ExpressionVar expression:
                expression.FunctionName(this);
                MarkValueEvaluated();
                return;

            default:
                LogRuntimeException(103);
                return;
        }
    }

    // EVAL called by name evaluates its expression by name: one that gives a value, no name, is error 249 wherever a name is required
    // (.EVAL('1 + 2'), LEN(1) . EVAL('1 + 2'), EVAL('1 + 2') = 4), as in SPITBOL, as $ of the name of a deferred expression is
    private void MarkValueEvaluated()
    {
        if (Failure || SystemStack.Count == 0)
            return;
        var result = SystemStack.Peek();
        if (result is NameVar || result.Symbol != "" || result.Key is not null)
            return;
        DeferredNameValue = result;
        DeferredNameValueError = 249;
    }

    internal static bool HasTopLevelComma(string text)
    {
        var depth = 0;
        for (var i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '\'' or '"':
                    var close = text.IndexOf(text[i], i + 1);
                    if (close < 0)
                        return false;
                    i = close;
                    break;
                case '(' or '[' or '<':
                    depth++;
                    break;
                case ')' or ']' or '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    return true;
            }
        }

        return false;
    }

    // Where a colon or a semicolon outside every bracket and quote ends the text's expression, or -1: a goto or the next statement,
    // which EVAL does not take
    internal static int TopLevelStatementMark(string text)
    {
        var depth = 0;
        for (var i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '\'' or '"':
                    var close = text.IndexOf(text[i], i + 1);
                    if (close < 0)
                        return -1;
                    i = close;
                    break;
                case '(' or '[' or '<':
                    depth++;
                    break;
                case ')' or ']' or '>':
                    depth--;
                    break;
                case ':' or ';' when depth == 0:
                    return i;
            }
        }

        return -1;
    }
}
