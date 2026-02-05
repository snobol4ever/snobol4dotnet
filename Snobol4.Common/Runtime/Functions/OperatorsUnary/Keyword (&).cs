using System.Diagnostics.CodeAnalysis;

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
    
    internal long Amp_AbnormalEnd;
    internal Pattern Amp_Abort = new AbortPattern();
    internal string Amp_Alphabet;
    internal long Amp_Anchor = 0;
    internal Pattern Amp_Arb = new ArbPattern();
    internal Pattern Amp_Bal = new BalPattern();
    internal long Amp_Case = 1;
    internal long Amp_Code = 0;
    internal long Amp_Compare = 0;
    internal long Amp_Dump = 0;
    internal long Amp_ErrorLimit = 0;
    internal string Amp_Errtext = "";
    internal long Amp_ErrorType = 0;
    internal Pattern Amp_Fail = new FailPattern();
    internal Pattern Amp_Fence = new AlternatePattern(new NullPattern(), new AbortPattern());
    internal string Amp_File;
    internal long Amp_FunctionLevel;
    internal long Amp_FunctionTrace;
    internal long Amp_Fullscan = 1;
    internal long Amp_Input = 1;
    internal string Amp_LastFile;
    internal string Amp_LastLine;
    internal long Amp_LastNumber;
    internal string Amp_LowerCase;
    internal long Amp_Line;
    internal long Amp_Maxlength = 4194304;
    internal string Amp_Output;
    internal long Amp_Profile;
    internal Pattern Amp_Rem = new RemPattern();
    internal string Amp_ReturnType;
    internal long Amp_StatementCount;
    internal long Amp_StatementLimit = 22147483647;
    internal long Amp_StatementNumber;
    internal Pattern Amp_Succeed = new SucceedPattern();
    internal long Amp_Trace;
    internal long Amp_Trim;
    internal string Amp_UpperCase;

    internal Dictionary<string, KeywordHandler> KeywordTable = null!;

    internal void HandleAbend(Var value, bool set)
    {
        if (set)
        {
            if (value is not IntegerVar integerVar)
            {
                LogRuntimeException(208);
                return;
            }

            Amp_AbnormalEnd = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(Amp_AbnormalEnd, "&abend", true, false));
    }

    internal void HandleAbort(Var value, bool set)
    {
        if (set)
        {
            LogRuntimeException(209);
            return;
        }

        SystemStack.Push(new PatternVar(Amp_Abort, "&abort", true, true));
    }

    internal void HandleAlphabet(Var value, bool set)
    {
        if (set)
        {
            LogRuntimeException(209);
            return;
        }

        SystemStack.Push(new StringVar(Amp_Alphabet, "&alphabet", true,true));
    }

    internal void HandleAnchor(Var value, bool set)
    {
        if (set)
        {
            if (value is not IntegerVar integerVar)
            {
                LogRuntimeException(208);
                return;
            }

            Amp_Anchor = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(Amp_Anchor, "&anchor", true, false));
    }

    internal void HandleStatementLimit(Var value, bool set)
    {
        if (set)
        {
            if (value is not IntegerVar integerVar)
            {
                LogRuntimeException(208);
                return;
            }

            Amp_StatementLimit = integerVar.Data;
        }

        SystemStack.Push(new IntegerVar(Amp_StatementLimit, "&stlimit", true, false));
    }
    
    internal void HandleLowerCase(Var value, bool set)
    {
        if (set)
        {
            LogRuntimeException(209);
            return;
        }

        SystemStack.Push(new StringVar(Amp_LowerCase, "&lcase", true,true));
    }
    
    internal void HandleStatementCount(Var value, bool set)
    {
        if (set)
        {
            LogRuntimeException(209);
            return;
        }

        SystemStack.Push(new IntegerVar(Amp_StatementCount, "&stcount", true, true));
    }

    internal void HandleUpperCase(Var value, bool set)
    {
        if (set)
        {
            LogRuntimeException(209);
            return;
        }

        SystemStack.Push(new StringVar(Amp_UpperCase, "&ucase", true, true));
    }
    
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

        if (KeywordTable.TryGetValue(newSymbol, out KeywordHandler handler))
        {
            handler(arguments[0], false);
            return;
        }

        if (!IdentifierTable.TryGetValue(newSymbol, out var keywordVar))
        {
            LogRuntimeException(251);
            return;
        }

        SystemStack.Push(keywordVar);
    }
}