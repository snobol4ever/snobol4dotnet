using System.Text;

namespace Snobol4.Common;

//"function called by name returned a value" /* 21 */,
//"pattern replacement right operand is not a string" /* 31 */,
//"keyword value assigned is not integer" /* 208 */,
//"keyword in assignment is protected" /* 209 */,
//"keyword value assigned is negative or too large" /* 210 */,
//"value assigned to keyword errtext not a string" /* 211 */,
//"syntax error: value used where name is required" /* 212 */,

public partial class Executive
{
    public void _BinaryEquals()
    {
        using var profile1 = Profiler.Start3("_BinaryEquals", this);

        if (Parent.BuildOptions.TraceStatements)
            Console.Error.WriteLine(@"_BinaryEquals");
        // Do not delete. Used by DLL

        // Get all arguments and check for prior failure, into a list lent for this depth of assignment: an assignment can run another
        // (a trace function, a deferred target's call), and neither Assign nor ReplaceMatch keeps the list (it was a new List and its
        // array on every assignment)
        // A failure or an error before the assignment stops the statement's evaluation there, as in SPITBOL (OperatorFast does the same
        // for the other operators): an assignment inside the expression after it is not made, -A + (Y = 2) with A not numeric leaves Y
        // as it was. It was made, and the statement then ended on an empty stack
        if (Failure)
        {
            SystemStack.Pop();
            SystemStack.Pop();
            PushFailedValue();
            return;
        }

        if (_assignDepth == _assignArguments.Count)
            _assignArguments.Add(new List<Var>(2));
        var arguments = _assignArguments[_assignDepth++];
        try
        {
            if (SystemStack.ExtractArguments(2, arguments, this, 1))
            {
                return;
            }

            if (arguments[0] is SubjectVar)
            {
                ReplaceMatch(arguments);
            }
            else
            {
                Assign(arguments);
            }
        }
        finally
        {
            arguments.Clear();
            _assignDepth--;
        }
    }

    private readonly List<List<Var>> _assignArguments = [];
    private int _assignDepth;

    internal void ReplaceMatch(List<Var> arguments)
    {
        var subjectVar = (SubjectVar)arguments[0];

        // Replacement must resolve to a string
        if (!arguments[1].Convert(VarType.STRING, out _, out var subject, this))
        {
            LogRuntimeException(31);
            return;
        }

        // Perform the replacement
        var resultsVar = subjectVar.MatchReplace((string)subject);

        // an element subject is replaced in its array or table, as an assignment to it would be
        if (subjectVar is { Key: not null, Collection: ArrayVar or TableVar })
        {
            Assign([new StringVar("") { Key = subjectVar.Key, Collection = subjectVar.Collection, LvalueMark = StatementSerial },
                    new StringVar(resultsVar.Data)]);
            return;
        }

        // the subject keeps its associations: a replacement into OUTPUT writes the new text, as an assignment to it does
        var held = IdentifierTable[resultsVar.Symbol];
        resultsVar.OutputChannel = held.OutputChannel;
        resultsVar.InputChannel = held.InputChannel;
        var v = IdentifierTable[resultsVar.Symbol] = resultsVar;
        // a replacement assigns the subject, so its VALUE trace fires, as in SPITBOL
        TraceIdentifierValue(v.Symbol, v);
        SystemStack.Push(v);

        // Sync-step monitor fire-point: emit VALUE record on the destructive
        // subject rebind.  Mirrors csn ASGNVV (v311.sil:5938) and spl asign:asg01
        // (sbl.min:17596) — both fire on the chokepoint that handles
        // BOTH plain assignment AND destructive pattern-match replacement.
        // dot's two paths (Assign / ReplaceMatch) are split by the SubjectVar
        // dispatch in _BinaryEquals, so each must fire its own EmitValue.
        // Without this, beauty self-host wire diverges at case.inc:22 step 1497:
        // spl emits VALUE str='ND' after the destructive `=`; dot did not.
        // See GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan.
        if (MonitorIpc.Enabled && string.IsNullOrEmpty(v.OutputChannel))
            MonitorIpc.EmitValue(v.Symbol ?? "", v);

        if (v.OutputChannel != "" && AmpOutput != 0)
            WriteToOutputChannel(v);
    }

