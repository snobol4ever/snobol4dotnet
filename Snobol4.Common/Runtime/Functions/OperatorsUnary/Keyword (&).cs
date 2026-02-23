namespace Snobol4.Common;

// TODO make sure these are implemented
//"string length exceeds value of maxlngth keyword" /* 205 */
//"keyword value assigned is not integer" /* 208 */
//"keyword in assignment is protected" /* 209 */
//"keyword value assigned is negative or too large" /* 210 */
//"value assigned to keyword errtext not a string" /* 211 */
//"statement count exceeds value of stlimit keyword" /* 244 */
//"keyword operand is not name of defined keyword" /* 251 */
//"inconsistent value assigned to keyword profile" /* 268 */
//"value assigned to keyword fullscan is zero" /* 274 */
//"value assigned to keyword maxlngth is too small" /* 287 */

public partial class Executive
{
    internal delegate void KeywordHandler(Var value, bool set);
    internal Dictionary<string, KeywordHandler> KeywordTable = null!;

    // Protected keywords
    internal Pattern AmpAbortPattern = new AbortPattern();
    internal string AmpAlphabet;
    internal Pattern AmpArbPattern = new ArbPattern();
    internal Pattern AmpBalPattern = new BalPattern();
    internal Pattern AmpFailPattern = new FailPattern();
    internal Pattern AmpFencePattern = new AlternatePattern(new NullPattern(), new AbortPattern());
    internal long AmpFunctionLevel;
    internal int AmpLastLineNumber;
    internal string AmpLowerCaseLetters;
    internal int AmpCurrentLineNumber;
    internal Pattern AmpRemPattern = new RemPattern();
    internal string AmpReturnType;
    internal long AmpStatementCount;
    internal Pattern AmpSucceedPattern = new SucceedPattern();
    internal string AmpUpperCaseLetters;

    // Unprotected keywords
    internal long AmpAbnormalEnd;
    internal long AmpAnchor;
    internal long AmpCaseFolding = 1;
    public static int AmpCode;
    internal long AmpCompare;
    internal long AmpDump;
    internal long AmpErrorLimit;
    internal string AmpErrorText = "";
    internal long AmpErrorType;
    internal long AmpFunctionTrace;
    internal long AmpFullScan = 1;
    internal long AmpInput = 1;
    internal long AmpMaxLength = 4194304;
    internal string AmpOutput;
    internal long AmpProfile;
    internal long AmpStatementLimit = 2147483647;
    internal long AmpTrace;
    internal long AmpTrim;


    internal void Ampersand(List<Var> arguments)
    {
        var v = arguments[0];

        // Unary & operation requires a named variable
        if (v.Symbol == "")
        {
            LogRuntimeException(251);
            return;
        }

        // &operator must be existing keyword
        var newSymbol = "&" + v.Symbol;

        if (KeywordTable.TryGetValue(newSymbol, out var handler))
        {
            handler(v, false);
            return;
        }

        if (!IdentifierTable.TryGetValue(newSymbol, out var keywordVar))
        {
            LogRuntimeException(251);
            return;
        }

        SystemStack.Push(keywordVar);
    }

    // Helpers

    private bool ReadOnlyHandler(bool set)
    {
        if (!set) return false;
        LogRuntimeException(209);
        return true;
    }

    private bool IntegerHandler(Var value, out IntegerVar integerVar)
    {
        if (value is not IntegerVar iv)
        {
            integerVar = null!;
            return true;
        }

        integerVar = iv;
        return false;
    }

    private bool StringHandler(Var value, out StringVar stringVar)
    {
        if (value is not StringVar sv)
        {
            stringVar = null!;
            return true;
        }

        stringVar = sv;
        return false;
    }

    // Protected keywords

