using System.Runtime.CompilerServices;

namespace Snobol4.Common;

/// <summary>
/// Cloning strategy for statement separator
/// Returns the singleton instance since statement separators are immutable
/// </summary>
public sealed class StatementSeparatorCloningStrategy : ICloningStrategy
{

    public Var Clone(Var self)
    {
        // Return the singleton instance - statement separators are immutable markers
        return StatementSeparator.Instance;
    }
}