    internal void Assign(List<Var> arguments)
    {
        // A deferred target (. *X, $ *X) is evaluated for the name it gives; a variable that merely holds an
        // expression is itself the target, as in SPITBOL (V = *(X + 1) then V = *Y assigns V)
        while (arguments[0] is ExpressionVar { Symbol: "" } expressionVar1)
        {
            var depth = SystemStack.Count;
            expressionVar1.FunctionName(this);
            // If the deferred Assignee evaluation FRETURNed (e.g. *match(...)
            // used as the right-hand side of `$`), the function pushed a null
            // StringVar carrying the function's own name as Symbol.  Continuing
            // here would silently overwrite that variable with the captured
            // substring.  Bail out so the caller (typically
            // ImmediateVariableAssociation2.Scan) can propagate the failure to
            // the pattern matcher and trigger alternation backtracking.
            if (Failure)
            {
                // what the evaluation pushed, if it pushed anything: a call of an undefined function is an error that leaves nothing, and
                // taking another entry off the stack took the statement's own separator
                if (SystemStack.Count > depth)
                    SystemStack.Pop();
                return;
            }
            arguments[0] = SystemStack.Pop();

            // A deferred target is evaluated for a NAME, as SPITBOL evaluates it by name: an expression that can only give a
            // value (*'abc', *(X 'b')) is error 249 and a call that gave one (a RETURN, a builtin's value) error 21, once its side
            // effects are done. The error is the deferred evaluation's, so it fails the statement even with &ERRLIMIT at 0, as in
            // SPITBOL; the assignment failed with no error, and before that assigned the value's own variable or was error 212
            if (ValueExpressions.Contains(expressionVar1.FunctionName) || !IsName(arguments[0]))
            {
                ExpressionDepth++;
                try { LogRuntimeException(ValueExpressions.Contains(expressionVar1.FunctionName) ? 249 : 21); }
                finally { ExpressionDepth--; }
                return;
            }
        }

        var leftVar = arguments[0];  // Destination Var

        // A call in the place of a name (G() = V) that handed back a value by RETURN has nothing to assign through: error 21,
        // as in SPITBOL, where it assigned the function's own variable; so is a value $ of the name of a deferred expression gave
        if (ValueInNamesPlace(leftVar) is var valueError and not 0)
        {
            LogRuntimeException(valueError);
            return;
        }

        // A NAME is an indirect target only when this statement produced it (an NRETURN result); a
        // variable that merely holds a NAME is itself the target, as in SPITBOL (R = .ONE, then
        // R = R ' ' .TWO assigns R, not ONE)
        if (leftVar is NameVar heldName && heldName.Symbol != "" && heldName.LvalueMark != StatementSerial)
        {
            leftVar = new StringVar("", heldName.Symbol)
            {
                InputChannel = heldName.InputChannel,
                OutputChannel = heldName.OutputChannel
            };
        }
        var rightVar = arguments[1]; // Source Var

        // If source has input channels, get input now; at end of input the statement fails and nothing
        // is assigned (OUTPUT = INPUT writes no empty line, X keeps its value), as in SPITBOL
        rightVar = InputArgument(rightVar);
        if (Failure)
            return;

        //LeftPattern side must have a symbol, be keyed to a collection, or be a NameVar
        if (leftVar is { Symbol: "", Key: null } and not NameVar)
        {
            LogRuntimeException(leftVar.IsKeyword ? 211 : 212);
            return;
        }

        // Protected natural variables (arb, bal, fail, fence, rem, succeed) — error 42
        if (!leftVar.IsKeyword && leftVar.IsReadOnly)
        {
            LogRuntimeException(42);
            return;
        }

        // Special checks for keywords
        if (leftVar.IsKeyword)
        {
            // The value of the assignment is the value assigned, as it was given ((&ANCHOR = ' 1') is the string ' 1', (&ERRTEXT = 5)
            // the integer 5), as in SPITBOL; the keyword keeps the integer or the string it takes from it
            var assigned = rightVar;

            // &ERRTEXT takes a string, or whatever has one (211 for a value that has none)
            if (leftVar.Symbol == "&errtext")
            {
                if (!rightVar.Convert(VarType.STRING, out var errorText, out _, this))
                {
                    LogRuntimeException(211);
                    return;
                }

                HandleErrText(errorText, true);
                KeywordAssignmentValue(assigned);
                return;
            }

            // Otherwise, right side must convert to an integer: a real or a numeric string
            // does, truncated, as in SPITBOL (&TRIM = 1.0, &TRIM = '1.0')
            if (rightVar is not IntegerVar)
            {
                if (!rightVar.Convert(VarType.INTEGER, out var asInteger, out _, this))
                {
                    LogRuntimeException(208);
                    return;
                }
                rightVar = asInteger;
            }

            // The value of a keyword is 0 up to the biggest object (only &STLIMIT may be negative), else 210, and some keywords
            // limit it further: &ERRTYPE to 999, and assigning it signals that error; &PROFILE to 2; &MAXLNGTH to 1024 and more
            // (287); &FULLSCAN to not 0 (274)
            var number = ((IntegerVar)rightVar).Data;
            if (leftVar.Symbol != "&stlimit")
            {
                if (number < 0 || number > MaxObjectLength
                    || leftVar.Symbol == "&errtype" && number > 999
                    || leftVar.Symbol == "&profile" && number > 2)
                {
                    LogRuntimeException(210);
                    return;
                }

                if (leftVar.Symbol == "&maxlngth" && number < 1024)
                {
                    LogRuntimeException(287);
                    return;
                }

                if (leftVar.Symbol == "&fullscan" && number == 0)
                {
                    LogRuntimeException(274);
                    return;
                }

                if (leftVar.IsReadOnly)
                {
                    LogRuntimeException(209);
                    return;
                }

                if (leftVar.Symbol == "&errtype")
                {
                    // an immediate error is signaled: its own text for a number SPITBOL uses, &ERRTEXT for one it does not
                    LogRuntimeException(number == 0 ? 36 : (int)number, false, number);
                    return;
                }
            }

            if (KeywordTable.TryGetValue(leftVar.Symbol, out var handler))
            {
                handler(rightVar, true);
                KeywordAssignmentValue(assigned);
                // Sync-step monitor fire-point for keyword assignment.
                if (MonitorIpc.Enabled)
                    MonitorIpc.EmitValue(leftVar.Symbol ?? "", rightVar);
                return;
            }
        }

        // An array, table or DATA object is shared, and every index read marks it with that
        // element's Key and Collection. A mark left by an earlier statement does not make it this
        // statement's lvalue: the variable that holds it is the target (TN2 = T<N,2> on a later
        // pass wrote into the old element; Budne's atn printed one node's properties for every node).
        if (leftVar is ArrayVar or TableVar or ProgramDefinedDataVar && leftVar.Symbol != ""
            && leftVar.Collection is not null && leftVar.LvalueMark != StatementSerial)
        {
            leftVar.Key = null;
            leftVar.Collection = null;
        }

        switch (leftVar.Collection)
        {
            case ArrayVar arrayVar:
            {
                // Dereference NameVar rvalue before storing into the array slot.
                // If rightVar is a NameVar (e.g. returned via NRETURN from Shift/Push),
                // storing it directly and then stamping Key/Collection onto it creates a
                // self-referential cycle: arrayVar.Data[i] → NameVar(Collection=arrayVar, Key=i)
                // → arrayVar.Data[i] → ... indefinitely (ShiftReduce S-10 root cause).
                // The NAME of a variable is a value like another and stays a NAME in the element (L<1> = .F, then DATATYPE(L<1>) is NAME,
                // and $L<1> = 'x' sets F); only the NAME of an element, which has no pointer, is read through.
                var rval = rightVar is NameVar { Pointer: "" } rvNv ? rvNv.Dereference(this) : rightVar;
                // Clone rightVar before mutating Key/Collection so the source variable's
                // VarSlotArray slot (if rightVar came from PushVar) is not contaminated.
                var arrVal = rval is ArrayVar or TableVar ? Handle(rval) : StoredCopy(rval);
                arrVal.Key        = leftVar.Key;
                arrVal.Collection = leftVar.Collection;
                arrayVar.Data[(int)(long)leftVar.Key!] = arrVal;
                SystemStack.Push(arrVal);
                break;
            }

            case TableVar tableVar:
            {
                var rval = rightVar is NameVar { Pointer: "" } rvNv ? rvNv.Dereference(this) : rightVar;
                var tblVal = rval is ArrayVar or TableVar ? Handle(rval) : StoredCopy(rval);
                tblVal.Key        = leftVar.Key;
                tblVal.Collection = leftVar.Collection;
                tableVar.Data[leftVar.Key!] = tblVal;
                SystemStack.Push(tblVal);
                break;
            }

            default:
                // If the lvalue is a NameVar (e.g. returned via NRETURN), assign through
                // its Pointer — the actual variable being referenced — not leftVar.Symbol.
                var targetSymbol = leftVar is NameVar nameVar ? nameVar.Pointer : leftVar.Symbol;
                var newVar = rightVar is ArrayVar or TableVar ? Handle(rightVar) : StoredCopy(rightVar);
                newVar.Symbol = targetSymbol;
                // Strip array/table bookkeeping from the stored scalar value.
                // A value read from A<K> carries .Collection=arrayVar and .Key=k so
                // that Assign can write back through it when used as an lvalue.
                // When stored into a plain scalar variable (not a NameVar, which needs
                // its own Key/Collection to track its referent), those fields must be
                // cleared — otherwise a later use of the scalar as an lvalue routes
                // into the array write branch instead of the scalar IdentifierTable path.
                if (newVar is not NameVar)
                {
                    newVar.Key        = null;
                    newVar.Collection = null;
                }
                newVar.OutputChannel = leftVar.OutputChannel;
                newVar.InputChannel = leftVar.InputChannel;
                IdentifierTable[newVar.Symbol] = newVar;
                TraceIdentifierValue(newVar.Symbol, newVar);
                // the value of the assignment is the value assigned: reading it again must not read the variable's input
                // association (X = (INPUT = 'zz') is zz and reads nothing), as in SPITBOL
                if (!string.IsNullOrEmpty(newVar.InputChannel))
                {
                    var assigned = newVar.Clone();
                    assigned.Symbol = newVar.Symbol;
                    assigned.OutputChannel = newVar.OutputChannel;
                    assigned.InputChannel = "";
                    SystemStack.Push(assigned);
                }
                else
                {
                    SystemStack.Push(newVar);
                }

                break;
        }

        // Sync-step monitor fire-point: emit VALUE record on the just-stored value.
        // Mirrors csn ASGNVV (v311.sil:5938) and spl asign:asg01 (sbl.min:17596).
        // No-op when MonitorIpc is dormant.  See GOAL-NET-BEAUTY-SELF S-2-bridge-2.
        if (MonitorIpc.Enabled)
        {
            var stored = SystemStack.Peek();
            // Lvalue name extraction:
            //   * Scalar              → stored.Symbol (e.g. "S" for S = ...)
            //   * Array element a<i>  → stored.Symbol is empty, but stored.Collection
            //                            points at the ArrayVar; use its Symbol ("a").
            //   * Table slot d<'k'>   → same, use the TableVar's Symbol ("d").
            //   * Truly anonymous     → fall through to MonitorIpc.LvalueNameId's
            //                            <lval> sentinel.
            // S-2-bridge-7-lval (Mon Apr 28 2026) — this gives semantically-meaningful
            // names for aggregate-element stores so the wire identifies WHICH table or
            // array was written.  Both csn and spl bridges use <lval> here today; the
            // dot wire is strictly more informative.  See GOAL-NET-BEAUTY-SELF.
            string lvalueName = stored.Symbol ?? "";
            if (string.IsNullOrEmpty(lvalueName) && stored.Collection?.Symbol is { Length: > 0 } collSym)
            {
                lvalueName = collSym;
            }
            // Skip emission for I/O channel writes (OUTPUT, PUNCH, etc.).
            // spl's asign routes these through the trap chain (asg02/asg14)
            // rather than the natural-variable store path that fires sysmv,
            // so spl emits no VALUE record for `OUTPUT = expr`.  csn behaves
            // the same way: ASGNVV is bypassed for trapped variables.
            // Without this skip, dot emits a VALUE record at every OUTPUT
            // assignment and the wire diverges from spl on every line of
            // output-driving code (e.g. beauty.sno main loop step 1565).
            //
            // The body-assign emission used to be conditionally suppressed
            // when the lvalue matched the current function's return slot
            // (paired with a VALUE emit in Define.cs at RETURN time).  That
            // convention diverged from spl/csn on multiple body-assigns to
            // fn-name (e.g. icase loop) and on bare returns (e.g.
            // IDENT(str) :S(RETURN)).  Aligned to spl/csn convention:
            // VALUE fires at every body-assign chokepoint; RETURN carries
            // only the return type.  See spl runtime comment at
            // monitor_ipc_runtime.c:449-452 and
            // GOAL-NET-BEAUTY-SELF S-2-bridge-7-fullscan.
            if (string.IsNullOrEmpty(stored.OutputChannel))
                MonitorIpc.EmitValue(lvalueName, stored);
        }

        // &OUTPUT = 0 suspends output associations (the variable still takes its value), as in SPITBOL
        if (SystemStack.Peek().OutputChannel == "" || AmpOutput == 0)
        {
            return;
        }

        WriteToOutputChannel(SystemStack.Peek());
    }

