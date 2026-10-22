namespace Snobol4.Common;

//"goto operand in direct goto is not code" /* 24 */,

public partial class Executive
{
    public void CreateCode(List<Var> arguments)
    {
        // SPITBOL's gtcod: a CODE is itself; a string is compiled; so is the text of a number or of a NAME (CODE(7) is the statement
        // whose label is 7, CODE(.S) compiles S, the name, and not what S holds); anything else fails
        if (arguments[0] is not (CodeVar or StringVar or IntegerVar or RealVar or NameVar))
        {
            NonExceptionFailure();
            return;
        }

        var previousCaseFolding = Parent.BuildOptions.CaseFolding;
        Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
        Parent.CodeMode = true;
        var converted = arguments[0].Convert(VarType.CODE, out var code, out _, this);
        Parent.BuildOptions.CaseFolding = previousCaseFolding;
        Parent.CodeMode = false;

        if (!converted)
        {
            NonExceptionFailure();
            return;
        }

        SystemStack.Push(code);
    }

    // Compiles CODE text onto the end of the program. Text that does not compile gives null, with &ERRTYPE
    // and &ERRTEXT set to the compile error and nothing printed, as in SPITBOL (CODE(' X = (') fails, 226)
    internal CodeVar? CompileCodeText(string text)
    {
        CodeVar code = new()
        {
            StatementNumber = Parent.StatementCount,
            Data = text
        };

        Parent.Code = new SourceCode(Parent);
        Parent.Code.ReadCodeInString(text, Parent.FilesToCompile[^1]);
        Parent.FirstErrorSource = null;
        var numbered = SourceStno.Count;
        if (CompileQuietly(Parent.BuildCode))
            return code;

        // The builder numbers a block in full even when a statement is in error; SPITBOL numbers only the
        // statements before it, and no implicit end, so the next block numbers on from there
        // (CODE(' B = 1 ; Y = (') takes one number, CODE(' Y = ( ; B = 1') none)
        var good = Parent.FirstErrorSource is { } erring ? Math.Max(0, Parent.Code.SourceLines.IndexOf(erring)) : 0;
        var last = good > 0 ? SourceStno[numbered + good - 1] : numbered > 0 ? SourceStno[numbered - 1] : 0;
        for (var k = numbered + good; k < SourceStno.Count; k++)
            SourceStno[k] = last;
        return null;
    }

    // Runs a compile for EVAL or CODE with its diagnostics unprinted. A compile error is no run-time error:
    // it is taken back out of the histories and left in &ERRTYPE and &ERRTEXT, and the compile fails
    internal bool CompileQuietly(Func<bool> build)
    {
        var errorsBefore = Parent.ErrorCodeHistory.Count;
        bool built;
        Parent.QuietCompile = true;
        try { built = build(); }
        finally { Parent.QuietCompile = false; }

        if (Parent.ErrorCodeHistory.Count == errorsBefore)
            return built;

        var code = Parent.ErrorCodeHistory[^1];
        var added = Parent.ErrorCodeHistory.Count - errorsBefore;
        Parent.ErrorCodeHistory.RemoveRange(errorsBefore, added);
        Parent.ColumnHistory.RemoveRange(Parent.ColumnHistory.Count - added, added);
        Parent.MessageHistory.RemoveRange(Parent.MessageHistory.Count - added, added);
        AmpErrorType = code;
        AmpErrorText = CompilerException.ErrorMessage[code];
        return false;
    }
}
