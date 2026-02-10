namespace Snobol4.Common;

public class SubjectFormattingStrategy : IFormattingStrategy
{
    public string ToString(Var self)
    {
        var subjectSelf = (SubjectVar)self;
        return subjectSelf.Subject;
    }

    public string DumpString(Var self)
    {
        var subjectSelf = (SubjectVar)self;
        
        // Optimized: Calculate length once and use Substring with explicit length
        var matchStart = subjectSelf.MatchResult.PreCursor;
        var matchLength = subjectSelf.MatchResult.PostCursor - matchStart;
        var matchedPortion = subjectSelf.Subject.Substring(matchStart, matchLength);
        
        // Optimized: Use string interpolation which is more efficient than concat
        return $"'{subjectSelf.Subject}' [matched: '{matchedPortion}']";
    }

    public string DebugVar(Var self)
    {
        var subjectSelf = (SubjectVar)self;
        
        // Optimized: Inline the ternary and use direct property access
        var symbolDisplay = subjectSelf.Symbol.Length == 0 ? "<no name>" : subjectSelf.Symbol;
        
        return $"SUBJECT Symbol: {symbolDisplay}  Subject: '{subjectSelf.Subject}'  MatchStart: {subjectSelf.MatchResult.PreCursor}  MatchEnd: {subjectSelf.MatchResult.PostCursor}  Succeeded: {subjectSelf.Succeeded}";
    }
}