    // The line an assignment to a variable with an output association writes
    internal void WriteToOutputChannel(Var outputVar)
    {
        // OUTPUT goes to stdout and TERMINAL to stderr, as in SPITBOL and CSNOBOL4
        switch (outputVar.OutputChannel)
        {
            case "+console-output":
                Console.Out.WriteLine(outputVar.ToString());
                break;

            case "+terminal-output":
                Console.Error.WriteLine(outputVar.ToString());
                break;

            case "+terminal-output-nnl":
                Console.Error.Write(outputVar.ToString());
                break;

            case "+console-output-nnl":
                Console.Out.Write(outputVar.ToString());
                break;
            
            default:
                StreamOutputs[outputVar.OutputChannel].Write(Encoding.UTF8.GetBytes(outputVar + Environment.NewLine));
                break;
        }
    }

    // An array or table is one object however many variables and elements hold it, so each holder keeps its own handle on it: the
    // same elements and identity, but its own name or place (L = V, then V = TABLE() assigns V and leaves L the array)
    internal static Var Handle(Var aggregate)
    {
        Var handle = aggregate is ArrayVar array ? array.Alias() : ((TableVar)aggregate).Alias();
        handle.IsReadOnly = false;
        handle.IsKeyword = false;
        return handle;
    }

    // The copy of a value an assignment stores: protection and keyword-ness belong to the variable the
    // value came from (FAIL, &TRIM), not to the value, so X = FAIL then X = 1 is an ordinary assignment
    // What a deferred target gave can be assigned through: a NAME, a variable, an element or a field, but not the value a
    // program-defined function handed back by RETURN, which carries the function's own variable
    internal bool IsName(Var target) =>
        ValueInNamesPlace(target) == 0 && (target is NameVar || target.Symbol != "" || target.Key is not null);

    // The keyword's handler pushed the keyword's own value; the assignment's value is the one assigned, under no name
    private void KeywordAssignmentValue(Var assigned)
    {
        if (Failure)
            return;
        SystemStack.Pop();
        SystemStack.Push(assigned.Unnamed());
    }

    private static Var StoredCopy(Var value)
    {
        var copy = value.Clone();
        copy.IsReadOnly = false;
        copy.IsKeyword = false;
        return copy;
    }
}