    internal void HandleAbort(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpAbortPattern, "&abort", true, true));
    }

    internal void HandleAlphabet(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(AmpAlphabet, "&alphabet", true, true));
    }

    internal void HandleArb(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpArbPattern, "&arb", true, true));
    }

    internal void HandleBal(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpBalPattern, "&bal", true, true));
    }

    internal void HandleFail(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpFailPattern, "&fail", true, true));
    }

    internal void HandleFence(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpFencePattern, "&fence", true, true));
    }

    internal void HandleFile(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        var fileName = Path.GetFileName(SourceFiles[AmpCurrentLineNumber - 1]);
        SystemStack.Push(new StringVar(fileName, "$file", true, true));
    }

    internal void HandleFncLevel(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(AmpFunctionLevel, "&fnclevel", true, true));
    }

    internal void HandleLastFile(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        var fileName = Path.GetFileName(SourceFiles[AmpLastLineNumber-1]);
        SystemStack.Push(new StringVar(fileName, "&lastfile", true, true));
    }

    internal void HandleLastNo(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(AmpLastLineNumber, "&lastno", true, true));
    }

    internal void HandleLastLine(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(SourceLineNumbers[AmpLastLineNumber - 1], "&lastline", true, true));
    }

    internal void HandleLCase(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(AmpLowerCaseLetters, "&lcase", true, true));
    }

    internal void HandleStNo(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(AmpCurrentLineNumber, "&line", true, true));
    }

    internal void HandleRem(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpRemPattern, "&rem", true, true));
    }

    internal void HandleRtnType(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(AmpReturnType, "&rtntype", true, true));
    }

    internal void HandleStCount(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(AmpStatementCount, "&stcount", true, true));
    }

    internal void HandleLine(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(SourceLineNumbers[AmpCurrentLineNumber - 1], "&stno", true, true));
    }

    internal void HandleSucceed(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new PatternVar(AmpSucceedPattern, "&succeed", true, true));
    }

    internal void HandleUCase(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(AmpUpperCaseLetters, "&ucase", true, true));
    }


    // Unprotected keywords

    internal void HandleAbend(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpAbnormalEnd = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpAbnormalEnd, "&abend", true));
    }

    internal void HandleAnchor(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpAnchor = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpAnchor, "&anchor", true));
    }

    internal void HandleCase(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpCaseFolding = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpCaseFolding, "&case", true));
    }

    internal void HandleCode(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpCode = (int)integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpCode, "&code", true));
    }

    internal void HandleCompare(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpCompare = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpCompare, "&compare", true));
    }

    internal void HandleDump(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpDump = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpDump, "&dump", true));
    }

    internal void HandleErrLimit(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpErrorLimit = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpErrorLimit, "&errlimit", true));
    }

    internal void HandleErrText(Var value, bool set)
    {
        if (set)
        {
            if (StringHandler(value, out var stringVar)) return;
            AmpErrorText = stringVar.Data;
        }

        SystemStack.Push(new StringVar(AmpErrorText, "&errtype", true));
    }

    internal void HandleErrType(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpErrorType = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpErrorType, "&errtype", true));
    }

    internal void HandleFTrace(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpFunctionTrace = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpFunctionTrace, "&ftrace", true));
    }

    internal void HandleFullScan(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpFullScan = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpFullScan, "&fullscan", true));
    }


    internal void HandleInput(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpInput = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpInput, "&input", true));
    }


    internal void HandleMaxLength(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpMaxLength = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpMaxLength, "&maxlngth", true));
    }

    internal void HandleOutput(Var value, bool set)
    {
        if (set)
        {
            if (StringHandler(value, out var stringVar)) return;
            AmpOutput = stringVar.Data;
        }

        SystemStack.Push(new StringVar(AmpOutput, "&output", true));
    }

    internal void HandleProfile(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpProfile = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpProfile, "&profile", true));
    }

    internal void HandleStatementLimit(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpStatementLimit = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpStatementLimit, "&stlimit", true));
    }

    internal void HandleTrace(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpTrace = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpTrace, "&trace", true));
    }

    internal void HandleTrim(Var value, bool set)
    {
        if (set)
        {
            if (IntegerHandler(value, out var integerVar)) return;
            AmpTrim = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpTrim, "&trim", true));
    }

}