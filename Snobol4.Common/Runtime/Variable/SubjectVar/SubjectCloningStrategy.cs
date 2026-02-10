    namespace Snobol4.Common;

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