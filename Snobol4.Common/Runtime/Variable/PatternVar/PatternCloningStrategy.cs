using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for pattern variables
/// Creates a deep copy of the pattern
/// </summary>
public class PatternCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        var patternSelf = (PatternVar)self;

        // Clone the underlying pattern structure
        return new PatternVar(patternSelf.Data.Clone())
        {
            Symbol = patternSelf.Symbol,
            InputChannel = patternSelf.InputChannel,
            OutputChannel = patternSelf.OutputChannel,
            Validation = patternSelf.Validation,
            IsReadOnly = patternSelf.IsReadOnly,
            IsKeyword = patternSelf.IsKeyword
        };
    }
}