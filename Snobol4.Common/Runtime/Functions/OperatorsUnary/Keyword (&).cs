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
    internal Pattern AmpArbPattern = ArbPattern.Structure();
    internal Pattern AmpBalPattern = BalPattern.Structure();
    internal Pattern AmpFailPattern = new FailPattern();
    internal Pattern AmpFencePattern = new AlternatePattern(new NullPattern(), new AbortPattern());
    internal long AmpFunctionLevel;
    internal int AmpLastLineNumber = -1;    // index of the previous statement executed; -1 before the first
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
    internal long AmpMaxLength = 16777216;
    internal long MaxObjectLength = 16777216;   // sbl's mxlen: the -m value, which no &MAXLNGTH and no small-integer argument exceeds
    internal long AmpOutput = 1;   // 0 suspends every output association, as in SPITBOL
    internal long AmpProfile;

    // SPITBOL counts the statements from the one that sets &STLIMIT, not from the start of the run: the limit-th statement
    // after it is error 244, unless the limit is below the count at the time it is set, when it is never reached. The error is like
    // any other while &ERRLIMIT allows it, twice at most: &STCOUNT becomes the limit that was passed, the limit goes up by ten and
    // the budget starts again at the new limit; the third time, or with &ERRLIMIT at 0, it ends the run
    private long _statementLimit = 2147483647;
    internal long AmpStatementLimit
    {
        get => _statementLimit;
        set
        {
            _statementLimit = value;
            StatementLimitArmed = value >= AmpStatementCount;
            StatementsToLimit = value;
            StatementLimitHits = 0;
        }
    }

    internal bool StatementLimitArmed = true;
    internal long StatementsToLimit = 2147483647;
    internal int StatementLimitHits;

    internal enum StatementLimit { Within, Passed, PassedForGood }

    // called as each statement starts, before it is counted: Passed when it is the statement the limit is passed at and the run goes
    // on (the statement fails), PassedForGood when it ends the run. The report and the dump then show the count as the limit, and
    // the limit is ten higher, as SPITBOL leaves them
    internal StatementLimit StatementLimitReached()
    {
        if (AmpStatementLimit <= 0 || !StatementLimitArmed || --StatementsToLimit > 0)
            return StatementLimit.Within;
        AmpStatementCount = AmpStatementLimit;
        _statementLimit += 10;
        StatementsToLimit = _statementLimit;
        return ++StatementLimitHits >= 3 || AmpErrorLimit <= 0 ? StatementLimit.PassedForGood : StatementLimit.Passed;
    }
    internal long AmpTrace;
    internal long AmpTrim = 1;   // sbl x64 starts &TRIM at 1 (kvtrm), not the manual's 0


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

        // names are spelled as written when case folding is off, and SPITBOL's keywords are the upper-case names only (the table
        // holds the folded, lower-case ones too)
        if (!Parent.BuildOptions.CaseFolding && v.Symbol.Any(char.IsLower))
        {
            LogRuntimeException(251);
            return;
        }

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

    // The keywords whose values never change, which SPITBOL's compiler takes as constants: an operator or a pre-evaluated function
    // given one is evaluated as the statement is compiled, and an error in it refuses the statement (StatementChecker)
    private static readonly HashSet<string> ConstantKeywords =
        ["ABORT", "ALPHABET", "ARB", "BAL", "FAIL", "FENCE", "LCASE", "REM", "SUCCEED", "UCASE"];

    // The value of &symbol when the symbol, as Ampersand reads it, names one of those keywords; else null
    internal Var? ConstantKeyword(string symbol)
    {
        if (!ConstantKeywords.Contains(symbol.ToUpperInvariant()) || !Parent.BuildOptions.CaseFolding && symbol.Any(char.IsLower)
            || !KeywordTable.TryGetValue("&" + symbol, out var handler))
            return null;
        handler(StringVar.Null(), false);
        return SystemStack.Pop();
    }

    // The predefined pattern variables are constants to SPITBOL's compiler too, holding their keywords' patterns (SIZE(ARB) is 189 as the
    // statement is compiled); assigned through $'REM', the variable changes and the compiled code does not see it
    private static readonly HashSet<string> ConstantPatternVariables = ["ABORT", "ARB", "BAL", "FAIL", "FENCE", "REM", "SUCCEED"];

    // The value of the variable when the symbol, as the program spells it, names one of those; else null
    internal Var? ConstantVariable(string symbol) =>
        ConstantPatternVariables.Contains(symbol.ToUpperInvariant()) && (Parent.BuildOptions.CaseFolding || !symbol.Any(char.IsLower))
            ? ConstantKeyword(symbol)
            : null;

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

    // A statement index (AmpCurrentLineNumber, AmpLastLineNumber) read as SPITBOL numbers it:
    // &STNO counts statements from 1, a blank line being a null statement; &LINE is the line in its
    // file. Index -1 (no previous statement yet) reads 0 and the null string.
    private long StatementNumber(int index) => index >= 0 && index < SourceStno.Count ? SourceStno[index] : 0;

    private long StatementLine(int index) => index >= 0 && index < SourceLineNumbers.Count ? SourceLineNumbers[index] + 1 : 0;

    private string StatementFile(int index) =>
        index >= 0 && index < SourceFiles.Count ? SourceFiles[index] : "";

    internal void HandleFile(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(StatementFile(AmpCurrentLineNumber), "$file", true, true));
    }

    internal void HandleFncLevel(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(AmpFunctionLevel, "&fnclevel", true, true));
    }

    internal void HandleLastFile(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(StatementFile(AmpLastLineNumber), "&lastfile", true, true));
    }

    internal void HandleLastNo(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(StatementNumber(AmpLastLineNumber), "&lastno", true, true));
    }

    internal void HandleLastLine(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(StatementLine(AmpLastLineNumber), "&lastline", true, true));
    }

    internal void HandleLCase(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new StringVar(AmpLowerCaseLetters, "&lcase", true, true));
    }

    internal void HandleStNo(Var value, bool set)
    {
        if (ReadOnlyHandler(set)) return;
        SystemStack.Push(new IntegerVar(StatementNumber(AmpCurrentLineNumber), "&stno", true, true));
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
        SystemStack.Push(new IntegerVar(StatementLine(AmpCurrentLineNumber), "&line", true, true));
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
            // the names the run reads from strings fold as &CASE says from now on, as in SPITBOL: APPLY('f') calls F, $'abc' reads
            // ABC and DEFINE('g()') defines G once &CASE is 1, under -f too (FoldCase reads this)
            Parent.BuildOptions.CaseFolding = AmpCaseFolding != 0;
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

        SystemStack.Push(new StringVar(AmpErrorText, "&errtext", true));
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
            if (IntegerHandler(value, out var integerVar)) return;
            AmpOutput = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(AmpOutput, "&output", true));
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