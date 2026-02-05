namespace Snobol4.Common;

//"pattern match right operand is not pattern" /* 240 */,
//"pattern match left operand is not a string" /* 241 */,

public partial class Executive
{
    /// <summary>
    /// Match a string to a pattern. Successful matches are stored
    /// in a SubjectVar object with information about how to do a
    /// replacement.
    /// </summary>
    public void PatternMatch(List<Var> arguments)
    {
        // arguments[0]: Subject and left operand
        // arguments[1]: Pattern and right operand

        while (arguments[1] is ExpressionVar expressionVar1)
        {
            expressionVar1.FunctionName(this);
            arguments[1] = SystemStack.Pop();
        }

        // Pattern must be a pattern
        if (!arguments[1].Convert(VarType.PATTERN, out _, out var patternValue, this))
        {
            LogRuntimeException(240);
            return;
        }

        // Subject must resolve to a string
        if (!arguments[0].Convert(VarType.STRING, out var subject, out var subjectValue, this))
        {
            LogRuntimeException(241);
            return;
        }

        // If the subject is converted from something other than a string, it loses its symbol.
        // So preserve it here.
        subject.Symbol = arguments[0].Symbol;

        // Try the match
        //var anchor = ((IntegerVar)IdentifierTable["&anchor"]).Data;
        var anchor = Amp_Anchor;
        Scanner scanner = new(this);
        var mr = scanner.PatternMatch((string)subjectValue, (Pattern)patternValue, 0, anchor != 0);

        if (mr.Outcome != MatchResult.Status.SUCCESS)
        {
            NonExceptionFailure();
            return;
        }

        // Perform conditional assignments
        foreach (var nameListEntry in BetaStack.Reverse())
        {
            List<Var> assignment =
            [
                nameListEntry.Assignee,
                new StringVar(nameListEntry.Scan.Subject[nameListEntry.PreCursor..nameListEntry.PostCursor])
            ];

            Assign(assignment);
            SystemStack.Pop();
        }
        
        // Store object reference to save SubjectVar in a symbol table
        var subjectVar = new SubjectVar((string)subjectValue, mr)
        {
            Symbol = subject.Symbol
        };
        SystemStack.Push(subjectVar);
    }
}