    namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for subject variables
/// Creates a copy of the subject variable with the same match result
/// </summary>
public class SubjectCloningStrategy : ICloningStrategy
{
    public Var Clone(Var self)
    {
        var subjectSelf = (SubjectVar)self;
        
        return new SubjectVar(subjectSelf.Subject, subjectSelf.MatchResult)
        {
            Symbol = subjectSelf.Symbol,
            InputChannel = subjectSelf.InputChannel,
            OutputChannel = subjectSelf.OutputChannel
        };
    }
}