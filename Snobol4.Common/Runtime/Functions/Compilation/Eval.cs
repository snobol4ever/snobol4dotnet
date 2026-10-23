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
                return;

            case RealVar realVar:
                SystemStack.Push(new RealVar(realVar.Data));
                return;

            case StringVar stringVar:
                // the text is one expression: a comma outside every bracket and quote is not part of one (223), a compile error
                // like any other (counted while &ERRLIMIT is above 0, taken by an armed SETEXIT handler)
                if (HasTopLevelComma(stringVar.Data))
                {
                    AmpErrorType = 223;
                    AmpErrorText = CompilerException.ErrorMessage[223];
                    CompileErrorFailure();
                    return;
                }

                var previousCaseFolding = Parent.BuildOptions.CaseFolding;
                Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
                Parent.CodeMode = true;
                Parent.Code = new SourceCode(Parent);
                Parent.Code.ReadCodeInString($" *({stringVar.Data.Trim()})", Parent.FilesToCompile[^1]);
                Parent.Code.SourceLines[0].DeferredExpression = true;
                // A string that does not compile makes EVAL fail with &ERRTYPE set to the compile error
                // (EVAL('1 +') is 221, an out-of-range real 231), printing nothing; with &ERRLIMIT above 0 the
                // error is counted and handled like a run-time one, as in SPITBOL (CompileErrorFailure)
                var compiled = CompileQuietly(() => { Parent.BuildEval(); return true; });
                Parent.BuildOptions.CaseFolding = previousCaseFolding;
                if (!compiled)
                {
                    Parent.CodeMode = false;
                    CompileErrorFailure();
                    return;
                }

                StarFunctionList[^1](this);
                Parent.CodeMode = false;
                return;

            case ExpressionVar expression:
                expression.FunctionName(this);
                return;

            default:
                LogRuntimeException(103);
                return;
        }
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
}
